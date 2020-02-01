using System;
using System.Collections.Generic;

namespace AStarGraphNode
{
    public class GraphMatchingParameters<V, VA, EA>
    {
        public Func<VA, double> vertexAdd;
        public Func<VA, VA, double> vertexRelabel;
        public Func<VA, double> vertexRemove;
        public Func<EA, double> edgeAdd;
        public Func<EA, EA, double> edgeRelabel;
        public Func<EA, double> edgeRemove;
        public IEnumerable<double> aCollection;
        public IEnumerable<double> bCollection;
        public List<(V, V)> preassignedVertices = default;
        public GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski;

        public static GraphMatchingParameters<V, VA, EA> UnitCostDefault()
        {
            return new GraphMatchingParameters<V, VA, EA>()
            {
                vertexAdd = a => 1d,
                edgeAdd = a => 1d,
                vertexRemove = a => 1d,
                edgeRemove = a => 1d,
                vertexRelabel = (a1, a2) => 1d,
                edgeRelabel = (a1, a2) => 1d,
                aCollection = new double[] { .5 },
                bCollection = new double[] { .5 },
            };
        }
    }
}