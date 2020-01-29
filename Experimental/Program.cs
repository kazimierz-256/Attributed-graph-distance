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

            double vertexBound = 2;
            double edgeBound = 3;
            double vertexDecay = 1;
            double edgeDecay = 1;

            Func<double, double, double, double> boundMetric = (a, bound, decay) => bound * a / (1 / decay + a);

            Func<double, double, double> vertexRelabel = (a1, a2) =>
            {
                return boundMetric(Math.Abs(a1 - a2), edgeBound, edgeDecay);
            };
            Func<double, double> vertexAdd = a => boundMetric(Math.Abs(a), vertexBound, vertexDecay);
            Func<double, double> vertexRemove = vertexAdd;

            Func<double, double, double> edgeRelabel = (a1, a2) =>
            {
                return boundMetric(Math.Abs(a1 - a2));
            };
            Func<double, double> edgeAdd = a => boundMetric(Math.Abs(a));
            Func<double, double> edgeRemove = edgeAdd;

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 10,
                density: .6,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );
            const int hGraphCount = 23;
            var graphsPreclassified = new List<(Graph<int, double, double>, int)>();
            // graphsPreclassified.Add((Transform.Permute(G, random), 0));
            // var anotherPermutation = Transform.Permute(G, random);
            // anotherPermutation.AddVertex(-1, 0);
            // anotherPermutation.AddVertex(-2, 0);
            // anotherPermutation.AddVertex(-3, 0);
            // graphsPreclassified.Add((anotherPermutation, 1));
            for (int i = 0; i < hGraphCount; i++)
            {
                var H = RandomGraphFactory.generateRandomInstance(
                    vertices: i,
                    density: .355,
                    directed: true,
                    vertexAttributeGenerator: vertexAttributeGenerator,
                    edgeAttributeGenerator: edgeAttributeGenerator
                    );
                var hClassID = H.EdgeCount > 54 ? 1 : 0;
                graphsPreclassified.Add((H, hClassID));
            }

            var a = new List<double>() { 0, 1, .5, 1d / 3, 2d / 3, -1, 2, 10, -10, 100, -100, 1000, -1000 };
            var b = new List<double>() { 0, 1, .5, 1d / 3, 2d / 3, -1, 2, 10, -10, 100, -100, 1000, -1000 };
            // var a = new List<double>() { 0, 1, .5};
            // var b = new List<double>() { 0, 1, .5};

            var k = 3;

            var encodingMethod = GraphEncodingMethod.Wojciechowski;

            var (gClassID, matchingClassPairs) = KNNClassifier.KNNClassifier.Classify<int, double, double>(
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

            var RiesenBunke2009AB = (1, 1);

            var printouts = new List<(string, Func<VertexPartialMatchingNode<int, double, double>, int, string>)>()
            {
                (
                    "Class: ",
                    (matching, classID) => $"{classID} "
                ),
                (
                    "My lower bound / theirs: ",
                    (matching, classID) => $"{matching.LowerBound / matching.abLowerBounds[RiesenBunke2009AB]:f2} "
                ),
                (
                    "My lower bound / my upper bound: ",
                    (matching, classID) => $"{matching.LowerBound / matching.UpperBound:f2}"
                ),
                (
                    "Their lower bound / their upper bound: ",
                    (matching, classID) => $"{matching.abLowerBounds[RiesenBunke2009AB] / matching.abUpperBounds[RiesenBunke2009AB]:f2}"
                ),
                (
                    "Their upper bound / my upper bound: ",
                    (matching, classID) => $"{matching.UpperBound / matching.abUpperBounds[RiesenBunke2009AB]:f2}"
                ),
                (
                    "My relative error, their relative error: ",
                    (matching, classID) => $"{(matching.UpperBound - matching.LowerBound)/matching.LowerBound:f2} {(matching.abUpperBounds[RiesenBunke2009AB] - matching.abLowerBounds[RiesenBunke2009AB])/matching.abLowerBounds[RiesenBunke2009AB]:f2}"
                ),
                (
                    $"Best lower bound: ",
                    (matching, classID) => $"A: {matching.BestLowerBoundA:f2} B: {matching.BestLowerBoundB:f2}"
                ),
                (
                    $"Best upper bound: ",
                    (matching, classID) => $"A: {matching.BestUpperBoundA:f2} B: {matching.BestUpperBoundB:f2}"
                ),
            };
            foreach (var (prefixMessage, greenDescription) in printouts)
            {
                for (int i = 0; i < matchingClassPairs.Count; i++)
                {
                    System.Console.Write(prefixMessage);
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write(greenDescription(matchingClassPairs[i].Item1, matchingClassPairs[i].Item2));
                    System.Console.ResetColor();
                    System.Console.WriteLine();
                }
            }
        }
    }
}
