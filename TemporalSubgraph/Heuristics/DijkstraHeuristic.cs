﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalSubgraph.Heuristics
{
    public class DijkstraHeuristic<V, EA> : IHeuristic<V, EA>
    {
        public double Compute(BipartiteGraph<V, EA> bipartitePossibilities)
        {
            // "dijkstra" simple upper bound heuristic
            return bipartitePossibilities.potentialConnections
                .Where((kvp) => kvp.Value.Count > 0)
                .Count();
        }
    }
}
