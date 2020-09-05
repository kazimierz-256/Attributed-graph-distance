using AttributedGraph;
using System;

namespace RandomGraphProvider
{
    public static class RandomGraphFactory
    {
        public static Graph<int, VA, EA> GenerateRandomInstance<VA, EA>(
                int vertices,
                double density,
                bool directed,
                Func<VA> vertexAttributeGenerator,
                Func<EA> edgeAttributeGenerator,
                Random random = null,
                int vertexOffset = 0,
                bool allowLoops = true
            )
        {
            var graph = new Graph<int, VA, EA>(directed);

            for (int i = 0; i < vertices; i++)
                graph.AddVertex(i + vertexOffset, vertexAttributeGenerator());

            if (random == null)
                random = new Random();
            for (int i = 0; i < vertices; i++)
            {
                var jstart = directed ? 0 : i;
                for (int j = jstart; j < vertices; j++)
                {
                    if (!(i == j && !allowLoops) && random.NextDouble() <= density)
                        graph.AddEdge((i + vertexOffset, j + vertexOffset), edgeAttributeGenerator());
                }
            }

            return graph;
        }

        /// <summary>
        /// First tuple element is a subgraph of the second element
        /// </summary>
        public static (Graph<int, VA, EA>, Graph<int, VA, EA>) GenerateRandomInstanceWithASubinstance<VA, EA>(
                int subgraphVertexCount,
                int supergraphVertexCount,
                double density,
                bool directed,
                Func<VA> vertexAttributeGenerator,
                Func<EA> edgeAttributeGenerator,
                Random random = null,
                int vertexOffset = 0,
                bool allowLoops = true
            )
        {
            if (supergraphVertexCount <= subgraphVertexCount || subgraphVertexCount < 0)
                throw new Exception("Invalid number of vertices of subgraph or supergraph");

            var subgraph = new Graph<int, VA, EA>(directed);

            for (int i = 0; i < subgraphVertexCount; i++)
                subgraph.AddVertex(i + vertexOffset, vertexAttributeGenerator());

            if (random == null)
                random = new Random();

            for (int i = 0; i < subgraphVertexCount; i++)
            {
                var jstart = directed ? 0 : i;
                for (int j = jstart; j < subgraphVertexCount; j++)
                {
                    if (!(i == j && !allowLoops) && random.NextDouble() <= density)
                        subgraph.AddEdge((i + vertexOffset, j + vertexOffset), edgeAttributeGenerator());
                }
            }

            var supergraph = subgraph.Clone();

            for (int i = subgraphVertexCount; i < supergraphVertexCount; i++)
                supergraph.AddVertex(i + vertexOffset, vertexAttributeGenerator());

            for (int i = subgraphVertexCount; i < supergraphVertexCount; i++)
            {
                var jstart = directed ? 0 : i;
                for (int j = jstart; j < supergraphVertexCount; j++)
                {
                    if (!(i == j && !allowLoops) && random.NextDouble() <= density)
                        supergraph.AddEdge((i + vertexOffset, j + vertexOffset), edgeAttributeGenerator());
                }
            }

            return (subgraph, supergraph);
        }
    }
}
