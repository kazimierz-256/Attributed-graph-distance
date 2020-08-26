using AStar;
using System;
using System.Collections.Generic;
using System.Text;
using AttributedGraph;

namespace TemporalSubgraph
{
    public class TemporalMatchingNode<V, VA, EA> : INode where EA : IComparable
    {
        private Graph<V, VA, EA> graph1;
        private Graph<V, VA, EA> graph2;

        // partial matching M
        private HashSet<(V, V)> partialMatchings = new HashSet<(V, V)>();
        // unmatched nodes U1
        private HashSet<V> U1 = new HashSet<V>();
        private HashSet<V> U2 = new HashSet<V>();
        // unmatched nodes U2

        public TemporalMatchingNode(Graph<V, VA, EA> graph1, Graph<V, VA, EA> graph2)
        {
            this.graph1 = graph1;
            this.graph2 = graph2;

            U1.UnionWith(graph1.Vertices.Keys);
            U2.UnionWith(graph2.Vertices.Keys);

            BuildBipartiteGraph();
        }

        private void BuildBipartiteGraph()
        {
            // find an upper bound for bipartite matching between U1 and U2


        }

        public List<INode> Expand()
        {
            throw new NotImplementedException();
        }

        public double GetHeuristicValue()
        {
            throw new NotImplementedException();
        }

        public double DistanceFromSource()
        {
            throw new NotImplementedException();
        }
    }
}
