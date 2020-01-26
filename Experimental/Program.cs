using RandomGraphProvider;
using System;

namespace Experimental
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            Func<double> vertexAttributeGenerator = () =>
            {
                return random.NextDouble();
            };
            Func<double> edgeAttributeGenerator = () =>
            {
                return random.NextDouble();
            };

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 10,
                density: .6,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );

            var H = RandomGraphFactory.generateRandomInstance(
                vertices: 13,
                density: .355,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );


        }
    }
}
