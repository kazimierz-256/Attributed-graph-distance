using System;
using Xunit;
using AttributedGraph;

namespace GraphEncodingTests
{
    public class EncodingTests
    {
        [Fact]
        public void EmptyGraphEncoding()
        {
            var emptyGraph = Encoding.EncodeJsonToGraph("{'directed': true}",
                int.Parse,
                double.Parse,
                double.Parse);

            Assert.True(emptyGraph.Directed);
            Assert.Empty(emptyGraph.Vertices);
        }

        private static readonly AttributedGraph.Graph<int, double, double> graph = Encoding.EncodeJsonToGraph(
                "{'vertex_attributes': {0: .5, 1: 2.0, 2: 0.128, 3: 8.5}, 'edge_attributes': {0: {1: 7, 0: 1}, 1: {0: -3, 2: 4.5}, 2: {3: 9.128}, 3: {}}, 'directed': true}",
                int.Parse,
                double.Parse,
                double.Parse);

        [Fact]
        public void GraphEncoding1()
        {
            Assert.Equal(8.5, graph.Vertices[3]);
            Assert.Equal(0.128, graph.Vertices[2]);
            Assert.Equal(2d, graph.Vertices[1]);
            Assert.Equal(.5, graph.Vertices[0]);

            Assert.Contains(2, graph.IncomingEdges[3]);
            Assert.Contains(0, graph.IncomingEdges[0]);
            Assert.Contains(1, graph.IncomingEdges[0]);

            Assert.Equal(7, graph.Edges[(0, 1)]);
            Assert.Equal(-3, graph.Edges[(1, 0)]);
        }
    }
}
