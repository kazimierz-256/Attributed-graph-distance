using System;

namespace TemporalSubgraph
{
    public interface IHeuristic<V, EA>
    {
        string Name { get; }

        double Compute(BipartiteGraph<V, EA> bipartitePossibilities);
    }
}