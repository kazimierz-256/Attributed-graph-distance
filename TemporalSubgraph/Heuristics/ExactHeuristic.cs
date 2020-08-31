using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalSubgraph.Heuristics
{
    public class ExactHeuristic<V, EA> : IHeuristic<V, EA>
    {
        private readonly Func<V> vertexGenerator;

        public ExactHeuristic(Func<V> vertexGenerator)
        {
            this.vertexGenerator = vertexGenerator;
        }
        public double Compute(BipartiteGraph<V, EA> bipartitePossibilities)
        {
            var setA = new List<V>();
            var setB = new List<V>();

            var edges = new List<Edge<V>>();

            foreach (var left in bipartitePossibilities.potentialConnections)
                foreach (var right in left.Value)
                    edges.Add(new Edge<V>(left.Key, right));

            setA.AddRange(bipartitePossibilities.potentialConnections.Keys);
            setB.AddRange(bipartitePossibilities.potentialConnectionsReversed.Keys);

            var graph = new AdjacencyGraph<V, Edge<V>>(true);
            graph.AddVerticesAndEdgeRange(edges);

            VertexFactory<V> vertexFactory = () => vertexGenerator();
            EdgeFactory<V, Edge<V>> edgeFactory = (source, target) => new Edge<V>(source, target);
            var maxMatch = new MaximumBipartiteMatchingAlgorithm<V, Edge<V>>(graph,
                setA, setB, vertexFactory, edgeFactory);

            maxMatch.Compute();

            return maxMatch.MatchedEdges.Count;
        }
    }
}
