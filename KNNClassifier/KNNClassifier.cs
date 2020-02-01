using System;
using AStarGraphNode;
using System.Collections.Generic;
using AttributedGraph;
using System.Linq;

namespace KNNClassifier
{
    public class KNNClassifier
    {
        public static (C, List<(VertexPartialMatchingNode<V, VA, EA>, C)>) Classify<V, VA, EA, C>(
                int k,
                Graph<V, VA, EA> G,
                IEnumerable<(Graph<V, VA, EA>, C)> graphsPreclassified,
                GraphMatchingParameters<V, VA, EA> matchingParameters
                )
        {
            // determine distances between G and H graphs
            var matchingClassPairs = new List<(VertexPartialMatchingNode<V, VA, EA>, C)>();

            foreach (var (H, classID) in graphsPreclassified)
            {
                var matching = new VertexPartialMatchingNode<V, VA, EA>(
                    G,
                    H,
                    matchingParameters
                );

                matchingClassPairs.Add((matching, classID));
            }

            // determine the k closest graphs to G
            matchingClassPairs.Sort((pair1, pair2) => pair1.Item1.LowerBound.CompareTo(pair2.Item1.LowerBound));

            var classCount = new Dictionary<C, int>();

            var fixedK = Math.Min(k, matchingClassPairs.Count);
            for (int i = 0; ;)
            {
                var (H, classID) = matchingClassPairs[i];
                if (classCount.ContainsKey(classID))
                {
                    classCount[classID] += 1;
                }
                else
                {
                    classCount.Add(classID, 1);
                }

                // stop if the following graphs have index > k and they are farther from the query graph
                i += 1;
                if (i >= fixedK)
                {
                    if (i < matchingClassPairs.Count && matchingClassPairs[i].Item1.LowerBound == matchingClassPairs[fixedK - 1].Item1.LowerBound)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // determine the class of G (most frequently occuring within the k closest graphs)
            var majorityClass = default(C);
            var majorityCount = -1;

            foreach (var kvp in classCount)
            {
                if (kvp.Value > majorityCount)
                {
                    majorityCount = kvp.Value;
                    majorityClass = kvp.Key;
                }
            }

            return (majorityClass, matchingClassPairs);
        }
    }
}
