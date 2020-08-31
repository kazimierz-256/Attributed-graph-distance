using System;
using System.Collections.Generic;
using System.Text;

namespace TemporalSubgraph
{
    public class BipartiteGraph<V, EA>
    {
        // valid descendant connections
        public Dictionary<V, HashSet<V>> potentialConnections = new Dictionary<V, HashSet<V>>();
        public Dictionary<V, HashSet<V>> potentialConnectionsReversed = new Dictionary<V, HashSet<V>>();
    }
}
