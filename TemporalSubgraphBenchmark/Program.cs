using AlgorithmInternalBenchmark;
using AStar;
using RandomGraphProvider;
using System;
using TemporalSubgraph;
using TemporalSubgraph.Heuristics;

namespace TemporalSubgraphBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var random1 = new Random(3_14159265);
            var random2 = new Random(2_71828182);
            double vertexAttributeGenerator() => random1.NextDouble();
            double edgeAttributeGenerator() => random1.NextDouble();
            var nodes = 100;
            var density = .9d;
            var randomTemporalGraph1 = RandomGraphFactory.GenerateRandomInstance(nodes, density, true, vertexAttributeGenerator, edgeAttributeGenerator, random1);
            var randomTemporalGraph2 = RandomGraphFactory.GenerateRandomInstance(nodes, density, true, vertexAttributeGenerator, edgeAttributeGenerator, random2, nodes);

            var heuristic = new EliminationHeuristic<int, double>();

            var initialNode = new TemporalMatchingNode<int, double, double>(randomTemporalGraph1, randomTemporalGraph2, heuristic);

            var benchmark = new Benchmark<string>();
            initialNode.Benchmark = benchmark;

            var aStarAlgorithm = new AStarAlgorithm<TemporalMatchingNode<int, double, double>>(initialNode, benchmark);

            var padRight = 12;
            aStarAlgorithm.Logger += (message) => Console.WriteLine($"{benchmark.GetIntermittentResult("AStar").TotalMilliseconds.ToString().PadRight(padRight)}ms: {message}");
            aStarAlgorithm.Logger += (message) => Console.WriteLine(benchmark.ExportResults());

            benchmark.StartBenchmark("AStar");
            var solution = aStarAlgorithm.ExpandRecursively();
            benchmark.StopBenchmark("AStar");

            var benchmarkTextSummary = benchmark.ExportResults();

            Console.WriteLine($"Graph1 size: {randomTemporalGraph1.VertexCount} vertices");
            Console.WriteLine($"Graph2 size: {randomTemporalGraph2.VertexCount} vertices");
            Console.WriteLine($"Maximum temporal subgraph size: {-solution.DistanceFromSource()}");

            Console.WriteLine("Performance benchmarking results:");

            Console.WriteLine(benchmarkTextSummary);
        }
    }
}
