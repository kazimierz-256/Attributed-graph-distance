using AlgorithmInternalBenchmark;
using AStar;
using AttributedGraph;
using RandomGraphProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TemporalSubgraph;
using TemporalSubgraph.Heuristics;

namespace TemporalSubgraphBenchmark
{
    class Program
    {
        private static IEnumerable<int> FibonacciGenerator(int counter1 = 1, int counter2 = 1)
        {
            var smallerCounter = Math.Min(counter1, counter2);
            var largerCounter = Math.Max(counter1, counter2);

            while (true)
            {
                yield return largerCounter;
                (largerCounter, smallerCounter) = (smallerCounter + largerCounter, largerCounter);
            }
        }

        private static IEnumerable<IHeuristic<int, double>> generateAllHeuristics()
        {
            yield return new TrivialHeuristic<int, double>();

            var counter = 100000;
            yield return new ExactHeuristic<int, double>(() => ++counter);
        }
        private static IEnumerable<(Graph<int, double, double>, Graph<int, double, double>, IHeuristic<int, double>)> generateTestCases()
        {
            var densityCount = 6;
            foreach (var graph1nodeCount in FibonacciGenerator())
            {
                foreach (var graph2nodeCount in FibonacciGenerator().TakeWhile(number => number <= graph1nodeCount))
                {
                    foreach (var density1 in Enumerable.Range(1, densityCount).Select(integer => integer * 1d / densityCount))
                    {
                        foreach (var density2 in Enumerable.Range(1, densityCount).Select(integer => integer * 1d / densityCount))
                        {
                            foreach (var heuristic in generateAllHeuristics())
                            {
                                var random1 = new Random(graph2nodeCount + graph1nodeCount * graph1nodeCount + (int)(density1 * 1000) + (int)(density2 * 100000));
                                var random2 = new Random(random1.Next());
                                Func<double> vertexAttributeGenerator1 = () => random1.NextDouble();
                                Func<double> vertexAttributeGenerator2 = () => random2.NextDouble();
                                Func<double> edgeAttributeGenerator1 = () => random1.NextDouble();
                                Func<double> edgeAttributeGenerator2 = () => random2.NextDouble();

                                var randomTemporalGraph1 = RandomGraphFactory.GenerateRandomInstance(graph1nodeCount, density1, true, vertexAttributeGenerator1, edgeAttributeGenerator1, random1);
                                var randomTemporalGraph2 = RandomGraphFactory.GenerateRandomInstance(graph2nodeCount, density2, true, vertexAttributeGenerator2, edgeAttributeGenerator2, random2, graph1nodeCount);

                                yield return (randomTemporalGraph1, randomTemporalGraph2, heuristic);
                            }
                        }
                    }
                }
            }
        }
        private const string csvExactPath = @"benchmark.csv";
        private const string texExactPath = @"benchmark.tex";
        static void Main(string[] args)
        {
            File.WriteAllText(csvExactPath, string.Empty);
            File.WriteAllText(texExactPath, string.Empty);

            foreach (var (graph1, graph2, heuristic) in generateTestCases())
            {
                var initialNode = new TemporalMatchingNode<int, double, double>(graph1, graph2, heuristic, false);
                var benchmark = new Benchmark<string>();
                initialNode.Benchmark = benchmark;

                var algorithm = new AStarAlgorithm<TemporalMatchingNode<int, double, double>>(initialNode, benchmark);

                Console.WriteLine("Computing...");

                benchmark.StartBenchmark("AStar");
                var temporalMatching = algorithm.ExpandRecursively();
                benchmark.StopBenchmark("AStar");

                Console.WriteLine("Computed.");

                var graph1density = graph1.EdgeCount * 1d / (graph1.VertexCount * graph1.VertexCount);
                var graph2density = graph2.EdgeCount * 1d / (graph2.VertexCount * graph2.VertexCount);
                var subgraphVertexCount = -temporalMatching.DistanceFromSource();
                var subgraphDensity = temporalMatching.alreadyMatchedEdges.Count * 1d / (temporalMatching.alreadyMatchedVertices.Count * temporalMatching.alreadyMatchedVertices.Count);
                var computationTime = benchmark.GetIntermittentResult("AStar").TotalSeconds;
                var expansionCount = benchmark.GetIntermittentCount("Expand outer");
                var removalCount = benchmark.GetIntermittentCount("Removed worst node");

                // size of graph1
                // size of graph2
                // density of graph1
                // density of graph2
                // type of heuristic
                // size of maximum common subgraph
                // density of subgraph
                // total computation time in miliseconds
                // number of nodes expanded by A* algorithm
                // number of nodes automatically pruned

                using (var csvWriter = File.AppendText(csvExactPath))
                    csvWriter.WriteLine($"{graph1.VertexCount},{graph2.VertexCount},{graph1density:0.00},{graph2density:0.00},{heuristic.Name},{subgraphVertexCount},{subgraphDensity:0.00},{computationTime:0.00},{expansionCount},{removalCount}");
                using (var texWriter = File.AppendText(texExactPath))
                    texWriter.WriteLine($"{graph1.VertexCount} & {graph2.VertexCount} & {graph1density:0.00} & {graph2density:0.00} & {heuristic.Name} & {subgraphVertexCount} & {subgraphDensity:0.00} & {computationTime:0.00} & {expansionCount} & {removalCount} \\\\");

                Console.WriteLine("Saved.");
            }

            //var random1 = new Random(3_14159265);
            //var random2 = new Random(2_71828182);
            //double vertexAttributeGenerator() => random1.NextDouble();
            //double edgeAttributeGenerator() => random1.NextDouble();
            //var nodes = 100;
            //var density = .9d;
            //var randomTemporalGraph1 = RandomGraphFactory.GenerateRandomInstance(nodes, density, true, vertexAttributeGenerator, edgeAttributeGenerator, random1);
            //var randomTemporalGraph2 = RandomGraphFactory.GenerateRandomInstance(nodes, density, true, vertexAttributeGenerator, edgeAttributeGenerator, random2, nodes);

            ////var counter = -100;
            ////var heuristic = new ExactHeuristic<int, double>(() => counter--);
            //var heuristic = new TrivialHeuristic<int, double>();

            //var initialNode = new TemporalMatchingNode<int, double, double>(randomTemporalGraph1, randomTemporalGraph2, heuristic);

            //var benchmark = new Benchmark<string>();
            //initialNode.Benchmark = benchmark;

            //var aStarAlgorithm = new AStarAlgorithm<TemporalMatchingNode<int, double, double>>(initialNode, benchmark);

            //var padRight = 12;
            //aStarAlgorithm.Logger += (message) => Console.WriteLine($"{benchmark.GetIntermittentResult("AStar").TotalMilliseconds.ToString().PadRight(padRight)}ms: {message}");
            //aStarAlgorithm.Logger += (message) => Console.WriteLine(benchmark.ExportResults());

            //benchmark.StartBenchmark("AStar");
            //var solution = aStarAlgorithm.ExpandRecursively();
            //benchmark.StopBenchmark("AStar");

            //var benchmarkTextSummary = benchmark.ExportResults();

            //Console.WriteLine($"Graph1 size: {randomTemporalGraph1.VertexCount} vertices");
            //Console.WriteLine($"Graph2 size: {randomTemporalGraph2.VertexCount} vertices");
            //Console.WriteLine($"Maximum temporal subgraph size: {-solution.DistanceFromSource()}");

            //Console.WriteLine("Performance benchmarking results:");

            //Console.WriteLine(benchmarkTextSummary);
        }
    }
}
