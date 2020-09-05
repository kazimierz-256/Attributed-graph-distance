using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalSubgraph.Heuristics
{
    public class TrivialHeuristic<V, EA> : IHeuristic<V, EA>
    {
        public string Name => "Trivial";

        public double Compute(BipartiteGraph<V, EA> bipartitePossibilities)
        {
            // "dijkstra" simple upper bound heuristic
            return Math.Min(

                bipartitePossibilities.potentialConnections
                .Where((kvp) => kvp.Value.Count > 0)
                .Count(),

                bipartitePossibilities.potentialConnectionsReversed
                .Where((kvp) => kvp.Value.Count > 0)
                .Count()

                );
        }
    }
}
