using AStar;
using AttributedGraph;
using System;
using TemporalSubgraph;
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

            var initialNode = new TemporalMatchingNode<string, int, int>(graph1, graph2);
            var algorithm = new AStarAlgorithm(initialNode);

            // Act
            var expanded = true;
            while (expanded)
                expanded = algorithm.ExpandBestNode();
            var temporalMatching = algorithm.BestNode;

            // Assert
            Assert.Equal(temporalMatching.)
        }
    }
}
