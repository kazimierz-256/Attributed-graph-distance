using System;

namespace TemporalSubgraph
{
    public interface IHeuristic<V, EA>
    {
        double Compute(BipartiteGraph<V, EA> bipartitePossibilities);
    }
}