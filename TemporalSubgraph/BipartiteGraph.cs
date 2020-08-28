using System;
using System.Collections.Generic;
using System.Text;

namespace TemporalSubgraph
{
    public class BipartiteGraph<V, EA>
    {
        // valid descendant connections
        public SortedDictionary<V, SortedSet<V>> potentialConnections = new SortedDictionary<V, SortedSet<V>>();

        //public SortedDictionary<EA, EA> potentialEdgeMatchings = new SortedDictionary<EA, EA>();

        // warning: this is slow and should be removed
        internal BipartiteGraph<V, EA> Clone()
        {
            return new BipartiteGraph<V, EA>()
            {
                potentialConnections = new SortedDictionary<V, SortedSet<V>>(potentialConnections),
                //potentialEdgeMatchings = new SortedDictionary<EA, EA>(potentialEdgeMatchings)
            };
        }
    }
}
