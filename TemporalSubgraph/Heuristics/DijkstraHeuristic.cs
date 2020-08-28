using System;
using System.Collections.Generic;
using System.Text;

namespace TemporalSubgraph.Heuristics
{
    class DijkstraHeuristic<V, EA> : IHeuristic<V, EA>
    {
        public double Compute(BipartiteGraph<V, EA> bipartitePossibilities) => 0d;
    }
}
