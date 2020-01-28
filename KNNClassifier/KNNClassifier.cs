using System;
using AStarGraphNode;
using System.Collections.Generic;
using AttributedGraph;

namespace KNNClassifier
{
    public class KNNClassifier
    {
        public static int Classify<V, VA, EA>(
                int k,
                Graph<V, VA, EA> G,
                ICollection<(Graph<V, VA, EA>, int)> graphsPreclassified,
                Func<VA, VA, double> vertexAttributeMetric,
                Func<EA, EA, double> edgeAttributeMetric,
                ICollection<double> a,
                ICollection<double> b,
                GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski)
        {
            // determine distances between G and H graphs
            var distancesClasses = new List<(double, int)>();

            foreach (var (H, classID) in graphsPreclassified)
            {
                var matching = new VertexPartialMatchingNode<V, VA, EA>(
                    G,
                    H,
                    vertexAttributeMetric,
                    edgeAttributeMetric,
                    a,
                    b,
                    encodingMethod: encodingMethod
                );

                distancesClasses.Add((matching.LowerBound, classID));
            }

            // determine the k closest graphs to G
            distancesClasses.Sort((pair1, pair2) => pair1.Item1.CompareTo(pair2.Item1));
            var classCount = new Dictionary<int, int>();
            
            // TODO: what if k+1th element and beyond have the same distance as kth element?
            for (int i = 0; i < k; i++)
            {
                var (H, classID) = distancesClasses[i];
                if (classCount.ContainsKey(classID))
                {
                    classCount[i] += 1;
                }
                else
                {
                    classCount.Add(i, 1);
                }
            }

            // determine the class of G (most frequently occuring within the k closest graphs)
            var majorityClass = -1;
            var majorityCount = -1;

            foreach (var kvp in classCount)
            {
                if (kvp.Value > majorityCount)
                {
                    majorityCount = kvp.Value;
                    majorityClass = kvp.Key;
                }
            }

            return majorityClass;
        }
    }
}
