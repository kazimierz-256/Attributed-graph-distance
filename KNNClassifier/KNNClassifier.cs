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
                Func<int, double, double> distanceScorer,
                Graph<V, VA, EA> G,
                IEnumerable<(Graph<V, VA, EA>, C)> graphsPreclassified,
                GraphMatchingParameters<V, VA, EA> matchingParameters,
                int k = int.MaxValue,
                Func<VertexPartialMatchingNode<V, VA, EA>, double> matchingFeatureSelector = null
                )
        {
            if (matchingFeatureSelector == null)
                matchingFeatureSelector = matching => matching.LowerBound;

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
            matchingClassPairs.Sort((pair1, pair2) => matchingFeatureSelector(pair1.Item1).CompareTo(matchingFeatureSelector(pair2.Item1)));

            var classScores = new Dictionary<C, double>();

            var fixedK = Math.Min(k, matchingClassPairs.Count);
            for (int i = 0; ;)
            {
                var (H, classID) = matchingClassPairs[i];

                if (!classScores.ContainsKey(classID))
                    classScores.Add(classID, 0);

                classScores[classID] += distanceScorer(i, matchingFeatureSelector(H));

                // stop if the following graphs have index > k and they are farther from the query graph
                i += 1;
                if (i >= fixedK)
                {
                    if (i < matchingClassPairs.Count && matchingFeatureSelector(matchingClassPairs[i].Item1) == matchingFeatureSelector(matchingClassPairs[fixedK - 1].Item1))
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
            var classScoringMaximum = default(C);
            var maxScore = double.MinValue;

            foreach (var kvp in classScores)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    classScoringMaximum = kvp.Key;
                }
            }

            return (classScoringMaximum, matchingClassPairs);
        }
    }
}
