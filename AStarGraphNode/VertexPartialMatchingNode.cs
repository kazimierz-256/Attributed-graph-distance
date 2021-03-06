﻿using AStar;
using AttributedGraph;
using System;
using System.Collections.Generic;
using LinearAssignmentSolver;
using System.Linq;

namespace AStarGraphNode
{
    public class VertexPartialMatchingNode<V, VA, EA> : INode
    {
        public readonly Graph<V, VA, EA> G;
        public readonly Graph<V, VA, EA> H;
        private readonly List<(V, V)> preassignedVertices;

        List<INode> INode.Expand()
        {
            // expand along some unassigned vertex
            throw new NotImplementedException();
        }

        public double GetHeuristicValue() => BestLowerBound;

        public double DistanceFromSource()
        {
            throw new NotImplementedException();
        }

        public Dictionary<(double, double), double> abLowerBounds = new Dictionary<(double, double), double>();
        public Dictionary<(double, double), double> abUpperBounds = new Dictionary<(double, double), double>();

        public int[] optimalAssignment { get; private set; }
        public double BestLowerBoundA { get; private set; }
        public double BestLowerBoundB { get; private set; }
        public double BestUpperBoundA { get; private set; }
        public double BestUpperBoundB { get; private set; }

        public double BestUpperBound;
        public double BestLowerBound;

