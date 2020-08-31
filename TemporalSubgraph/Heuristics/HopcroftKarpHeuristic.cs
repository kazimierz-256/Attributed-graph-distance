using System;
using System.Collections.Generic;
using System.Text;

namespace TemporalSubgraph.Heuristics
{
    public class HopcroftKarpHeuristic<V, EA> : IHeuristic<V, EA>
    {
        public double Compute(BipartiteGraph<V, EA> bipartitePossibilities)
        {
            return 0;
            //var graph = new Advanced.Algorithms.DataStructures.Graph.AdjacencyList.Graph<V>();

            //foreach (var vertex in bipartitePossibilities.potentialConnectionsReversed.Keys)
            //    graph.AddVertex(vertex);
            //foreach (var vertex in bipartitePossibilities.potentialConnections.Keys)
            //    graph.AddVertex(vertex);

            //if (graph.VerticesCount == 0)
            //    return 0;

            //foreach (var vertexConnections in bipartitePossibilities.potentialConnections)
            //    foreach (var vertexTo in vertexConnections.Value)
            //        graph.AddEdge(vertexConnections.Key, vertexTo);

            //foreach (var vertexConnections in bipartitePossibilities.potentialConnectionsReversed)
            //    foreach (var vertexFrom in vertexConnections.Value)
            //        if (!graph.HasEdge(vertexFrom, vertexConnections.Key))
            //            throw new Exception("Invalid graph");

            //var algorithm = new QuickGraph.Algorithms.MaximumBipartiteMatchingAlgorithm<>();
        }
    }
}
