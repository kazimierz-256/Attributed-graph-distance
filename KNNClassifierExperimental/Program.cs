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

            // var resultsFilePath = "./results.txt";
            var topResultsFilePath = "./top_results.txt";
            var topTopResultsFilePath = "./top_top_results.txt";

            var matchingFeatureSelectors = new List<(string, Func<VertexPartialMatchingNode<string, double, double>, double>)>()
            {
                ("Upper bound", matching => matching.BestUpperBound),
                ("Lower bound", matching => matching.BestLowerBound)
            };

            var edgeCostTypes = new List<(string, CostType, List<double>, List<double>)>()
            {
                // ("Unit cost Wojciechowski .5", CostType.UnitCost, new List<double>(){.5}, new List<double>(){.5}),
                // ("Unit cost Wojciechowski (1.) .5", CostType.UnitCost, new List<double>(){1}, new List<double>(){.5}),
                // ("Unit cost Wojciechowski .5 Riesen Bunke 1.", CostType.UnitCost, new List<double>(){.5, 1}, new List<double>(){1, .5}),
                // ("Unit cost Riesen Bunke 1.", CostType.UnitCost, new List<double>(){1}, new List<double>(){1}),

                ("Absolute value Wojciechowski .5", CostType.AbsoluteValue, new List<double>(){.5}, new List<double>(){.5}),
                // ("Absolute value Wojciechowski (1.) .5", CostType.AbsoluteValue, new List<double>(){1}, new List<double>(){.5}),
                // ("Absolute value Wojciechowski .5 Riesen Bunke 1.", CostType.AbsoluteValue, new List<double>(){.5, 1}, new List<double>(){1, .5}),
                ("Absolute value Riesen Bunke 1.", CostType.AbsoluteValue, new List<double>(){1}, new List<double>(){1}),

                // ("Absolute value bounded Wojciechowski .5", CostType.AbsoluteValueBounded, new List<double>(){.5}, new List<double>(){.5}),
                // ("Absolute value bounded Wojciechowski (1.) .5", CostType.AbsoluteValueBounded, new List<double>(){1}, new List<double>(){.5}),
                // ("Absolute value bounded Wojciechowski .5 Riesen Bunke 1.", CostType.AbsoluteValueBounded, new List<double>(){.5, 1}, new List<double>(){1, .5}),
                // ("Absolute value bounded Riesen Bunke 1.", CostType.AbsoluteValueBounded, new List<double>(){1}, new List<double>(){1}),
            };

            var results = new Dictionary<(int vertexUpperBound, int k, string distanceScorerName, string edgeCostType, string matchingFeatureSelectorName), (double testAccuracy, double validationAccuracy)>();
            var topResults = new Dictionary<(int vertexUpperBound, int k, string distanceScorerName, string edgeCostType, string matchingFeatureSelectorName), (double testAccuracy, double validationAccuracy)>();

            for (int iteration = 0; iteration < 10; iteration++)
            {
                for (int vertexUpperBound = 3; vertexUpperBound < 8; vertexUpperBound++)
                {
                    var dataset = GenerateDataSet(
                        context,
                        trainingProportion: 8,
                        validatingProportion: 1,
                        testingProportion: 1,
                        vertexUpperBound: vertexUpperBound,
                        randomSeed: iteration
                        );
                    foreach (var (matchingFeatureSelectorName, matchingFeatureSelector) in matchingFeatureSelectors)
                    {
                        foreach (var (edgeCostTypeName, edgeCostType, aCollection, bCollection) in edgeCostTypes)
                        {

                            var matchingParameters = GraphMatchingParameters<string, double, double>.DoubleCostComposer(
                                CostType.UnitCost,
                                edgeCostType
                            );
                            matchingParameters.aCollection = aCollection;
                            matchingParameters.bCollection = bCollection;

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
                                // System.Console.WriteLine($"Test set: computed distance: {i * 100d / dataset.testSet.Count:f2}%.");
                            }

                            foreach (var (pair, i) in dataset.validationSet.Zip(Enumerable.Range(0, int.MaxValue)))
                            {
                                var pairToAdd = (
                                        KNNClassifier.KNNClassifier.FindClosest(pair.Item1, dataset.trainingSet, matchingParameters, matchingFeatureSelector),
                                        pair.Item2
                                    );
                                validationMatchingClassPairsList.Add(pairToAdd);
                                // System.Console.WriteLine($"Validation set: computed distance: {i * 100d / dataset.validationSet.Count:f2}%.");
                            }

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
                                ("Position linear decay", (position, matching) => 1d / position),
                                ("Position quadratic decay", (position, matching) => 1d / (position * position)),
                                ("Position single exponential decay", (position, matching) => Math.Exp(-position)),
                                ("Position double exponential decay", (position, matching) => Math.Exp(-position * position)),
                                ("Double exponential lower bound", (position, matching) => Math.Exp(-matching.BestLowerBound * matching.BestLowerBound)),
                                ("Single exponential lower bound", (position, matching) => Math.Exp(-matching.BestLowerBound)),
                                ("Double exponential upper bound", (position, matching) => Math.Exp(-matching.BestUpperBound * matching.BestUpperBound)),
                                ("Single exponential upper bound", (position, matching) => Math.Exp(-matching.BestUpperBound)),
                                ("Quadratic lower bound", (position, matching) => -matching.BestLowerBound * matching.BestLowerBound),
                                ("Quadratic upper bound", (position, matching) => -matching.BestUpperBound * matching.BestUpperBound),
                                ("Lower bound", (position, matching) => -matching.BestLowerBound),
                                ("Upper bound", (position, matching) => -matching.BestUpperBound),
                            };


                            foreach (var k in ks)
                            {
                                foreach (var (distanceScorerName, distanceScorer) in distanceScorers)
                                {
                                    var testAccuracy = getAccuracy(k > 0 ? k : dataset.testSet.Count, distanceScorer, testMatchingClassPairsList);
                                    var validationAccuracy = getAccuracy(k > 0 ? k : dataset.validationSet.Count, distanceScorer, validationMatchingClassPairsList);
                                    var key = (vertexUpperBound, k, distanceScorerName, edgeCostTypeName, matchingFeatureSelectorName);
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


                            // entire summary
                            var winningSummary = String.Empty;
                            // var topResult = results.OrderByDescending(kvp => kvp.Value.testAccuracy).First();
                            // topResults.Add(topResult.Key, topResult.Value);

                            // using var file = new System.IO.StreamWriter(resultsFilePath, true);
                            foreach (var kvp in results.OrderBy(kvp => kvp.Value.testAccuracy).TakeLast(10))
                            {
                                var summary = $"Graph vertices: {vertexUpperBound}. Iteration: {iteration}. Test accuracy: {kvp.Value.testAccuracy * 100d:f3}%. Validation accuracy: {kvp.Value.validationAccuracy * 100d:f3}%. k: {kvp.Key.k}. scoring function: {kvp.Key.distanceScorerName}. Feature selector: {matchingFeatureSelectorName}. Edge cost type: {edgeCostTypeName}.";

                                winningSummary = summary;

                                // System.Console.WriteLine(summary);

                                // file.WriteLine(summary);
                            }
                            // file.WriteLine();

                            // top summary
                            using var topResultsFile = new System.IO.StreamWriter(topResultsFilePath, true);
                            topResultsFile.WriteLine(winningSummary);

                            using var topTopResultsFile = new System.IO.StreamWriter(topTopResultsFilePath);

                            foreach (var (kvp, i) in results.OrderByDescending(result => result.Value.testAccuracy).Take(5).Zip(Enumerable.Range(1, int.MaxValue)))
                            {
                                var summary = $"Graph vertices: {vertexUpperBound}. Iteration: {iteration}. Test accuracy: {kvp.Value.testAccuracy * 100d:f3}%. Validation accuracy: {kvp.Value.validationAccuracy * 100d:f3}%. k: {kvp.Key.k}. scoring function: {kvp.Key.distanceScorerName}. Feature selector: {matchingFeatureSelectorName}. Edge cost type: {edgeCostTypeName}.";

                                topTopResultsFile.WriteLine($"Top {i}: {summary}");
                            }
                        }
                    }
                }
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
