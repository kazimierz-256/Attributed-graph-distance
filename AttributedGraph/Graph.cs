using System;
using System.Collections.Generic;

namespace AttributedGraph
{
    public class Graph<V, VA, EA>
    {
        public readonly Dictionary<V, VA> Vertices = new Dictionary<V, VA>();
        public readonly Dictionary<V, HashSet<V>> OutgoingEdges = new Dictionary<V, HashSet<V>>();
        public readonly Dictionary<V, HashSet<V>> IncomingEdges = new Dictionary<V, HashSet<V>>();
        public readonly Dictionary<(V, V), EA> Edges = new Dictionary<(V, V), EA>();

        public Graph(bool directed)
        {
            Directed = directed;
        }
        public void AddEdge((V, V) edge, EA edgeAttribute)
        {
            if (ContainsEdge(edge))
                throw new Exception("The edge being added already exists in the graph");

            if (!ContainsVertex(edge.Item1) || !ContainsVertex(edge.Item2))
                throw new Exception("Tried to add an edge but no such adjacent vertex exist");

            Edges.Add(edge, edgeAttribute);
            var (u, v) = edge;
            OutgoingEdges[u].Add(v);
            IncomingEdges[v].Add(u);
            if (!Directed)
            {
                if (!edge.Item1.Equals(edge.Item2))
                    Edges.Add((v, u), edgeAttribute);
                OutgoingEdges[v].Add(u);
                IncomingEdges[u].Add(v);
            }
        }
        public void RemoveEdge((V, V) edge)
        {
            if (!ContainsEdge(edge))
                throw new Exception("The edge being deleted does not exists in the graph");

            Edges.Remove(edge);
            OutgoingEdges[edge.Item1].Remove(edge.Item2);
            if (!Directed && !edge.Item1.Equals(edge.Item2))
            {
                var (u, v) = edge;
                Edges.Remove((v, u));
                IncomingEdges[edge.Item2].Remove(edge.Item1);
            }
        }
        public bool ContainsEdge((V, V) edge)
        {
            if (Edges.ContainsKey(edge))
                return true;
            else if (!Directed)
                return Edges.ContainsKey((edge.Item2, edge.Item1));
            else
                return false;
        }
        public void AddVertex(V vertex, VA vertexAttribute = default)
        {
            if (ContainsVertex(vertex))
                throw new Exception("Vertex being added already exists in the graph");

            Vertices.Add(vertex, vertexAttribute);
            OutgoingEdges.Add(vertex, new HashSet<V>());
            IncomingEdges.Add(vertex, new HashSet<V>());
        }
        public void RemoveVertex(V vertex)
        {
            if (!Vertices.ContainsKey(vertex))
                throw new Exception("Vertex being deleted does not exists in the graph");

            foreach (var neighbour in OutgoingEdges[vertex])
                Edges.Remove((vertex, neighbour));
            foreach (var neighbour in IncomingEdges[vertex])
                Edges.Remove((neighbour, vertex));

            OutgoingEdges.Remove(vertex);
            IncomingEdges.Remove(vertex);
            Vertices.Remove(vertex);
        }
        public bool ContainsVertex(V vertex)
            => Vertices.ContainsKey(vertex);

        public int VertexCount => Vertices.Count;
        public int EdgeCount => Edges.Count;

        public bool Directed { get; }
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
            get
            {
                if (Edges.ContainsKey(edge))
                    return Edges[edge];
                else if (!Directed)
                    return Edges[(edge.Item2, edge.Item1)];
                else
                    throw new Exception("No such edge exists in the graph");
            }
            set
            {
                var (u, v) = edge;
                if (!ContainsEdge(edge))
                    AddEdge(edge, value);
                else
                {
                    Edges[edge] = value;
                    if (!Directed && !u.Equals(v))
                        Edges[(v, u)] = value;
                }
            }
        }
    }
}
