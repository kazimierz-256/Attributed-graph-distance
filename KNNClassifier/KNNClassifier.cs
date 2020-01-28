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
                List<(Graph<V, VA, EA>, int)> graphsPreclassified,
                Func<VA, VA, double> vertexAttributeMetric,
                Func<EA, EA, double> edgeAttributeMetric,
                List<double> a,
                List<double> b,
                GraphEncodingMethod encodingMethod = GraphEncodingMethod.Wojciechowski)
        {
            // determine distances between G and H graphs
            var distancesClasses = new List<(double, int)>();
            for (int i = 0; i < graphsPreclassified.Count; i++)
            {
                var (H, classID) = graphsPreclassified[i];
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

            // choose k closest graphs
            distancesClasses.Sort((pair1, pair2) => pair1.Item1.CompareTo(pair2.Item1));
            var classCount = new Dictionary<int, int>();
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
            // determine the class of G
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
