using AStar;
using RandomGraphProvider;
using System;
using TemporalSubgraph;

namespace TemporalSubgraphBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random(3_14159265);
            double vertexAttributeGenerator() => random.NextDouble();
            double edgeAttributeGenerator() => random.NextDouble();
            var nodes = 10;
            var density = .5d;
            var randomTemporalGraph1 = RandomGraphFactory.GenerateRandomInstance(nodes, density, true, vertexAttributeGenerator, edgeAttributeGenerator);
            var randomTemporalGraph2 = RandomGraphFactory.GenerateRandomInstance(nodes, density, true, vertexAttributeGenerator, edgeAttributeGenerator);
            var initialNode = new TemporalMatchingNode<int, double, double>(randomTemporalGraph1, randomTemporalGraph2);
            var aStarAlgorithm = new AStarAlgorithm<TemporalMatchingNode<int, double, double>>(initialNode);

            while (aStarAlgorithm.Queue.Count > 0)
            {
                aStarAlgorithm.ExpandBestNode();
                var bestNode = aStarAlgorithm.BestNode;
            }
        }
    }
}
