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
        private Func<double, double, double> vertexRelabel = (a1, a2) =>
        {
            return Math.Abs(a1 - a2);
        };
        private Func<double, double> vertexAdd = a => Math.Abs(a);
        private Func<double, double> vertexRemove = a => Math.Abs(a);

        private Func<double, double, double> edgeRelabel = (a1, a2) =>
        {
            return Math.Abs(a1 - a2);
        };
        private Func<double, double> edgeAdd = a => Math.Abs(a);
        private Func<double, double> edgeRemove = a => Math.Abs(a);
        private int precision = 13;

        private void EncodingsMatch<V, VA, EA>(
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                Func<VA, double> vertexAdd,
                Func<VA, VA, double> vertexRelabel,
                Func<VA, double> vertexRemove,
                Func<EA, double> edgeAdd,
                Func<EA, EA, double> edgeRelabel,
                Func<EA, double> edgeRemove,
                ICollection<double> aCollection,
                ICollection<double> bCollection,
                GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski
                )
        {
            var matching1 = new VertexPartialMatchingNode<V, VA, EA>(
                G,
                H,
                vertexAdd,
                vertexRelabel,
                vertexRemove,
                edgeAdd,
                edgeRelabel,
                edgeRemove,
                aCollection,
                aCollection,
                encodingMethod: GraphEncodingMethod.Wojciechowski
            );
            var matching2 = new VertexPartialMatchingNode<V, VA, EA>(
                G,
                H,
                vertexAdd,
                vertexRelabel,
                vertexRemove,
                edgeAdd,
                edgeRelabel,
                edgeRemove,
                aCollection,
                aCollection,
                encodingMethod: GraphEncodingMethod.RiesenBunke
            );

            Assert.Equal(matching1.LowerBound, matching2.LowerBound, precision);
        }

        private VertexPartialMatchingNode<V, VA, EA> AugmentationMatches<V, VA, EA>(
                int m,
                Random random,
                Func<(V, VA)> vertexGenerator,
                Graph<V, VA, EA> G,
                Graph<V, VA, EA> H,
                Func<VA, double> vertexAdd,
                Func<VA, VA, double> vertexRelabel,
                Func<VA, double> vertexRemove,
                Func<EA, double> edgeAdd,
                Func<EA, EA, double> edgeRelabel,
                Func<EA, double> edgeRemove,
                ICollection<double> aCollection,
                ICollection<double> bCollection,
                GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski
                )
        {
            var matching1 = new VertexPartialMatchingNode<V, VA, EA>(
                G,
                H,
                vertexAdd,
                vertexRelabel,
                vertexRemove,
                edgeAdd,
                edgeRelabel,
                edgeRemove,
                aCollection,
                aCollection,
                encodingMethod: encodingMethod
            );
            var gClone = Transform.Clone(G);
            var hClone = Transform.Clone(H);
            Transform.Augment(gClone, m, vertexGenerator);
            Transform.Augment(hClone, m, vertexGenerator);

            var matching2 = new VertexPartialMatchingNode<V, VA, EA>(
                gClone,
                hClone,
                vertexAdd,
                vertexRelabel,
                vertexRemove,
                edgeAdd,
                edgeRelabel,
                edgeRemove,
                aCollection,
                aCollection,
                encodingMethod: encodingMethod
            );

            Assert.Equal(matching1.UpperBound, matching2.UpperBound, precision);
            return matching1;
        }
        [Fact]
        public void AugmentationTests()
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
            Func<(int, double)> vertexGenerator = () => (random.Next(), 0d);

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 7,
                density: .5,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );

            var H = RandomGraphFactory.generateRandomInstance(
                vertices: 4,
                density: .7,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );

            var a = new List<double>() { .5 };
            var b = new List<double>() { .5 };

            foreach (var encoding in new[] { GraphEncodingMethod.Wojciechowski, GraphEncodingMethod.RiesenBunke })
            {
                var matching1 = AugmentationMatches<int, double, double>(
                    Math.Max(G.VertexCount, H.VertexCount),
                    random,
                    vertexGenerator,
                    G,
                    H,
                    vertexAdd,
                    vertexRelabel,
                    vertexRemove,
                    edgeAdd,
                    edgeRelabel,
                    edgeRemove,
                    a,
                    b,
                    encodingMethod: encoding
                    );
                var matching2 = AugmentationMatches<int, double, double>(
                    G.VertexCount + H.VertexCount,
                    random,
                    vertexGenerator,
                    G,
                    H,
                    vertexAdd,
                    vertexRelabel,
                    vertexRemove,
                    edgeAdd,
                    edgeRelabel,
                    edgeRemove,
                    a,
                    b,
                    encodingMethod: encoding
                    );
                Assert.Equal(matching1.UpperBound, matching2.UpperBound, precision);
            }
        }

        [Fact]
        public void CompletingEquivalence()
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

            var G = RandomGraphFactory.generateRandomInstance(
                vertices: 7,
                density: .5,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );

            var H = RandomGraphFactory.generateRandomInstance(
                vertices: 4,
                density: .7,
                directed: true,
                vertexAttributeGenerator: vertexAttributeGenerator,
                edgeAttributeGenerator: edgeAttributeGenerator
                );

            var a = new List<double>() { .5 };
            var b = new List<double>() { .5 };

            EncodingsMatch<int, double, double>(
                G,
                H,
                vertexAdd,
                vertexRelabel,
                vertexRemove,
                edgeAdd,
                edgeRelabel,
                edgeRemove,
                a,
                b);
        }
    }
}
