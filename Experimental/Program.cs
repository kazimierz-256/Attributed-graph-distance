using AStarGraphNode;
using RandomGraphProvider;
using System;
using System.Collections.Generic;
using KNNClassifier;
using AttributedGraph;
using System.Linq;

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
                return boundMetric(Math.Abs(a1 - a2), vertexBound, vertexDecay);
            };
            Func<double, double> vertexAdd = a => boundMetric(Math.Abs(a), vertexBound, vertexDecay);
            Func<double, double> vertexRemove = vertexAdd;

            Func<double, double, double> edgeRelabel = (a1, a2) =>
            {
                return boundMetric(Math.Abs(a1 - a2), edgeBound, edgeDecay);
            };
            Func<double, double> edgeAdd = a => boundMetric(Math.Abs(a), edgeBound, edgeDecay);
            Func<double, double> edgeRemove = edgeAdd;



            // Func<double, double, double> vertexRelabel = (a1, a2) =>
            // {
            //     return Math.Abs(a1 - a2);
            // };
            // Func<double, double> vertexAdd = a => Math.Abs(a);
            // Func<double, double> vertexRemove = vertexAdd;

            // Func<double, double, double> edgeRelabel = (a1, a2) =>
            // {
            //     return Math.Abs(a1 - a2);
            // };
            // Func<double, double> edgeAdd = a => Math.Abs(a);
            // Func<double, double> edgeRemove = edgeAdd;

            var a = new List<double>() { 0, 1, .5, 1d / 3, 2d / 3, -1, 2, 10, -10, 100, -100, 1000, -1000 };
            var b = new List<double>() { 0, 1, .5, 1d / 3, 2d / 3, -1, 2, 10, -10, 100, -100, 1000, -1000 };
            var RiesenBunke2009AB = (1, 1);



            for (int gVertices = 8; gVertices < 12; gVertices++)
            {
                var hVertices = gVertices;
                // for (int hVertices = gVertices; hVertices > 0; hVertices-=1)
                {
                    var measurements = 0;
                    var results = new Dictionary<(double, double), double>();
                    foreach (var aElement in a)
                        foreach (var bElement in b)
                            results.Add((aElement, bElement), 0);
                    for (double gDensity = 1.0; gDensity > 0; gDensity -= 0.1)
                    {
                        for (double hDensity = gDensity; hDensity > 0; hDensity -= 0.1)
                        {
                            for (int iter = 0; iter < 5; iter++)
                            {
                                var G = RandomGraphFactory.generateRandomInstance(
                                    vertices: gVertices,
                                    density: gDensity,
                                    directed: true,
                                    vertexAttributeGenerator: vertexAttributeGenerator,
                                    edgeAttributeGenerator: edgeAttributeGenerator
                                    );
                                var H = RandomGraphFactory.generateRandomInstance(
                                    vertices: hVertices,
                                    density: hDensity,
                                    directed: true,
                                    vertexAttributeGenerator: vertexAttributeGenerator,
                                    edgeAttributeGenerator: edgeAttributeGenerator
                                    );

                                var matchingParameters = new GraphMatchingParameters<int, double, double>
                                {
                                    aCollection = a,
                                    bCollection = b,
                                    edgeAdd = edgeAdd,
                                    vertexAdd = vertexAdd,
                                    edgeRelabel = edgeRelabel,
                                    edgeRemove = edgeRemove,
                                    vertexRemove = vertexRemove,
                                    vertexRelabel = vertexRelabel,
                                    encodingMethod = GraphEncodingMethod.Wojciechowski
                                };
                                var matching = new VertexPartialMatchingNode<int, double, double>(
                                    G,
                                    H,
                                    matchingParameters
                                );

                                var myRelativeError = (matching.UpperBound - matching.LowerBound) / matching.LowerBound;
                                var theirRelativeError = (matching.UpperBound - matching.abLowerBounds[RiesenBunke2009AB]) / matching.abLowerBounds[RiesenBunke2009AB];
                                var eps = 1e-12;
                                // if (theirRelativeError > eps)
                                // {
                                //     System.Console.WriteLine($"|Vg|={gVertices}, |Eg|={gDensity:f2}, |Vh|={hVertices}, |Eh|={hDensity:f2}. My estimate / theirs {matching.LowerBound / matching.abLowerBounds[RiesenBunke2009AB]:f2}.");

                                // }

                                // var theirLowerBound = matching.abLowerBounds[(2d/3, .5)];
                                // if (theirLowerBound > eps)
                                {
                                    measurements += 1;
                                    foreach (var kvp in matching.abLowerBounds)
                                    {
                                        // var score = (matching.UpperBound - kvp.Value) / matching.UpperBound;
                                        var score = kvp.Value > Math.Max(Math.Max(
                                            matching.abLowerBounds[(.5, .5)],
                                            matching.abLowerBounds[(1d / 3, .5)]),
                                            matching.abLowerBounds[(2d / 3, .5)]
                                            ) ? 1 : 0;
                                        // if (!double.IsNaN(score))
                                        results[kvp.Key] += score;
                                    }
                                }

                            }
                        }
                    }

                    var resultsNames = results.Keys.ToArray();
                    var resultsScores = resultsNames.Select(key => -results[key]).ToArray();
                    Array.Sort(resultsScores, resultsNames);
                    System.Console.WriteLine(gVertices);
                    System.Console.Write($"(1, 1):{results[(1, 1)] / measurements:f2}  ");
                    for (int i = 0; i < 6; i++)
                    {
                        var (aa, bb) = resultsNames[i];
                        System.Console.Write($"({aa:f2}, {bb:f2}):{-resultsScores[i] / measurements:f2}  ");
                    }
                    System.Console.WriteLine();
                }
            }

            // var graphsPreclassified = new List<(Graph<int, double, double>, int)>();
            // // graphsPreclassified.Add((Transform.Permute(G, random), 0));
            // // var anotherPermutation = Transform.Permute(G, random);
            // // anotherPermutation.AddVertex(-1, 0);
            // // anotherPermutation.AddVertex(-2, 0);
            // // anotherPermutation.AddVertex(-3, 0);
            // // graphsPreclassified.Add((anotherPermutation, 1));
            // var a = new List<double>() { 0, 1, .5};
            // var b = new List<double>() { 0, 1, .5};

            // var k = 3;

            // var encodingMethod = GraphEncodingMethod.Wojciechowski;

            // var (gClassID, matchingClassPairs) = KNNClassifier.KNNClassifier.Classify<int, double, double>(
            //     k: k,
            //     G,
            //     graphsPreclassified,
            //     vertexAdd,
            //     vertexRelabel,
            //     vertexRemove,
            //     edgeAdd,
            //     edgeRelabel,
            //     edgeRemove,
            //     a,
            //     b,
            //     encodingMethod: encodingMethod
            // );

            // System.Console.WriteLine($"Detected class: {gClassID}");

            // var RiesenBunke2009AB = (1, 1);

            // var printouts = new List<(string, Func<VertexPartialMatchingNode<int, double, double>, int, string>)>()
            // {
            //     (
            //         "Class: ",
            //         (matching, classID) => $"{classID} "
            //     ),
            //     (
            //         "My lower bound / theirs: ",
            //         (matching, classID) => $"{matching.LowerBound / matching.abLowerBounds[RiesenBunke2009AB]:f2} "
            //     ),
            //     (
            //         "My lower bound / my upper bound: ",
            //         (matching, classID) => $"{matching.LowerBound / matching.UpperBound:f2}"
            //     ),
            //     (
            //         "Their lower bound / their upper bound: ",
            //         (matching, classID) => $"{matching.abLowerBounds[RiesenBunke2009AB] / matching.abUpperBounds[RiesenBunke2009AB]:f2}"
            //     ),
            //     (
            //         "Their upper bound / my upper bound: ",
            //         (matching, classID) => $"{matching.UpperBound / matching.abUpperBounds[RiesenBunke2009AB]:f2}"
            //     ),
            //     (
            //         "My relative error, their relative error: ",
            //         (matching, classID) => $"{(matching.UpperBound - matching.LowerBound)/matching.LowerBound:f2} {(matching.abUpperBounds[RiesenBunke2009AB] - matching.abLowerBounds[RiesenBunke2009AB])/matching.abLowerBounds[RiesenBunke2009AB]:f2}"
            //     ),
            //     (
            //         $"Best lower bound: ",
            //         (matching, classID) => $"A: {matching.BestLowerBoundA:f2} B: {matching.BestLowerBoundB:f2}"
            //     ),
            //     (
            //         $"Best upper bound: ",
            //         (matching, classID) => $"A: {matching.BestUpperBoundA:f2} B: {matching.BestUpperBoundB:f2}"
            //     ),
            // };
            // foreach (var (prefixMessage, greenDescription) in printouts)
            // {
            //     for (int i = 0; i < matchingClassPairs.Count; i++)
            //     {
            //         System.Console.Write(prefixMessage);
            //         System.Console.ForegroundColor = ConsoleColor.Green;
            //         System.Console.Write(greenDescription(matchingClassPairs[i].Item1, matchingClassPairs[i].Item2));
            //         System.Console.ResetColor();
            //         System.Console.WriteLine();
            //     }
            // }
        }
    }
}
