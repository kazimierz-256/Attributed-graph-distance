﻿using AttributedGraph;
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
                Random random = null
            )
        {
            var graph = new Graph<int, VA, EA>(directed);

            for (int i = 0; i < vertices; i++)
                graph.AddVertex(i, vertexAttributeGenerator());

            if (random == null)
                random = new Random();
            for (int i = 0; i < vertices; i++)
            {
                var jstart = directed ? 0 : i;
                for (int j = jstart; j < vertices; j++)
                {
                    if (random.NextDouble() <= density)
                        graph.AddEdge((i, j), edgeAttributeGenerator());
                }
            }

            return graph;
        }
    }
}
