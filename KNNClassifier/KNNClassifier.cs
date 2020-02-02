using System;
using AStarGraphNode;
using System.Collections.Generic;
using AttributedGraph;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace KNNClassifier
{
    public class KNNClassifier
    {
        public static List<(VertexPartialMatchingNode<V, VA, EA> graph, C originalClass)> FindClosest<V, VA, EA, C>(
                Graph<V, VA, EA> G,
                List<(Graph<V, VA, EA>, C)> graphsPreclassified,
                GraphMatchingParameters<V, VA, EA> matchingParameters,
                Func<VertexPartialMatchingNode<V, VA, EA>, double> matchingFeatureSelector = null
                )
        {
            if (matchingFeatureSelector == null)
                matchingFeatureSelector = matching => matching.LowerBound;

            // determine distances between G and H graphs
            var matchingClassPairsBag = new ConcurrentBag<(VertexPartialMatchingNode<V, VA, EA>, C)>();

            Parallel.For(0, graphsPreclassified.Count, i =>
            {
                var (H, classID) = graphsPreclassified[i];
                var matching = new VertexPartialMatchingNode<V, VA, EA>(
                    G,
                    H,
                    matchingParameters
                );

                matchingClassPairsBag.Add((matching, classID));
            });
            var matchingClassPairs = matchingClassPairsBag.ToList();
            // foreach (var (H, classID) in graphsPreclassified)
            // {
            //     var matching = new VertexPartialMatchingNode<V, VA, EA>(
            //         G,
            //         H,
            //         matchingParameters
            //     );

            //     matchingClassPairs.Add((matching, classID));
            // }

            // determine the k closest graphs to G
            matchingClassPairs.Sort((pair1, pair2) => matchingFeatureSelector(pair1.Item1).CompareTo(matchingFeatureSelector(pair2.Item1)));

            return matchingClassPairs;
        }
        public static (C graphClass, List<(VertexPartialMatchingNode<V, VA, EA> graph, C originalClass)> matchedGraphs) Classify<V, VA, EA, C>(
                List<(VertexPartialMatchingNode<V, VA, EA> graph, C originalClass)> matchingClassPairs,
                Func<int, VertexPartialMatchingNode<V, VA, EA>, double> distanceScorer,
                int k = int.MaxValue
                )
        {
            var classScores = new Dictionary<C, double>();

            var fixedK = Math.Min(k, matchingClassPairs.Count);
            for (int i = 0; ;)
            {
                var (H, classID) = matchingClassPairs[i];

                if (!classScores.ContainsKey(classID))
                    classScores.Add(classID, 0);

                classScores[classID] += distanceScorer(i, H);

                // stop if the following graphs have index > k and they are farther from the query graph
                i += 1;
                if (i >= fixedK)
                {
                    if (i < matchingClassPairs.Count && distanceScorer(i, matchingClassPairs[i].Item1) == distanceScorer(fixedK - 1, matchingClassPairs[fixedK - 1].Item1))
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
