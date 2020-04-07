using System;
using Xunit;
using System.Collections.Generic;
using AttributedGraph;
using AStarGraphNode;
using RandomGraphProvider;

namespace AStarGraphNodeTests
{
    public class VertexMatchingNodeTests
    {
        private static Func<double, double> bound = a => a / (1 + a);
        private Func<double, double, double> vertexRelabel = (a1, a2) =>
        {
            return bound(Math.Abs(a1 - a2));
        };
        private Func<double, double> vertexAdd = a => bound(Math.Abs(a));
        private Func<double, double> vertexRemove = a => bound(Math.Abs(a));

        private Func<double, double, double> edgeRelabel = (a1, a2) =>
        {
            return bound(Math.Abs(a1 - a2));
        };
        private Func<double, double> edgeAdd = a => bound(Math.Abs(a));
        private Func<double, double> edgeRemove = a => bound(Math.Abs(a));
        private int precision = 13;

        private void EncodingsMatch<V, VA, EA>(
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                Func<(V, VA)> vertexGenerator,
                GraphMatchingParameters<V, VA, EA> matchingParameters
                )
        {
            var gClone = G.Clone().Augment(G.VertexCount + H.VertexCount, vertexGenerator);
            var hClone = H.Clone().Augment(G.VertexCount + H.VertexCount, vertexGenerator);

            matchingParameters.encodingMethod = GraphEncodingMethod.Wojciechowski;
            var matching1 = new VertexPartialMatchingNode<V, VA, EA>(
                gClone,
                hClone,
                matchingParameters
            );
            matchingParameters.encodingMethod = GraphEncodingMethod.Wojciechowski;

            var matching3 = new VertexPartialMatchingNode<V, VA, EA>(
                G,
                H,
                matchingParameters
            );
            matchingParameters.encodingMethod = GraphEncodingMethod.RiesenBunke2009;
            var matching2 = new VertexPartialMatchingNode<V, VA, EA>(
                G,
                H,
                matchingParameters
            );

            Assert.Equal(matching1.LowerBound, matching2.LowerBound, precision);
            Assert.Equal(matching1.LowerBound, matching3.LowerBound, precision);
        }

        private VertexPartialMatchingNode<V, VA, EA> AugmentationMatches<V, VA, EA>(
                int m,
                Random random,
                Func<(V, VA)> vertexGenerator,
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                GraphMatchingParameters<V, VA, EA> matchingParameters
                )
        {
            var matching1 = new VertexPartialMatchingNode<V, VA, EA>(
                G,
                H,
                matchingParameters
            );
            var gClone = G.Clone();
            var hClone = H.Clone();
            Transform.Augment(gClone, m, vertexGenerator);
            Transform.Augment(hClone, m, vertexGenerator);

            var matching2 = new VertexPartialMatchingNode<V, VA, EA>(
                gClone,
                hClone,
                matchingParameters
            );

            Assert.Equal(matching1.LowerBound, matching2.LowerBound, precision);
            return matching1;
        }
        [Fact]
        public void AugmentationTests()
        {
            var random = new Random(3_14159265);
            double vertexAttributeGenerator() => random.NextDouble();
            
            double edgeAttributeGenerator() => random.NextDouble();

            (int, double) vertexGenerator() => (random.Next(), 0d);

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 7,
                density: .5,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator,
                random
                );

            var H = RandomGraphFactory.generateRandomInstance(
                vertices: 4,
                density: .7,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator,
                random
                );

            var a = new List<double>() { 1 };
            var b = new List<double>() { .5 };

            foreach (var encoding in new[] {
                    GraphEncodingMethod.Wojciechowski,
                    GraphEncodingMethod.RiesenBunke2009,
                })
            {
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
                    encodingMethod = encoding
                };
                var matching1 = AugmentationMatches(
                    Math.Max(G.VertexCount, H.VertexCount),
                    random,
                    vertexGenerator,
                    G,
                    H,
                    matchingParameters
                    );
                var matching2 = AugmentationMatches(
                    H.VertexCount + G.VertexCount,
                    random,
                    vertexGenerator,
                    G,
                    H,
                    matchingParameters
                    );
                Assert.Equal(matching1.LowerBound, matching2.LowerBound, precision);
            }
        }

        [Fact]
        public void CompletingEquivalence()
        {
            var random = new Random(3_14159265);
            double vertexAttributeGenerator() => random.NextDouble();
            double edgeAttributeGenerator() => random.NextDouble();

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 7,
                density: .5,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator,
                random
                );

            var H = RandomGraphFactory.generateRandomInstance(
                vertices: 4,
                density: .7,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator,
                random
                );

            var a = new List<double>() { 1 };
            var b = new List<double>() { .5 };

            (int, double) vertexGenerator() => (random.Next(), 0d);
            var matchingParameters = new GraphMatchingParameters<int, double, double>
            {
                aCollection = new List<double>() { 1 },
                bCollection = new List<double>() { .5 },
                edgeAdd = edgeAdd,
                vertexAdd = vertexAdd,
                edgeRelabel = edgeRelabel,
                edgeRemove = edgeRemove,
                vertexRemove = vertexRemove,
                vertexRelabel = vertexRelabel
            };
            EncodingsMatch(
                G,
                H,
                vertexGenerator,
                matchingParameters
                );
        }
    }
}
