using System;
using System.Collections.Generic;

namespace AStarGraphNode
{
    public class GraphMatchingParameters<V, VA, EA>
    {
        public Func<VA, double> vertexAdd = a => 1;
        public Func<VA, double> vertexRemove = a => 1;
        public Func<VA, VA, double> vertexRelabel = (a1, a2) => a1.Equals(a2) ? 0 : 1;

        public Func<EA, double> edgeAdd = a => 1;
        public Func<EA, double> edgeRemove = a => 1;
        public Func<EA, EA, double> edgeRelabel = (a1, a2) => a1.Equals(a2) ? 0 : 1;

        public IEnumerable<double> aCollection = new double[] { .5 };
        public IEnumerable<double> bCollection = new double[] { .5 };

        public List<(V, V)> preassignedVertices = default;
        public GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski;

        public static GraphMatchingParameters<V, VA, EA> UnitCostDefault() => new GraphMatchingParameters<V, VA, EA>();
        public static GraphMatchingParameters<V, double, double> DoubleCostComposer(
            CostType vertexCostType,
            CostType edgeCostType,
            List<double> vertexParameters = default,
            List<double> edgeParameters = default
            )
        {
            var parameters = new GraphMatchingParameters<V, double, double>();

            Func<List<double>, int, double, double> getParameterDefault = (list, index, defaultValue) =>
            {
                if (list != null && index < list.Count)
                    return list[index];
                else
                    return defaultValue;
            };

            Func<double, double, double, double> bound = (a, c1, c2) =>
            {
                var aScaled = a * c2;
                if (a < 1)
                    return c1 * aScaled * (1 + aScaled);
                else
                    return c1 / (1 / aScaled + 1);
            };

            switch (vertexCostType)
            {
                case CostType.UnitCost:
                    parameters.vertexAdd = a => 1;
                    parameters.vertexRemove = a => 1;
                    parameters.vertexRelabel = (a1, a2) => a1.Equals(a2) ? 0 : 1;

                    parameters.edgeAdd = a => 1;
                    parameters.edgeRemove = a => 1;
                    parameters.edgeRelabel = (a1, a2) => a1.Equals(a2) ? 0 : 1;

                    break;
                case CostType.AbsoluteValue:
                    parameters.vertexAdd = a => Math.Abs(a);
                    parameters.vertexRemove = a => Math.Abs(a);
                    parameters.vertexRelabel = (a1, a2) => Math.Abs(a1 - a2);

                    parameters.edgeAdd = a => Math.Abs(a);
                    parameters.edgeRemove = a => Math.Abs(a);
                    parameters.edgeRelabel = (a1, a2) => Math.Abs(a1 - a2);

                    break;
                case CostType.AbsoluteValueBounded:
                    parameters.vertexAdd = a => bound(
                        Math.Abs(a),
                        (double)getParameterDefault(vertexParameters, 0, 1),
                        (double)getParameterDefault(vertexParameters, 1, 1)
                        );
                    parameters.vertexRemove = a => bound(
                        Math.Abs(a),
                        (double)getParameterDefault(vertexParameters, 0, 1),
                        (double)getParameterDefault(vertexParameters, 1, 1)
                        );
                    parameters.vertexRelabel = (a1, a2) => bound(
                        Math.Abs(a1 - a2),
                        getParameterDefault(vertexParameters, 0, 1),
                        getParameterDefault(vertexParameters, 1, 1)
                        );


                    parameters.edgeAdd = a => bound(
                        Math.Abs(a),
                        getParameterDefault(edgeParameters, 0, 1),
                        getParameterDefault(edgeParameters, 1, 1)
                        );
                    parameters.edgeRemove = a => bound(
                        Math.Abs(a),
                        getParameterDefault(edgeParameters, 0, 1),
                        getParameterDefault(edgeParameters, 1, 1)
                        );
                    parameters.edgeRelabel = (a1, a2) => bound(
                        Math.Abs(a1 - a2),
                        getParameterDefault(edgeParameters, 0, 1),
                        getParameterDefault(edgeParameters, 1, 1)
                        );

                    break;
            }

            return parameters;
        }
    }

    public enum CostType
    {
        UnitCost,
        AbsoluteValue,
        AbsoluteValueBounded
    }
}