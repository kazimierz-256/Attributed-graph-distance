using AStar;
using AttributedGraph;
using RandomGraphProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using TemporalSubgraph;
using TemporalSubgraph.Heuristics;
using Xunit;

namespace TemporalSubgraphTests
{
    public class HighLevelTests
    {
        [Fact]
        public void ExampleFromArxiv()
        {
            // Arrange
            // Graph from first page of https://arxiv.org/pdf/1801.08098.pdf
            var graph1 = new Graph<string, int, int>(directed: true);
            foreach (var vertex in new[] { "A", "B", "C", "D", "E", "F" })
                graph1.AddVertex(vertex);
            graph1.AddEdge(("A", "B"), 3);
            graph1.AddEdge(("B", "C"), 2);
            graph1.AddEdge(("B", "D"), 6);
            graph1.AddEdge(("C", "A"), 5);
            graph1.AddEdge(("C", "E"), 4);
            graph1.AddEdge(("D", "E"), 1);
            graph1.AddEdge(("E", "B"), 7);
            graph1.AddEdge(("E", "F"), 9);
            graph1.AddEdge(("F", "C"), 8);

            var graph2 = new Graph<string, int, int>(directed: true);
            graph2.AddVertex("1");
            graph2.AddVertex("2");
            graph2.AddVertex("3");
            graph2.AddEdge(("1", "2"), 1);
            graph2.AddEdge(("2", "3"), 2);
            graph2.AddEdge(("3", "1"), 3);

            var heuristic = new TrivialHeuristic<string, int>();

            var initialNode = new TemporalMatchingNode<string, int, int>(graph1, graph2, heuristic);
            var algorithm = new AStarAlgorithm<TemporalMatchingNode<string, int, int>>(initialNode);

            // Act
            var temporalMatching = algorithm.ExpandRecursively();

            // Assert
            Assert.Equal("1", temporalMatching.Matching("B"));
            Assert.Equal("2", temporalMatching.Matching("C"));
            Assert.Equal("3", temporalMatching.Matching("E"));
        }

        public static IEnumerable<object[]> IsomorphicGraphCases(string name)
        {
            var densitiyCount = 8;
            var verticesCount = 8;
            var offset = verticesCount;
            foreach (var heuristic in generateAllHeuristics())
            {
                foreach (var density in Enumerable.Range(1, densitiyCount).Select(integer => integer * 1d / densitiyCount))
                {
                    foreach (var vertexCount in Enumerable.Range(2, verticesCount))
                    {
                        var random = new Random(vertexCount + (int)(density * 10000));
                        var random2 = new Random(random.Next());
                        var random3 = new Random(random.Next());
                        Func<double> edgeAttributeGenerator = () => random.NextDouble();
                        var graph1 = RandomGraphFactory.GenerateRandomInstance(vertexCount, density, true, () => 0, edgeAttributeGenerator, random2, allowLoops: false);
                        var graph2 = Transform.PermuteClone(graph1, random3, (id, attr) => (id + offset, attr), (pair, attr) => ((pair.Item1 + offset, pair.Item2 + offset), attr));

                        yield return new object[] { $"VertexCount: {vertexCount}, density: {density:0.00}", heuristic, graph1, graph2 };
                    }
                }
            }
        }

        public static IEnumerable<object[]> SubgraphSupergraphCases(string name)
        {
            var densitiyCount = 8;
            var verticesCount = 8;
            var offset = verticesCount;
            foreach (var heuristic in generateAllHeuristics())
            {
                foreach (var density in Enumerable.Range(1, densitiyCount).Select(integer => integer * 1d / densitiyCount))
                {
                    foreach (var supergraphVertexCount in Enumerable.Range(2, verticesCount))
                    {
                        foreach (var subgraphVertexCount in Enumerable.Range(0, supergraphVertexCount))
                        {
                            var random = new Random(subgraphVertexCount + verticesCount * supergraphVertexCount + (int)(density * 10000));
                            var random2 = new Random(random.Next());
                            var random3 = new Random(random.Next());
                            Func<double> edgeAttributeGenerator = () => random.NextDouble();
                            var (subgraph, supergraph) = RandomGraphFactory.GenerateRandomInstanceWithASubinstance(subgraphVertexCount, supergraphVertexCount, density, true, () => 0, edgeAttributeGenerator, random2, allowLoops: false);
                            var subgraphPermuted = Transform.PermuteClone(subgraph, random3, (id, attr) => (id + offset, attr), (pair, attr) => ((pair.Item1 + offset, pair.Item2 + offset), attr));

                            yield return new object[] { $"SubgraphVC: {subgraphVertexCount}, supergraphVC: {supergraphVertexCount}, density: {density:0.00}", heuristic, subgraphPermuted, supergraph };
                        }
                    }
                }
            }
        }

        static IHeuristic<int, double> generateExactHeuristic()
        {
            var counter = 100000;
            return new ExactHeuristic<int, double>(() => ++counter);
        }
        static IHeuristic<int, double> generateTrivialHeuristic() => new TrivialHeuristic<int, double>();

        static IEnumerable<IHeuristic<int, double>> generateAllHeuristics()
        {
            yield return generateTrivialHeuristic();
            yield return generateExactHeuristic();
        }

        [Theory]
        [MemberData(nameof(IsomorphicGraphCases), "Isomorphic")]
        public void IsomorphicGraphs(string name, IHeuristic<int, double> heuristic, Graph<int, int, double> graph1, Graph<int, int, double> graph2)
        {
            // Arrange
            var initialNode = new TemporalMatchingNode<int, int, double>(graph1, graph2, heuristic, false);
            var algorithm = new AStarAlgorithm<TemporalMatchingNode<int, int, double>>(initialNode);

            // Act
            var temporalMatching = algorithm.ExpandRecursively();

            // Assert
            Assert.Equal(graph1.VertexCount, graph2.VertexCount);
            Assert.Equal(graph1.VertexCount, -1 * temporalMatching.DistanceFromSource());
        }

        [Theory]
        [MemberData(nameof(SubgraphSupergraphCases), "SubSupergraph")]
        public void SubgraphsSupergraphs(string name, IHeuristic<int, double> heuristic, Graph<int, int, double> graph1, Graph<int, int, double> graph2)
        {
            // Arrange
            var initialNode = new TemporalMatchingNode<int, int, double>(graph1, graph2, heuristic, false);
            var algorithm = new AStarAlgorithm<TemporalMatchingNode<int, int, double>>(initialNode);

            // Act
            var temporalMatching = algorithm.ExpandRecursively();

            // Assert
            Assert.True(graph1.VertexCount <= graph2.VertexCount);
            Assert.Equal(graph1.VertexCount, -1 * temporalMatching.DistanceFromSource());
        }
    }
}
