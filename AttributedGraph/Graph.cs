using System;
using System.Collections.Generic;

namespace AttributedGraph
{
    public class Graph<V, VA, EA>
    {
        private readonly Dictionary<V, VA> vertices = new Dictionary<V, VA>();
        private readonly Dictionary<(V, V), EA> edges = new Dictionary<(V, V), EA>();
        private readonly bool directed;

        public Graph(bool directed)
        {
            this.directed = directed;
        }
        public void AddEdge((V, V) edge, EA edgeAttribute)
        {
            edges.Add(edge, edgeAttribute);
            if (!directed)
            {
                var (u, v) = edge;
                edges.Add((v, u), edgeAttribute);
            }
        }
        public void RemoveEdge((V, V) edge)
            => edges.Remove(edge);
        public bool ContainsEdge((V, V) edge)
            => edges.ContainsKey(edge);
        public void AddVertex(V vertex, VA vertexAttribute)
            => vertices.Add(vertex, vertexAttribute);
        public void RemoveVertex(V vertex)
            => vertices.Remove(vertex);
        public bool ContainsVertex(V vertex)
            => vertices.ContainsKey(vertex);
        public VA this[V vertex]
        {
            get { return vertices[vertex]; }
            set
            {
                if (!vertices.ContainsKey(vertex))
                    vertices.Add(vertex, value);
                else
                    vertices[vertex] = value;
            }
        }
        public EA this[(V, V) edge]
        {
            get { return edges[edge]; }
            set
            {
                var (u, v) = edge;
                if (!edges.ContainsKey(edge))
                {
                    edges.Add(edge, value);
                    if (!directed)
                        edges.Add((v, u), value);
                }
                else
                {
                    edges[edge] = value;
                    if (!directed)
                        edges[(v, u)] = value;
                }
            }
        }
    }
}