        public VertexPartialMatchingNode(
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                GraphMatchingParameters<V, VA, EA> matchingParameters
            )
        {
            this.G = G;
            this.H = H;
            this.preassignedVertices = matchingParameters.preassignedVertices;

            var cachedVertexAddingCosts = new Dictionary<VA, double>();
            var cachedVertexRemovingCosts = new Dictionary<VA, double>();
            var cachedVertexRelabellingCosts = new Dictionary<(VA, VA), double>();

            var cachedEdgeAddingCosts = new Dictionary<EA, double>();
            var cachedEdgeRemovingCosts = new Dictionary<EA, double>();
            var cachedEdgeRelabellingCosts = new Dictionary<(EA, EA), double>();

            // attempt to cache costs
            Func<VA, double> vertexAdd = (a) =>
            {
                if (!cachedVertexAddingCosts.ContainsKey(a))
                    cachedVertexAddingCosts.Add(a, matchingParameters.vertexAdd(a));
                return cachedVertexAddingCosts[a];
            };
            Func<VA, double> vertexRemove = (a) =>
            {
                if (!cachedVertexRemovingCosts.ContainsKey(a))
                    cachedVertexRemovingCosts.Add(a, matchingParameters.vertexRemove(a));
                return cachedVertexRemovingCosts[a];
            };
            Func<VA, VA, double> vertexRelabel = (a1, a2) =>
            {
                var key = (a1, a2);
                if (!cachedVertexRelabellingCosts.ContainsKey(key))
                    cachedVertexRelabellingCosts.Add(key, matchingParameters.vertexRelabel(a1, a2));
                return cachedVertexRelabellingCosts[key];
            };

            Func<EA, double> edgeAdd = (a) =>
            {
                if (!cachedEdgeAddingCosts.ContainsKey(a))
                    cachedEdgeAddingCosts.Add(a, matchingParameters.edgeAdd(a));
                return cachedEdgeAddingCosts[a];
            };
            Func<EA, double> edgeRemove = (a) =>
            {
                if (!cachedEdgeRemovingCosts.ContainsKey(a))
                    cachedEdgeRemovingCosts.Add(a, matchingParameters.edgeRemove(a));
                return cachedEdgeRemovingCosts[a];
            };
            Func<EA, EA, double> edgeRelabel = (a1, a2) =>
            {
                var key = (a1, a2);
                if (!cachedEdgeRelabellingCosts.ContainsKey(key))
                    cachedEdgeRelabellingCosts.Add(key, matchingParameters.edgeRelabel(a1, a2));
                return cachedEdgeRelabellingCosts[key];
            };

            // Matching part
            // TODO: compute the lower bound using LAP taking constraints into account

            double[,] costMatrix = null;

            var gVerticesKVP = G.Vertices.ToList();
            var hVerticesKVP = H.Vertices.ToList();


            BestLowerBound = double.NegativeInfinity;
            BestUpperBound = double.PositiveInfinity;
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
            var max = Math.Max(G.VertexCount, H.VertexCount);
            if (matchingParameters.encodingMethod == GraphEncodingMethod.Wojciechowski)
            {
                m = max;
                costMatrix = new double[m, m];
            }
            else if (matchingParameters.encodingMethod == GraphEncodingMethod.RiesenBunke2009)
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

            foreach (var a in matchingParameters.aCollection)
            {
                foreach (var b in matchingParameters.bCollection)
                {
                    // fill in vertex replacement costs
                    // v in G
                    for (int v = 0; v < G.VertexCount; v++)
                    {
                        // attribute of a vertex
                        var vAttribute = gVerticesKVP[v].Value;
                        // fv in H
                        for (int fv = 0; fv < H.VertexCount; fv++)
                        {
                            // attribute of a vertex
                            var fvAttribute = hVerticesKVP[fv].Value;

                            // local cost matrix
                            var localCostMatrix = new double[m, m];

                            // w in G
                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // attributes of edges in both directions
                                var vwEdge = (gVerticesKVP[v].Key, gVerticesKVP[w].Key);
                                var wvEdge = (gVerticesKVP[w].Key, gVerticesKVP[v].Key);

                                // gw in H
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

                                // gw outside H
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

                            // w outside G
                            for (int w = G.VertexCount; w < m; w++)
                            {
                                // gw in H
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
                                // gw outside H
                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexRelabel(vAttribute, fvAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        }
                    }

                    // fill in vertex adding (to G) costs
                    // v outside G
                    for (int v = G.VertexCount; v < m; v++)
                    {
                        // fv in H
                        Action<int> fvTask = fv =>
                        {
                            // attribute of a vertex
                            var fvAttribute = hVerticesKVP[fv].Value;

                            // local cost matrix
                            var localCostMatrix = new double[m, m];


                            // w in G
                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // gw in H
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

                                // gw outside H
                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexAdd(fvAttribute)
                                    + (1 - a) / m * vertexRemove(wAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            // w outside G
                            for (int w = G.VertexCount; w < m; w++)
                            {
                                // gw in H
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

                                // gw outside H
                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexAdd(fvAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        };

                        if (matchingParameters.encodingMethod == GraphEncodingMethod.Wojciechowski)
                            for (int fv = 0; fv < H.VertexCount; fv++)
                                fvTask(fv);
                        else
                            fvTask(v - G.VertexCount);
                    }

                    // fill in vertex removal (from G) costs
                    // v inside G
                    for (int v = 0; v < G.VertexCount; v++)
                    {
                        // attribute of a vertex
                        var vAttribute = gVerticesKVP[v].Value;
                        // fv outside H
                        Action<int> fvTask = fv =>
                        {
                            // local cost matrix
                            var localCostMatrix = new double[m, m];


                            // w in G
                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                // attribute of a vertex
                                var wAttribute = gVerticesKVP[w].Value;

                                // attributes of edges in both directions
                                var vwEdge = (gVerticesKVP[v].Key, gVerticesKVP[w].Key);
                                var wvEdge = (gVerticesKVP[w].Key, gVerticesKVP[v].Key);

                                // gw in H
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

                                // gw outside H
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

                            // w outside G
                            for (int w = G.VertexCount; w < m; w++)
                            {
                                // gw in H
                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    // attribute of a vertex
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var cost =
                                    a / m * vertexRemove(vAttribute)
                                    + (1 - a) / m * vertexAdd(gwAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }

                                // gw outside H
                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    a / m * vertexRemove(vAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        };

                        if (matchingParameters.encodingMethod == GraphEncodingMethod.Wojciechowski)
                            for (int fv = H.VertexCount; fv < m; fv++)
                                fvTask(fv);
                        else
                            fvTask(v + H.VertexCount);
                    }

                    // v outside G
                    for (int v = G.VertexCount; v < m; v++)
                    {
                        // fv outside  H
                        for (int fv = H.VertexCount; fv < m; fv++)
                        {
                            // local cost matrix
                            var localCostMatrix = new double[m, m];

                            // w in G
                            for (int w = 0; w < G.VertexCount; w++)
                            {
                                var wAttribute = gVerticesKVP[w].Value;

                                // gw in H
                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var cost =
                                    +(1 - a) / m * vertexRelabel(wAttribute, gwAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }

                                // gw outside H
                                for (int gw = H.VertexCount; gw < m; gw++)
                                {
                                    var cost =
                                    +(1 - a) / m * vertexRemove(wAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                            }

                            // w outside G
                            for (int w = G.VertexCount; w < m; w++)
                            {
                                // gw in H
                                for (int gw = 0; gw < H.VertexCount; gw++)
                                {
                                    var gwAttribute = hVerticesKVP[gw].Value;

                                    var cost =
                                    +(1 - a) / m * vertexAdd(gwAttribute);

                                    localCostMatrix[w, gw] = cost;
                                }
                                // gw outside H (zeros)
                            }

                            var localAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(localCostMatrix);
                            var localAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(localCostMatrix, localAssignment);
                            costMatrix[v, fv] = localAssignmentCost;
                        }
                    }

                    // lower bound estimate
                    var abAssignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
                    var abAssignmentCost = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, abAssignment);
                    if (abAssignmentCost > BestLowerBound)
                    {
                        BestLowerBound = abAssignmentCost;
                        bestLowerBoundAssignment = abAssignment;
                        BestLowerBoundA = a;
                        BestLowerBoundB = b;
                        optimalAssignment = abAssignment;
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
                            if (fw < H.VertexCount && fv < H.VertexCount)
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
                    if (realAssignmentCost < BestUpperBound)
                    {
                        BestUpperBound = realAssignmentCost;
                        bestUpperBoundAssignment = abAssignment;
                        BestUpperBoundA = a;
                        BestUpperBoundB = b;
                    }

                    abLowerBounds.Add((a, b), abAssignmentCost);
                    abUpperBounds.Add((a, b), realAssignmentCost);
                }
            }
        }
    }
}
