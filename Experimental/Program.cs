using AStarGraphNode;
using RandomGraphProvider;
using System;
using System.Collections.Generic;
using KNNClassifier;
using AttributedGraph;

namespace Experimental
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random(3_14159265);
            Func<double> vertexAttributeGenerator = () =>
            {
                return random.NextDouble();
            };
            Func<double> edgeAttributeGenerator = () =>
            {
                return random.NextDouble();
            };

            Func<double, double, double> vertexRelabel = (a1, a2) =>
            {
                return Math.Abs(a1 - a2);
            };
            Func<double, double> vertexAdd = a => Math.Abs(a);
            Func<double, double> vertexRemove = vertexAdd;

            Func<double, double, double> edgeRelabel = (a1, a2) =>
            {
                return Math.Abs(a1 - a2);
            };
            Func<double, double> edgeAdd = a => Math.Abs(a);
            Func<double, double> edgeRemove = edgeAdd;

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 10,
                density: .6,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );
            var graphsPreclassified = new List<(Graph<int, double, double>, int)>();
            const int hGraphCount = 100;
            for (int i = 0; i < hGraphCount; i++)
            {
                var H = RandomGraphFactory.generateRandomInstance(
                    vertices: 13,
                    density: .355,
                    directed: true,
                    vertexAttributeGenerator: vertexAttributeGenerator,
                    edgeAttributeGenerator: edgeAttributeGenerator
                    );
                var hClassID = H.EdgeCount > 54 ? 1 : 0;
                graphsPreclassified.Add((H, hClassID));
            }

            var a = new List<double>() { -1, 2, 0, 1, .5 };
            var b = new List<double>() { -1, 2, 0, 1, .5 };

            var k = 3;

            var encodingMethod = GraphEncodingMethod.Wojciechowski;

            var gClassID = KNNClassifier.KNNClassifier.Classify<int, double, double>(
                k: k,
                G,
                graphsPreclassified,
                vertexAdd,
                vertexRelabel,
                vertexRemove,
                edgeAdd,
                edgeRelabel,
                edgeRemove,
                a,
                b,
                encodingMethod: encodingMethod
            );

            System.Console.WriteLine($"Detected class: {gClassID}");
        }
    }
}
