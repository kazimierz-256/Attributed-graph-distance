using AStar;
using System;
using System.Collections.Generic;
using System.Text;
using AttributedGraph;
using System.Linq;

namespace TemporalSubgraph
{
    public class TemporalMatchingNode<V, VA, EA> : INode where EA : IComparable
    {
        private Graph<V, VA, EA> graph1;
        private Graph<V, VA, EA> graph2;

        // partial matching M
        private Dictionary<V, V> partialMatchings = new Dictionary<V, V>();
        // unmatched nodes U1
        private HashSet<V> U1 = new HashSet<V>();
        // unmatched nodes U2
        private HashSet<V> U2 = new HashSet<V>();

        private HashSet<(V, V)> bipartiteValidConnections = new HashSet<(V, V)>();

        public TemporalMatchingNode(Graph<V, VA, EA> graph1, Graph<V, VA, EA> graph2)
        {
            this.graph1 = graph1;
            this.graph2 = graph2;

            U1.UnionWith(graph1.Vertices.Keys);
            U2.UnionWith(graph2.Vertices.Keys);

            throw new NotImplementedException("Still needs to setup the bipartite graph");
        }

        private TemporalMatchingNode(
            Graph<V, VA, EA> graph1,
            Graph<V, VA, EA> graph2,
            HashSet<V> U1,
            HashSet<V> U2,
            Dictionary<V, V> partialMatchings,
            HashSet<(V, V)> bipartiteValidConnections)
        {

        }

        private HashSet<(V, V)> RestrictBipartiteGraph(
            HashSet<V> descendantU1,
            HashSet<V> descendantU2,
            HashSet<(V, V)> bipartiteValidConnections,
            Dictionary<V, V> descendantMatchings
            )
        {
            // find an upper bound for bipartite matching between U1 and U2
            throw new NotImplementedException();
        }

        public List<INode> Expand()
        {
            if (U1.Count == 0 || U2.Count == 0)
                return new List<INode>();

            var descendants = new List<INode>();

            // does not need to be from U1, does not need to be random
            var candidate1 = U1.First();

            // try matching some node from u1 with nodes from u2
            foreach (var candidate2 in U2)
            {
                if (bipartiteValidConnections.Contains((candidate1, candidate2)))
                {
                    var descendantU1 = new HashSet<V>(U1);
                    var descendantU2 = new HashSet<V>(U2);
                    var descendantMatchings = new Dictionary<V, V>(partialMatchings);
                    descendantU1.Remove(candidate1);
                    descendantU2.Remove(candidate2);
                    // verify the matching is existant and valid!
                    descendantMatchings.Add(candidate1, candidate2);
                    var newBipartiteValidConnections = RestrictBipartiteGraph(
                        descendantU1,
                        descendantU2,
                        bipartiteValidConnections,
                        descendantMatchings
                        );
                    var descendant = new TemporalMatchingNode<V, VA, EA>(
                        graph1,
                        graph2,
                        descendantU1,
                        descendantU2,
                        descendantMatchings,
                        newBipartiteValidConnections
                        );
                    descendants.Add(descendant);
                }
            }

            // and try removing that node from U1
            {
                // should most probably go with immutable data structures
                var descendantU1 = new HashSet<V>(U1);
                var descendantU2 = new HashSet<V>(U2);
                var descendantMatchings = new Dictionary<V, V>(partialMatchings);
                descendantU1.Remove(candidate1);
                var newBipartiteValidConnections = RestrictBipartiteGraph(
                    descendantU1,
                    descendantU2,
                    bipartiteValidConnections,
                    descendantMatchings
                    );
                var descendant = new TemporalMatchingNode<V, VA, EA>(
                    graph1,
                    graph2,
                    descendantU1,
                    descendantU2,
                    descendantMatchings,
                    newBipartiteValidConnections
                    );
                descendants.Add(descendant);
            }

            return descendants;
        }

        public double GetHeuristicValue()
        {
            throw new NotImplementedException("We need a cached bipartite solution provider");
        }

        public double DistanceFromSource()
        {
            return -partialMatchings.Count;
        }

        public V Matching(V vertex1)
        {
            return partialMatchings[vertex1];
        }
    }
}
