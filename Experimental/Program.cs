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
            var random = new Random();
            Func<double> vertexAttributeGenerator = () =>
            {
                return random.NextDouble();
            };
            Func<double> edgeAttributeGenerator = () =>
            {
                return random.NextDouble();
            };

            Func<double, double, double> vertexAttributeMetric = (a1, a2) =>
            {
                return Math.Abs(a1 - a2);
            };

            Func<double, double, double> edgeAttributeMetric = (a1, a2) =>
            {
                return Math.Abs(a1 - a2);
            };

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

            var distanceDictionary = new Dictionary<(double, double), double>();
            foreach (var (vertex, attribute) in G.Vertices)
            {

            }

            var a = new List<double>() { -1, 2, 0, 1, .5 };
            var b = new List<double>() { -1, 2, 0, 1, .5 };

            var k = 3;

            var encodingMethod = GraphEncodingMethod.Wojciechowski;

            var gClassID = KNNClassifier.KNNClassifier.Classify<int, double, double>(
                k: k,
                G,
                graphsPreclassified,
                vertexAttributeMetric,
                edgeAttributeMetric,
                a,
                b,
                encodingMethod: encodingMethod
            );

            System.Console.WriteLine($"Detected class: {gClassID}");
        }
    }
}
