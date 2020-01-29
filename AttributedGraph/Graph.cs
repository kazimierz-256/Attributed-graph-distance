using System;
using System.Collections.Generic;

namespace AttributedGraph
{
    public class Graph<V, VA, EA>
    {
        public readonly Dictionary<V, VA> Vertices = new Dictionary<V, VA>();
        // TODO: create outgoing and incoming edge data structures
        public readonly Dictionary<V, List<V>> OutgoingEdges = new Dictionary<V, List<V>>();
        public readonly Dictionary<V, List<V>> IncomingEdges = new Dictionary<V, List<V>>();
        public readonly Dictionary<(V, V), EA> Edges = new Dictionary<(V, V), EA>();
        private readonly bool directed;

        public Graph(bool directed)
        {
            this.directed = directed;
        }
        public void AddEdge((V, V) edge, EA edgeAttribute)
        {
            Edges.Add(edge, edgeAttribute);
            if (!directed)
            {
                var (u, v) = edge;
                Edges.Add((v, u), edgeAttribute);
            }
        }
        public void RemoveEdge((V, V) edge)
            => Edges.Remove(edge);
        public bool ContainsEdge((V, V) edge)
            => Edges.ContainsKey(edge);
        public void AddVertex(V vertex, VA vertexAttribute)
            => Vertices.Add(vertex, vertexAttribute);
        // IMPORTANT TODO: when removeing a vertex, remove adjacent edges too!
        public void RemoveVertex(V vertex)
            => Vertices.Remove(vertex);
        public bool ContainsVertex(V vertex)
            => Vertices.ContainsKey(vertex);

        public int VertexCount => Vertices.Count;
        public int EdgeCount => Edges.Count;

        public bool Directed => directed;
        public VA this[V vertex]
        {
            get { return Vertices[vertex]; }
            set
            {
                if (!Vertices.ContainsKey(vertex))
                    Vertices.Add(vertex, value);
                else
                    Vertices[vertex] = value;
            }
        }
        public EA this[(V, V) edge]
        {
            get { return Edges[edge]; }
            set
            {
                var (u, v) = edge;
                if (!Edges.ContainsKey(edge))
                {
                    Edges.Add(edge, value);
                    if (!directed)
                        Edges.Add((v, u), value);
                }
                else
                {
                    Edges[edge] = value;
                    if (!directed)
                        Edges[(v, u)] = value;
                }
            }
        }
    }
}
