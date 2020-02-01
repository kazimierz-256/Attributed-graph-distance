﻿using AttributedGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using Postgres_enron_database.Data;
using KNNClassifier;

namespace KNNClassifierExperimental
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new EnronContext();
            Console.WriteLine($"{context.Emails.Count()} emails in total.");

            var dataset = GenerateDataSet(context, vertexUpperBound: 10);
            System.Console.WriteLine(dataset);

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

        public static DataSet<string, double, double, DayOfWeek> GenerateDataSet(
            EnronContext context,
            TimeSpan? daySplittingTimeAfter0000hrs = null,
            TimeSpan? dayLength = null,
            double trainingProportion = 7,
            double validatingProportion = 2,
            double testingProportion = 3,
            int vertexUpperBound = -1
            )
        {
            if (!daySplittingTimeAfter0000hrs.HasValue)
                daySplittingTimeAfter0000hrs = TimeSpan.FromHours(4);
            if (!dayLength.HasValue)
                dayLength = TimeSpan.FromHours(24);// could be longer than 24 hrs, but that's complicated
            var emails = context.Emails;
            using var context2 = new EnronContext();
            var emails2 = context2.Emails;

            Func<DateTime, DataSetCategory> datetimeToClass = datetime =>
            {
                var date = datetime - daySplittingTimeAfter0000hrs.Value;
                var random = new Random(date.Day + 31 * date.Month + 366 * date.Year);
                var totalProportion = (double)(trainingProportion + validatingProportion + testingProportion);
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

            var dataSetToReturn = new DataSet<string, double, double, DayOfWeek>();

            for (
                DateTime date = beginDate, dateEnd = beginDate + dayLength.Value;
                date < endDate;
                date += TimeSpan.FromHours(24), dateEnd += dayLength.Value
                )
            {
                var emailsFromDate = emails2.Where(email => email.SendDate > date && email.SendDate < dateEnd);
                var graph = EmailToGraph.GetGraph(context, emailsFromDate);

                if (vertexUpperBound >= 0)
                {
                    graph.RemoveSmallestLast(vertexUpperBound);
// #if DEBUG
//                     System.Console.WriteLine($"{date.DayOfWeek}: {graph.EdgeCount}");
// #endif
                }

                switch (datetimeToClass(date))
                {
                    case DataSetCategory.Training:
                        dataSetToReturn.trainingSet.Add((graph, date.DayOfWeek));
                        break;
                    case DataSetCategory.Validation:
                        dataSetToReturn.validationSet.Add((graph, date.DayOfWeek));
                        break;
                    case DataSetCategory.Testing:
                        dataSetToReturn.testSet.Add((graph, date.DayOfWeek));
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
