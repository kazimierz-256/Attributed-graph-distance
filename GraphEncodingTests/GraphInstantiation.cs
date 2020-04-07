using AttributedGraph;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AttributedGraphTests
{
    public class GraphInstantiation
    {
        [Fact]
        public void EmptyGraph()
        {
            var graph = new Graph<int, double, double>(true);
            Assert.Empty(graph.Vertices);
            Assert.Empty(graph.OutgoingEdges);
            Assert.Empty(graph.IncomingEdges);

            graph.AddVertex(0, 0d);
            graph.AddVertex(1, .1);
            graph.AddVertex(2, .2);
            graph.AddVertex(3, .3);

            Assert.Equal(4, graph.VertexCount);
            foreach (var vertexKVP in graph.OutgoingEdges)
                Assert.Empty(vertexKVP.Value);
            foreach (var vertexKVP in graph.IncomingEdges)
                Assert.Empty(vertexKVP.Value);

            graph.AddEdge((2, 3), -1d);

            Assert.Single(graph.OutgoingEdges[2]);
            Assert.Single(graph.IncomingEdges[3]);

            Assert.Equal(graph.Edges[(2, 3)], -1d);
        }
    }
}
