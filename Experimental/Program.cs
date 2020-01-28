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
            const int hGraphCount = 50;
            
            var graphsPreclassified = new List<(Graph<int, double, double>, int)>();
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

            // var a = new List<double>() { 0, 1, .5, 1d/3, 2d/3, -1, 2, 10, -10, 100, -100, 1000, -1000 };
            // var b = new List<double>() { 0, 1, .5, 1d/3, 2d/3, -1, 2, 10, -10, 100, -100, 1000, -1000 };
            var a = new List<double>() { 0, 1, .5};
            var b = new List<double>() { 0, 1, .5};

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
            
            var RiesenBunkeAB = (1,1);

            var printouts = new List<(string, Func<VertexPartialMatchingNode<int, double, double>, int, string>)>()
            {
                (
                    "Class: ",
                    (matching, classID) => $"{classID} "
                ),
                (
                    "My lower bound / theirs: ",
                    (matching, classID) => $"{matching.LowerBound / matching.abLowerBounds[RiesenBunkeAB]:f2} "
                ),
                (
                    "My lower bound / my upper bound: ",
                    (matching, classID) => $"{matching.LowerBound / matching.UpperBound:f2}"
                ),
                (
                    "Their lower bound / their upper bound: ",
                    (matching, classID) => $"{matching.abLowerBounds[RiesenBunkeAB] / matching.abUpperBounds[RiesenBunkeAB]:f2}"
                ),
                (
                    "Their upper bound / my upper bound: ",
                    (matching, classID) => $"{matching.UpperBound / matching.abUpperBounds[RiesenBunkeAB]:f2}"
                ),
                (
                    "My relative error, their relative error: ",
                    (matching, classID) => $"{(matching.UpperBound - matching.LowerBound)/matching.LowerBound:f2} {(matching.abUpperBounds[RiesenBunkeAB] - matching.abLowerBounds[RiesenBunkeAB])/matching.abLowerBounds[RiesenBunkeAB]:f2}"
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
