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
            var infinity = double.PositiveInfinity;

            var gVerticesKVP = G.Vertices.ToList();
            var hVerticesKVP = G.Vertices.ToList();

            // TODO: take into account preassignedVertices, definitely create smaller cost matrix
            switch (encodingMethod)
            {
                case GraphEncodingMethod.Wojciechowski:
                    {
                        var m = Math.Max(G.VertexCount, H.VertexCount);
                        costMatrix = new double[m, m];
                        var maximumAssignment = double.NegativeInfinity;

                        Func<Graph<V, VA, EA>, Graph<V, VA, EA>, (V, V), (V, V), double> edgeRelabelRobust =
                        (graph1, graph2, e1, e2) =>
                        {
                            var g1Contains = graph1.ContainsEdge(e1);
                            var g2Contains = graph1.ContainsEdge(e2);
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
                                        var edgeCostArray = new double[m, m];


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

                                                edgeCostArray[w, gw] = cost;
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

                                                edgeCostArray[w, gw] = cost;
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

                                                edgeCostArray[w, gw] = cost;
                                            }
                                        }

                                        var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
                                        var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, localAssignment);
                                        costMatrix[v, fv] = localAssignmentCost;
                                    }
                                }

                                // fill in vertex adding (to G) costs
                                for (int v = G.VertexCount; v < m; v++)
                                {
                                    for (int fv = 0; fv < H.VertexCount; fv++)
                                    {
                                        // attribute of a vertex
                                        var fvAttribute = hVerticesKVP[fv].Value;

                                        // local cost matrix
                                        var edgeCostArray = new double[m, m];


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

                                                edgeCostArray[w, gw] = cost;
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

                                                edgeCostArray[w, gw] = cost;
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

                                                edgeCostArray[w, gw] = cost;
                                            }
                                        }

                                        var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
                                        var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, localAssignment);
                                        costMatrix[v, fv] = localAssignmentCost;
                                    }
                                }

                                // fill in vertex removal (from G) costs
                                for (int v = 0; v < G.VertexCount; v++)
                                {
                                    // attribute of a vertex
                                    var vAttribute = gVerticesKVP[v].Value;
                                    for (int fv = H.VertexCount; fv < m; fv++)
                                    {
                                        // local cost matrix
                                        var edgeCostArray = new double[m, m];


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

                                                edgeCostArray[w, gw] = cost;
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

                                                edgeCostArray[w, gw] = cost;
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

                                                edgeCostArray[w, gw] = cost;
                                            }
                                        }

                                        var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
                                        var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, localAssignment);
                                        costMatrix[v, fv] = localAssignmentCost;
                                    }
                                }


                                var abAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
                                var abAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, abAssignment);
                                if (abAssignmentCost > maximumAssignment)
                                {
                                    maximumAssignment = abAssignmentCost;
                                }
                            }
                        }
                        LowerBound = maximumAssignment;
                        // TODO: compute the upper bound cost from the approximate assignment
                        UpperBound = double.NaN;
                        break;
                    }
                case GraphEncodingMethod.RiesenBunke:
                    {
                        var m = G.VertexCount + H.VertexCount;
                        costMatrix = new double[m, m];

                        // TODO: assign LowerBound and UpperBound
                        break;
                    }
                default:
                    throw new Exception("Unknown graph encoding method");
            }
        }

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
    }
}
