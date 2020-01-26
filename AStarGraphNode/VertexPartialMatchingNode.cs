using AStar;
using AttributedGraph;
using System;
using System.Collections.Generic;

namespace AStarGraphNode
{
    public class VertexPartialMatchingNode<V, VA, EA> : INode
    {
        private readonly Graph<V, VA, EA> G;
        private readonly Graph<V, VA, EA> H;
        private readonly List<(V, V)> preassignedVertices;
        public VertexPartialMatchingNode(Graph<V, VA, EA> G, Graph<V, VA, EA> H, List<(V, V)> preassignedVertices = default)
        {
            // TODO: build a cached matrix of all possible attributes for easy retrieval and modification
            // taking into account already assigned vertices
            this.G = G;
            this.H = H;
            this.preassignedVertices = preassignedVertices;
            // TODO: compute the lower bound using LAP taking constraints into account
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
        List<INode> INode.Expand()
        {
            throw new NotImplementedException();
        }

        public double LowerBound
        {
            get;
            private set;
        }

        public double UpperBound
        {
            get;
            private set;
        }
    }
}
