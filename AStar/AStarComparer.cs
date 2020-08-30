using AlgorithmInternalBenchmark;
using System.Collections.Generic;

namespace AStar
{
    public class AStarComparer<T> : IComparer<T> where T : INode
    {
        public AStarComparer(Benchmark<string> benchmark = null)
        {
            Benchmark = benchmark;
        }

        public Benchmark<string> Benchmark { get; set; }

        public int Compare(T x, T y)
        {
            Benchmark?.StartBenchmark("AStarComparer.Compare");
            var priority1 = (x.DistanceFromSource() + x.GetHeuristicValue()).CompareTo(y.DistanceFromSource() + y.GetHeuristicValue());
            if (priority1 != 0)
            {
                Benchmark?.StopBenchmark("AStarComparer.Compare");
                return priority1;
            }

            var priority2 = x.DistanceFromSource().CompareTo(y.DistanceFromSource());
            Benchmark?.StopBenchmark("AStarComparer.Compare");
            return priority2;
        }
    }
}