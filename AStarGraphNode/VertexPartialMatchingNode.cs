using AStar;
using AttributedGraph;
using System;
using System.Collections.Generic;
using LinearAssignmentSolver;
using System.Linq;

namespace AStarGraphNode
{
    public class VertexPartialMatchingNode<V, VA, EA> : INode
    {
        private readonly Graph<V, VA, EA> G;
        private readonly Graph<V, VA, EA> H;
        private readonly List<(V, V)> preassignedVertices;
        public VertexPartialMatchingNode(
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                Func<VA, VA, double> vertexAttributeMetric,
                Func<EA, EA, double> edgeAttributeMetric,
                ICollection<double> a,
                ICollection<double> b,
                List<(V, V)> preassignedVertices = default,
                GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski
            )
        {
            this.G = G;
            this.H = H;
            this.preassignedVertices = preassignedVertices;

            // Matching part
            // TODO: build a cached matrix of all possible attributes for easy retrieval and modification
            // taking into account already assigned vertices

            // TODO: compute the lower bound using LAP taking constraints into account

            double[,] costMatrix = null;
            var infinity = double.PositiveInfinity;

            var gVerticesKVP = G.Vertices.ToList();
            var hVerticesKVP = G.Vertices.ToList();

            // TODO: take into account preassignedVertices, definitely create smaller cost matrix
            switch (encodingMethod)
            {
                case GraphEncodingMethod.Wojciechowski:
                    {
                        var m = Math.Max(G.VertexCount, H.VertexCount);
                        costMatrix = new double[m, m];

                        // fill in vertex replacement costs
                        for (int v = 0; v < G.VertexCount; v++)
                        {
                            for (int f_v = 0; f_v < H.VertexCount; f_v++)
                            {
                                var edgeCostArray = new double[m, m];
                                throw new NotImplementedException();
                            }
                        }

                        // fill in vertex adding (to G) costs
                        for (int v = G.VertexCount; v < m; v++)
                        {
                            for (int f_v = 0; f_v < H.VertexCount; f_v++)
                            {
                                throw new NotImplementedException();
                            }
                        }

                        // fill in vertex removal (from G) costs
                        for (int v = 0; v < G.VertexCount; v++)
                        {
                            for (int f_v = H.VertexCount; f_v < m; f_v++)
                            {
                                throw new NotImplementedException();
                            }
                        }
                        break;
                    }
                case GraphEncodingMethod.RiesenBunke:
                    {
                        var m = G.VertexCount + H.VertexCount;
                        costMatrix = new double[m, m];
                        break;
                    }
                default:
                    throw new Exception("Unknown graph encoding method");
            }

            var assignment = LinearAssignmentSolver.LAPSolver.SolveAssignment(costMatrix);
            var distance = LinearAssignmentSolver.LAPSolver.AssignmentCost(costMatrix, assignment);
            LowerBound = distance;
            // TODO: compute the cost from the assignment
            UpperBound = double.NaN;
        }

        public int CompareTo(object obj)
        {
            var node = (INode) obj;
            if (node == null)
                throw new Exception("Incompatible node type");
            return this.LowerBound.CompareTo(node.LowerBound);
        }
        List<INode> INode.Expand()
        {
            // expand along some unassigned vertex
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
