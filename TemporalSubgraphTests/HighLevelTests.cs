using AStar;
using AttributedGraph;
using System;
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

            var heuristic = new DijkstraHeuristic<string, int>();

            var initialNode = new TemporalMatchingNode<string, int, int>(graph1, graph2, heuristic);
            var algorithm = new AStarAlgorithm<TemporalMatchingNode<string, int, int>>(initialNode);

            // Act
            var absolutelyBestNode = initialNode;
            var lowestAnalyzedDistanceValue = double.PositiveInfinity;
            while (algorithm.Queue.Count > 0)
            {
                var bestNode = algorithm.BestNode;
                var nodeDistance = bestNode.DistanceFromSource();
                if (nodeDistance < lowestAnalyzedDistanceValue)
                {
                    lowestAnalyzedDistanceValue = nodeDistance;
                    absolutelyBestNode = bestNode;
                }

                algorithm.ExpandBestNode();
                algorithm.Queue.Remove(bestNode);
            }
            var temporalMatching = absolutelyBestNode;

            // Assert
            Assert.Equal("1", temporalMatching.Matching("B"));
            Assert.Equal("2", temporalMatching.Matching("C"));
            Assert.Equal("3", temporalMatching.Matching("E"));
        }
    }
}
