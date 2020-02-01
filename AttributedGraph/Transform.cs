using System;
using System.Linq;

namespace AttributedGraph
{
    public static class Transform
    {
        public static Graph<V, VA, EA> PermuteClone<V, VA, EA>(
                this Graph<V, VA, EA> graph,
                Random random = null,
                Func<V, VA, (V, VA)> cloneVertex = null,
                Func<(V, V), EA, ((V, V), EA)> cloneEdge = null
            )
        {
            if (random == null)
            {
                random = new Random();
            }
            var vertices = graph.Vertices.ToList();
            var indexes = new int[graph.VertexCount];
            var randomNumbers = new int[graph.VertexCount];
            for (int i = 0; i < graph.VertexCount; i++)
            {
                indexes[i] = i;
                randomNumbers[i] = random.Next();
            }
            Array.Sort(randomNumbers, indexes);

            var newGraph = new Graph<V, VA, EA>(graph.Directed);

            foreach (var kvp in graph.Vertices)
            {
                var newVertex = kvp.Key;
                var newVertexAttribute = kvp.Value;
                if (cloneVertex != null)
                    (newVertex, newVertexAttribute) = cloneVertex(kvp.Key, kvp.Value);
                newGraph.AddVertex(newVertex, newVertexAttribute);
            }

            for (int i = 0; i < graph.VertexCount; i++)
            {
                for (int j = 0; j < graph.VertexCount; j++)
                {
                    var edge = (vertices[i].Key, vertices[j].Key);
                    if (graph.ContainsEdge(edge))
                    {
                        var newEdgeAttribute = graph[edge];
                        var newEdge = edge;
                        if (cloneEdge != null)
                            (newEdge, newEdgeAttribute) = cloneEdge(edge, newEdgeAttribute);
                        newGraph.AddEdge((vertices[indexes[i]].Key, vertices[indexes[j]].Key), newEdgeAttribute);
                    }
                }
            }

            return newGraph;
        }
        public static Graph<V, VA, EA> Clone<V, VA, EA>(
                this Graph<V, VA, EA> graph,
                Func<V, VA, (V, VA)> cloneVertex = null,
                Func<(V, V), EA, ((V, V), EA)> cloneEdge = null
            )
        {
            var newGraph = new Graph<V, VA, EA>(graph.Directed);

            foreach (var kvp in graph.Vertices)
            {
                var newVertex = kvp.Key;
                var newVertexAttribute = kvp.Value;
                if (cloneVertex != null)
                    (newVertex, newVertexAttribute) = cloneVertex(kvp.Key, kvp.Value);
                newGraph.AddVertex(newVertex, newVertexAttribute);
            }

            foreach (var kvp in graph.Edges)
            {
                var newEdge = kvp.Key;
                var newEdgeAttribute = kvp.Value;
                if (cloneEdge != null)
                    (newEdge, newEdgeAttribute) = cloneEdge(kvp.Key, kvp.Value);
                newGraph.AddEdge(newEdge, newEdgeAttribute);
            }

            return newGraph;
        }

        public static Graph<V, VA, EA> Augment<V, VA, EA>(
                this Graph<V, VA, EA> graph,
                int targetVertexCount,
                Func<(V, VA)> vertexGenerator
            )
        {
            while (graph.VertexCount < targetVertexCount)
            {
                var (vertex, attribute) = vertexGenerator();
                if (!graph.ContainsVertex(vertex))
                    graph.AddVertex(vertex, attribute);
            }

            return graph;
        }

        public static Graph<V, VA, EA> RemoveSmallestLast<V, VA, EA>(
                this Graph<V, VA, EA> graph,
                int targetVertexCount,
                Func<Graph<V, VA, EA>, V, int> graphVertexToDegree = null
                )
        {
            Func<V> chooseVertexToDelete = () =>
            {
                var vertexWithMinNeighbours = graph.Vertices.First().Key;
                var minNeighbours = int.MaxValue;
                foreach (var vertexKVP in graph.Vertices)
                {
                    var vertexNeighbourCount = int.MaxValue;
                    if (graphVertexToDegree == null)
                    {
                        vertexNeighbourCount = Math.Max(graph.OutgoingEdges[vertexKVP.Key].Count, graph.IncomingEdges[vertexKVP.Key].Count);
                    }
                    else
                    {
                        vertexNeighbourCount = graphVertexToDegree(graph, vertexKVP.Key);
                    }

                    if (vertexNeighbourCount < minNeighbours)
                    {
                        minNeighbours = vertexNeighbourCount;
                        vertexWithMinNeighbours = vertexKVP.Key;
                    }
                }
                return vertexWithMinNeighbours;
            };

            while (graph.VertexCount > targetVertexCount)
                graph.RemoveVertex(chooseVertexToDelete());

            return graph;
        }
    }
}