using AttributedGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using Postgres_enron_database.Data;
using KNNClassifier;
using AStarGraphNode;
using System.Text;

namespace KNNClassifierExperimental
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new EnronContext();
            Console.WriteLine($"{context.Emails.Count()} emails in total.");

            var vertexUpperBound = 6;

            var randomSeeds = new int[] { 0, 1, 2, 3 };

            var results = new Dictionary<(int k, string distanceScorerName), (double testAccuracy, double validationAccuracy)>();

            for (int iteration = 0; ; iteration++)
            {
                var dataset = GenerateDataSet(
                    context,
                    trainingProportion: 8,
                    validatingProportion: 1,
                    testingProportion: 1,
                    vertexUpperBound: vertexUpperBound,
                    randomSeed: iteration
                    );

                // var (testGraph, testGraphLabel) = dataset.testSet[random.Next(dataset.testSet.Count)];
                Func<VertexPartialMatchingNode<string, double, double>, double> matchingFeatureSelector = matching => matching.UpperBound;
                // Func<VertexPartialMatchingNode<string, double, double>, double> matchingFeatureSelector = matching => Math.Abs(matching.G.EdgeCount - matching.H.EdgeCount);

                var matchingParameters = GraphMatchingParameters<string, double, double>.DoubleCostComposer(
                        CostType.UnitCost,
                        CostType.UnitCost
                    );
                // matchingParameters.aCollection = new double[] { 1 };

                // determine closest neighbours for each test and validation graph
                var testMatchingClassPairsList = new List<(List<(VertexPartialMatchingNode<string, double, double>, bool)>, bool)>();
                var validationMatchingClassPairsList = new List<(List<(VertexPartialMatchingNode<string, double, double>, bool)>, bool)>();

                foreach (var (pair, i) in dataset.testSet.Zip(Enumerable.Range(0, int.MaxValue)))
                {
                    var pairToAdd = (
                            KNNClassifier.KNNClassifier.FindClosest(pair.Item1, dataset.trainingSet, matchingParameters, matchingFeatureSelector),
                            pair.Item2
                        );
                    testMatchingClassPairsList.Add(pairToAdd);
                    System.Console.WriteLine($"Test set: computed distance: {i * 100d / dataset.testSet.Count:f2}%.");
                }

                foreach (var (pair, i) in dataset.validationSet.Zip(Enumerable.Range(0, int.MaxValue)))
                {
                    var pairToAdd = (
                            KNNClassifier.KNNClassifier.FindClosest(pair.Item1, dataset.trainingSet, matchingParameters, matchingFeatureSelector),
                            pair.Item2
                        );
                    validationMatchingClassPairsList.Add(pairToAdd);
                    System.Console.WriteLine($"Validation set: computed distance: {i * 100d / dataset.validationSet.Count:f2}%.");
                }

                // TODO: provide an easy classification framework
                Func<int, Func<int, VertexPartialMatchingNode<string, double, double>, double>, List<(List<(VertexPartialMatchingNode<string, double, double>, bool)>, bool)>, double> getAccuracy = (k, distanceScorer, matchingClassPairsList) =>
                {
                    var truePositives = 0;
                    var falsePositives = 0;
                    var trueNegatives = 0;
                    var falseNegatives = 0;

                    foreach (var (matchingClassPairs, testGraphLabel) in matchingClassPairsList)
                    {

                        var classificationResult = KNNClassifier.KNNClassifier.Classify<string, double, double, bool>(
                            matchingClassPairs,
                            distanceScorer,
                            k: k
                            );

                        var result = (expected: testGraphLabel, received: classificationResult.graphClass);
                        if (result.expected)
                        {
                            if (result.received)
                                truePositives += 1;
                            else
                                falseNegatives += 1;
                        }
                        else
                        {
                            if (result.received)
                                falsePositives += 1;
                            else
                                trueNegatives += 1;
                        }
                    }

                    return (truePositives + trueNegatives) * 1d / matchingClassPairsList.Count;
                };

                var ks = new int[]
                {
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                -1
                };

                var distanceScorers = new (string, Func<int, VertexPartialMatchingNode<string, double, double>, double>)[]
                {
                ("Unit cost", (position, matching) => 1d),
                ("Double exponential lower bound", (position, matching) => Math.Exp(-matching.LowerBound * matching.LowerBound)),
                ("Single exponential lower bound", (position, matching) => Math.Exp(-matching.LowerBound)),
                ("Double exponential upper bound", (position, matching) => Math.Exp(-matching.UpperBound * matching.UpperBound)),
                ("Single exponential upper bound", (position, matching) => Math.Exp(-matching.UpperBound)),
                ("Quadratic lower bound", (position, matching) => -matching.LowerBound * matching.LowerBound),
                ("Quadratic upper bound", (position, matching) => -matching.UpperBound * matching.UpperBound),
                ("Lower bound", (position, matching) => -matching.LowerBound),
                ("Upper bound", (position, matching) => -matching.UpperBound),
                };


                foreach (var k in ks)
                {
                    foreach (var (distanceScorerName, distanceScorer) in distanceScorers)
                    {
                        var testAccuracy = getAccuracy(k > 0 ? k : dataset.testSet.Count, distanceScorer, testMatchingClassPairsList);
                        var validationAccuracy = getAccuracy(k > 0 ? k : dataset.validationSet.Count, distanceScorer, validationMatchingClassPairsList);
                        var key = (k, distanceScorerName);
                        if (iteration >= 1)
                        {
                            var newTestAccuracy = (results[key].testAccuracy * iteration + testAccuracy) / (iteration + 1);
                            var newValidationAccuracy = (results[key].validationAccuracy * iteration + validationAccuracy) / (iteration + 1);
                            results[key] = (newTestAccuracy, newValidationAccuracy);
                        }
                        else
                            results.Add(key, (testAccuracy, validationAccuracy));
                    }
                }

                foreach (var kvp in results.OrderBy(kvp => -kvp.Value.testAccuracy).Take(10).Reverse())
                {
                    System.Console.WriteLine($"Test accuracy: {kvp.Value.testAccuracy * 100d:f3}%. Validation accuracy: {kvp.Value.validationAccuracy * 100d:f3}%. k: {kvp.Key.k}. distanceScorer: {kvp.Key.distanceScorerName}");
                }

                System.Console.WriteLine($"Done with iteration {iteration}");
            }
            // DATA ANALYSIS

            // var emailsLocal = emails.AsEnumerable();
            // foreach (var group in emailsLocal.GroupBy(email => (email.SendDate.Year, email.SendDate.Month)).OrderBy(group => group.Key))
            // {
            //     System.Console.WriteLine($"{group.Key}: {group.Count()}");
            // }
            // var bankruptcyDate = DateTime.Parse("2001 Dec 03");
            // var bankruptcyTimespan = (DateTime.Parse("2001 Nov 01"), bankruptcyDate);
        }

        private enum DataSetCategory
        {
            Training,
            Validation,
            Testing
        }

        private static Graph<string, double, double> GetGraphFromDates(
            EnronContext context,
            DateTime dateFrom,
            DateTime dateTo
            )
        {
            var emailsFromDate = context.Emails.Where(email => email.SendDate > dateFrom && email.SendDate < dateTo);
            return EmailToGraph.GetGraph(context, emailsFromDate);
        }

        public static DataSet<string, double, double, bool> GenerateDataSet(
            EnronContext context,
            TimeSpan? daySplittingTimeAfter0000hrs = null,
            TimeSpan? dayLength = null,
            double trainingProportion = 8,
            double validatingProportion = 1,
            double testingProportion = 1,
            int vertexUpperBound = -1,
            int randomSeed = 0
            )
        {
            if (!daySplittingTimeAfter0000hrs.HasValue)
                daySplittingTimeAfter0000hrs = TimeSpan.FromHours(4);
            if (!dayLength.HasValue)
                dayLength = TimeSpan.FromHours(24);// could be longer than 24 hrs, but that's complicated
            var emails = context.Emails;

            Func<DateTime, DataSetCategory> datetimeToClass = datetime =>
            {
                var date = datetime - daySplittingTimeAfter0000hrs.Value;
                var random = new Random(
                        (new Random(randomSeed).Next() - 2_000_000)
                        + (date.Day + 31 * date.Month + 366 * date.Year)
                    );
                var totalProportion = (double)(
                                                trainingProportion
                                                + validatingProportion
                                                + testingProportion
                                                );
                var trainingThreshold = trainingProportion / totalProportion;
                var validatingThreshold = trainingThreshold + validatingProportion / totalProportion;

                var sample = random.NextDouble();
                if (sample < trainingThreshold)
                    return DataSetCategory.Training;
                else if (sample < validatingThreshold)
                    return DataSetCategory.Validation;
                else
                    return DataSetCategory.Testing;
            };

            var viableTimespan = (
                    from: DateTime.Parse("1999 Dec 01"),// first occurrence of exchanging more than 3000 emails per month
                    to: DateTime.Parse("2001 Jun 01")
                );
            var beginDate = viableTimespan.from + daySplittingTimeAfter0000hrs.Value;
            var endDate = viableTimespan.to + daySplittingTimeAfter0000hrs.Value;

            var dataSetToReturn = new DataSet<string, double, double, bool>();

            for (
                DateTime date = beginDate, dateEnd = beginDate + dayLength.Value;
                date < endDate;
                date += TimeSpan.FromHours(24), dateEnd += dayLength.Value
                )
            {
                var graph = GetGraphFromDates(context, date, dateEnd);

                if (graph.VertexCount < vertexUpperBound)
                    continue; // the graph is too small

                if (vertexUpperBound >= 0)
                {
                    graph.RemoveSmallestLast(vertexUpperBound);
                }

                var weekend = new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday };

                switch (datetimeToClass(date))
                {
                    case DataSetCategory.Training:
                        dataSetToReturn.trainingSet.Add((graph, weekend.Contains(date.DayOfWeek)));
                        break;
                    case DataSetCategory.Validation:
                        dataSetToReturn.validationSet.Add((graph, weekend.Contains(date.DayOfWeek)));
                        break;
                    case DataSetCategory.Testing:
                        dataSetToReturn.testSet.Add((graph, weekend.Contains(date.DayOfWeek)));
                        break;
                    default:
                        throw new NotImplementedException("Unknown dataset category");
                }

#if DEBUG
                System.Console.Write($"Finished {(date - beginDate).TotalDays * 100d / (endDate - beginDate).TotalDays:f2}%. ");
                System.Console.WriteLine(dataSetToReturn);
#endif
            }

            return dataSetToReturn;
        }
    }
}
