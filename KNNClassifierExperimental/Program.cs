using System;
using System.Collections.Generic;
using System.Linq;
using Postgres_enron_database.Data;

namespace KNNClassifierExperimental
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new EnronContext();
            var beginDate = DateTime.Parse("1999 Dec 01");// first occurrence of exchanging more than 3000 emails per month
            var bankruptcyDate = DateTime.Parse("2001 Dec 03");
            var daySplittingHour = 4;
            var years = Enumerable.Range(2000, 2);
            var emails = context.Emails;
            var emailsLocal = emails.AsEnumerable();
            Console.WriteLine(context.Emails.Count());
            // foreach (var group in emailsLocal.GroupBy(email => (email.SendDate.Year, email.SendDate.Month)).OrderBy(group => group.Key))
            // {
            //     System.Console.WriteLine($"{group.Key}: {group.Count()}");
            // }
            var viableTimespan = ("1999 Dec 01", "2001 Jun 01");
            var bankruptcyTimespan = ("2001 Nov 01", "2001 Dec 03");

            var trainingProportion = 5;
            var validatingProportion = 2;
            var testingProportion = 3;

            Func<DateTime, int> datetimeToClass = datetime =>
            {
                var date = datetime - TimeSpan.FromHours(daySplittingHour);
                var random = new Random(date.Day + 31 * date.Month + 366 * date.Year);
                var totalProportion = (double)(testingProportion + validatingProportion + testingProportion);
                var trainingThreshold = trainingProportion / totalProportion;
                var validatingThreshold = trainingThreshold + validatingProportion / totalProportion;
                var sample = random.NextDouble();
                if (sample < trainingThreshold)
                    return 0;
                else if (sample < validatingThreshold)
                    return 1;
                else
                    return 2;
            };

            var randomDay = $"2000 Mar 20 {daySplittingHour}:00";
            var randomDateFrom = DateTime.Parse(randomDay);
            var randomDateTo = randomDateFrom.AddHours(24);
            var emailsFromRandomDay = emails.Where(email => email.SendDate > randomDateFrom && email.SendDate < randomDateTo);
            System.Console.WriteLine(emailsFromRandomDay.Count());

            var graph = EmailToGraph.GetGraph(context, emailsFromRandomDay);

        }
    }
}
