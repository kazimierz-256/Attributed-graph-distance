using AStar;
using AttributedGraph;
using System;
using System.Collections.Generic;
using LinearAssignmentSolver;
using System.Linq;

namespace AStarGraphNode
{
    public class VertexPartialMatchingNode<V, VA, EA> : INode
    {
        private readonly Graph<V, VA, EA> G;
        private readonly Graph<V, VA, EA> H;
        private readonly List<(V, V)> preassignedVertices;

        public int CompareTo(object obj)
        {
            var node = (INode)obj;
            if (node == null)
                throw new Exception("Incompatible node type");
            return this.LowerBound.CompareTo(node.LowerBound);
        }
        List<INode> INode.Expand()
        {
            // expand along some unassigned vertex
            throw new NotImplementedException();
        }

        public Dictionary<(double, double), double> abLowerBounds = new Dictionary<(double, double), double>();
        public Dictionary<(double, double), double> abUpperBounds = new Dictionary<(double, double), double>();

        public double LowerBound
        {
            get;
            private set;
        }

        public double UpperBound
        {
            get;
            private set;
        }

        public double BestLowerBoundA { get; private set; }
        public double BestLowerBoundB { get; private set; }
        public double BestUpperBoundA { get; private set; }
        public double BestUpperBoundB { get; private set; }
        public VertexPartialMatchingNode(
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                Func<VA, double> vertexAdd,
                Func<VA, VA, double> vertexRelabel,
                Func<VA, double> vertexRemove,
                Func<EA, double> edgeAdd,
                Func<EA, EA, double> edgeRelabel,
                Func<EA, double> edgeRemove,
                ICollection<double> aCollection,
                ICollection<double> bCollection,
                List<(V, V)> preassignedVertices = default,
                GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski
            )
        {
            this.G = G;
            this.H = H;
            this.preassignedVertices = preassignedVertices;

            // Matching part
            // TODO: build a cached matrix of all possible attributes for easy retrieval and modification
            // taking into account already assigned vertices

            // TODO: compute the lower bound using LAP taking constraints into account

            double[,] costMatrix = null;

            var gVerticesKVP = G.Vertices.ToList();
            var hVerticesKVP = H.Vertices.ToList();


            var bestLowerBound = double.NegativeInfinity;
            var bestUpperBound = double.PositiveInfinity;
            int[] bestLowerBoundAssignment;
            int[] bestUpperBoundAssignment;

            Func<Graph<V, VA, EA>, Graph<V, VA, EA>, (V, V), (V, V), double> edgeRelabelRobust =
            (graph1, graph2, e1, e2) =>
            {
                var g1Contains = graph1.ContainsEdge(e1);
                var g2Contains = graph2.ContainsEdge(e2);
                if (g1Contains)
                {
                    if (g2Contains)
                        return edgeRelabel(graph1[e1], graph2[e2]);
                    else
                        return edgeRemove(graph1[e1]);
                }
                else if (g2Contains)
                {
                    return edgeAdd(graph2[e2]);
                }
                else
                {
                    return 0;
                }
            };

            Func<Graph<V, VA, EA>, (V, V), double> edgeAddRobust =
            (graph, e) =>
            {
                if (graph.ContainsEdge(e))
                    return edgeAdd(graph[e]);
                else
                    return 0;
            };

            Func<Graph<V, VA, EA>, (V, V), double> edgeRemoveRobust =
            (graph, e) =>
            {
                if (graph.ContainsEdge(e))
                    return edgeRemove(graph[e]);
                else
                    return 0;
            };

            // TODO: take into account preassignedVertices, definitely create smaller cost matrix
            int m = 0;
            if (encodingMethod == GraphEncodingMethod.Wojciechowski)
            {
                m = Math.Max(G.VertexCount, H.VertexCount);
                costMatrix = new double[m, m];
            }
            else if (encodingMethod == GraphEncodingMethod.RiesenBunke)
            {
                m = G.VertexCount + H.VertexCount;
                costMatrix = new double[m, m];
                var infinity = double.PositiveInfinity;
                for (int v = G.VertexCount; v < m; v++)
                    for (int fv = 0; fv < H.VertexCount; fv++)
                        costMatrix[v, fv] = infinity;
                for (int fv = H.VertexCount; fv < m; fv++)
                    for (int v = 0; v < G.VertexCount; v++)
                        costMatrix[v, fv] = infinity;
            }

            foreach (var a in aCollection)
            {
                foreach (var b in bCollection)
                {
                    // fill in vertex replacement costs
                    for (int v = 0; v < G.VertexCount; v++)
                    {
                        // attribute of a vertex
                        var vAttribute = gVerticesKVP[v].Value;
                        for (int fv = 0; fv < H.VertexCount; fv++)
                        {
                            // attribute of a vertex
                            var fvAttribute = hVerticesKVP[fv].Value;

                            // local cost matrix
                            var localCostMatrix = new double[m, m];


                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // attributes of edges in both directions
                                var vwEdge = (gVerticesKVP[v].Key, gVerticesKVP[w].Key);
                                var wvEdge = (gVerticesKVP[w].Key, gVerticesKVP[v].Key);

                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var fvgwEdge = (hVerticesKVP[fv].Key, hVerticesKVP[gw].Key);
                                    var gwfvEdge = (hVerticesKVP[gw].Key, hVerticesKVP[fv].Key);

                                    var cost =
                                    a / m * vertexRelabel(vAttribute, fvAttribute)
                                    + (1 - a) / m * vertexRelabel(wAttribute, gwAttribute)
                                    + b * edgeRelabelRobust(G, H, vwEdge, fvgwEdge)
                                    + (1 - b) * edgeRelabelRobust(G, H, wvEdge, gwfvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            for (int w = G.VertexCount; w < m; w++)
                            {
                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var fvgwEdge = (hVerticesKVP[fv].Key, hVerticesKVP[gw].Key);
                                    var gwfvEdge = (hVerticesKVP[gw].Key, hVerticesKVP[fv].Key);

                                    var cost =
                                    a / m * vertexRelabel(vAttribute, fvAttribute)
                                    + (1 - a) / m * vertexAdd(gwAttribute)
                                    + b * edgeAddRobust(H, fvgwEdge)
                                    + (1 - b) * edgeAddRobust(H, gwfvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }


                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // attributes of edges in both directions
                                var vwEdge = (gVerticesKVP[v].Key, gVerticesKVP[w].Key);
                                var wvEdge = (gVerticesKVP[w].Key, gVerticesKVP[v].Key);

                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexRelabel(vAttribute, fvAttribute)
                                    + (1 - a) / m * vertexRemove(wAttribute)
                                    + b * edgeRemoveRobust(G, vwEdge)
                                    + (1 - b) * edgeRemoveRobust(G, wvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        }
                    }

                    // fill in vertex adding (to G) costs
                    for (int v = G.VertexCount; v < m; v++)
                    {
                        Action<int> fvTask = fv =>
                        {
                            // attribute of a vertex
                            var fvAttribute = hVerticesKVP[fv].Value;

                            // local cost matrix
                            var localCostMatrix = new double[m, m];


                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var fvgwEdge = (hVerticesKVP[fv].Key, hVerticesKVP[gw].Key);
                                    var gwfvEdge = (hVerticesKVP[gw].Key, hVerticesKVP[fv].Key);

                                    var cost =
                                    a / m * vertexAdd(fvAttribute)
                                    + (1 - a) / m * vertexRelabel(wAttribute, gwAttribute)
                                    + b * edgeAddRobust(H, fvgwEdge)
                                    + (1 - b) * edgeAddRobust(H, gwfvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            for (int w = G.VertexCount; w < m; w++)
                            {
                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var fvgwEdge = (hVerticesKVP[fv].Key, hVerticesKVP[gw].Key);
                                    var gwfvEdge = (hVerticesKVP[gw].Key, hVerticesKVP[fv].Key);

                                    var cost =
                                    a / m * vertexAdd(fvAttribute)
                                    + (1 - a) / m * vertexAdd(gwAttribute)
                                    + b * edgeAddRobust(H, fvgwEdge)
                                    + (1 - b) * edgeAddRobust(H, gwfvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }


                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexAdd(fvAttribute)
                                    + (1 - a) / m * vertexRemove(wAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        };

                        if (encodingMethod == GraphEncodingMethod.Wojciechowski)
                            for (int fv = 0; fv < H.VertexCount; fv++)
                                fvTask(fv);
                        else
                            fvTask(v - G.VertexCount);
                    }

                    // fill in vertex removal (from G) costs
                    for (int v = 0; v < G.VertexCount; v++)
                    {
                        // attribute of a vertex
                        var vAttribute = gVerticesKVP[v].Value;
                        Action<int> fvTask = fv =>
                        {
                            // local cost matrix
                            var localCostMatrix = new double[m, m];


                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // attributes of edges in both directions
                                var vwEdge = (gVerticesKVP[v].Key, gVerticesKVP[w].Key);
                                var wvEdge = (gVerticesKVP[w].Key, gVerticesKVP[v].Key);

                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var cost =
                                    a / m * vertexRemove(vAttribute)
                                    + (1 - a) / m * vertexRelabel(wAttribute, gwAttribute)
                                    + b * edgeRemoveRobust(G, vwEdge)
                                    + (1 - b) * edgeRemoveRobust(G, wvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            for (int w = G.VertexCount; w < m; w++)
                            {
                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var cost =
                                    a / m * vertexRemove(vAttribute)
                                    + (1 - a) / m * vertexAdd(gwAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }


                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // attributes of edges in both directions
                                var vwEdge = (gVerticesKVP[v].Key, gVerticesKVP[w].Key);
                                var wvEdge = (gVerticesKVP[w].Key, gVerticesKVP[v].Key);

                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexRemove(vAttribute)
                                    + (1 - a) / m * vertexRemove(wAttribute)
                                    + b * edgeRemoveRobust(G, vwEdge)
                                    + (1 - b) * edgeRemoveRobust(G, wvEdge);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        };

                        if (encodingMethod == GraphEncodingMethod.Wojciechowski)
                            for (int fv = H.VertexCount; fv < m; fv++)
                                fvTask(fv);
                        else
                            fvTask(v + H.VertexCount);
                    }

                    // lower bound estimate
                    var abAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
                    var abAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, abAssignment);
                    if (abAssignmentCost > bestLowerBound)
                    {
                        bestLowerBound = abAssignmentCost;
                        bestLowerBoundAssignment = abAssignment;
                        BestLowerBoundA = a;
                        BestLowerBoundB = b;
                        LowerBound = bestLowerBound;
                    }

                    // upper bound estimate
                    var realAssignmentCost = 0d;
                    for (int v = 0; v < G.VertexCount; v++)
                    {
                        var fv = abAssignment[v];
                        if (fv < H.VertexCount)
                        {
                            realAssignmentCost += vertexRelabel(gVerticesKVP[v].Value, hVerticesKVP[fv].Value);
                        }
                        else
                        {
                            realAssignmentCost += vertexAdd(gVerticesKVP[v].Value);

                        }
                        for (int w = 0; w < G.VertexCount; w++)
                        {
                            var fw = abAssignment[w];
                            if (fw < H.VertexCount)
                            {
                                if (fv < hVerticesKVP.Count)
                                {
                                    realAssignmentCost += edgeRelabelRobust(
                                        G,
                                        H,
                                        (gVerticesKVP[v].Key, gVerticesKVP[w].Key),
                                        (hVerticesKVP[fv].Key, hVerticesKVP[fw].Key)
                                    );
                                }
                                else
                                {
                                    realAssignmentCost += edgeRemoveRobust(
                                        G,
                                        (gVerticesKVP[v].Key, gVerticesKVP[w].Key)
                                    );
                                }
                            }
                            else
                            {
                                realAssignmentCost += edgeRemoveRobust(
                                    G,
                                    (gVerticesKVP[v].Key, gVerticesKVP[w].Key)
                                );
                            }
                        }
                        for (int w = G.VertexCount; w < m; w++)
                        {
                            var fw = abAssignment[w];
                            if (fw < H.VertexCount && fv < hVerticesKVP.Count)
                            {
                                realAssignmentCost += edgeAddRobust(
                                    H,
                                    (hVerticesKVP[fv].Key, hVerticesKVP[fw].Key)
                                );
                            }
                        }
                    }
                    for (int v = G.VertexCount; v < m; v++)
                    {
                        var fv = abAssignment[v];
                        if (fv < H.VertexCount)
                        {
                            realAssignmentCost += vertexRemove(hVerticesKVP[fv].Value);
                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                var fw = abAssignment[w];
                                if (fw < H.VertexCount)
                                {
                                    realAssignmentCost += edgeAddRobust(
                                        H,
                                        (hVerticesKVP[fv].Key, hVerticesKVP[fw].Key)
                                    );
                                }
                            }
                            for (int w = G.VertexCount; w < m; w++)
                            {
                                var fw = abAssignment[w];
                                if (fw < H.VertexCount)
                                {
                                    realAssignmentCost += edgeAddRobust(
                                        H,
                                        (hVerticesKVP[fv].Key, hVerticesKVP[fw].Key)
                                    );
                                }
                            }
                        }
                    }
                    if (realAssignmentCost < bestUpperBound)
                    {
                        bestUpperBound = realAssignmentCost;
                        bestUpperBoundAssignment = abAssignment;
                        BestUpperBoundA = a;
                        BestUpperBoundB = b;
                        UpperBound = bestUpperBound;
                    }

                    abLowerBounds.Add((a, b), abAssignmentCost);
                    abUpperBounds.Add((a, b), realAssignmentCost);
                }
            }
        }
    }
}
