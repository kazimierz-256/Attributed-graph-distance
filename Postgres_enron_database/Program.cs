using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Postgres_enron_database.Data;
using Postgres_enron_database.Models;
using LinqStatistics;

namespace Postgres_enron_database
{
    public class Program
    {
        private static void ClearDatabase(EnronContext context, bool clearEmailAddresses, bool clearEmailEntities, bool clearEmailDestinations)
        {
            Console.WriteLine("Preparing the clearing query...");

            if (clearEmailAddresses)
                context.EmailAddresses.RemoveRange(context.EmailAddresses);
            if (clearEmailEntities)
                context.Emails.RemoveRange(context.Emails);
            if (clearEmailDestinations)
                context.DestinationEmails.RemoveRange(context.DestinationEmails);

            Console.WriteLine("Removing elements from database...");
            SaveChanges(context);
            Console.WriteLine("Database cleared");

        }

        private static IEnumerable<string> YieldFileLines(string jsonURL)
        {
            using var file = new System.IO.StreamReader(jsonURL);
            // breaking out of the while loop is guaranteed if the file has finite length
            while (true)
            {
                var line = file.ReadLine();
                if (line == null)
                {
                    yield break;
                }
                else
                {
                    yield return line;
                }
            }
        }

        public static DateTime ParseDatabaseDate(string dateString) => Convert.ToDateTime(dateString[4..^5]);

        static void Main(string[] args)
        {
            using var context = new EnronContext();

            PrintThoroughDatabaseStatistics(context);

            //PrintDatabaseStatistics(context);
            //ClearDatabase(
            //    context,
            //    clearEmailAddresses: false,
            //    clearEmailEntities: false,
            //    clearEmailDestinations: false
            //    );
            //PrintDatabaseStatistics(context);

            //// Parsing
            //FillDatabase(
            //    context,
            //    @"C:\Users\Kazimierz\source\repos\Left To Categorize\2019\Podstawy Przetwarzania Danych\Magisterka Projekt\db_clean.txt",
            //    insertEmailAddresses: false,
            //    insertEmailEntities: false,
            //    insertEmailDestinations: false);
        }

        private static void PrintThoroughDatabaseStatistics(EnronContext context)
        {
            var emailaddressesThatSent = context.EmailAddresses
                .Where(ea => ea.BelongsToEnron)
                .Where(ea => context.Emails.Any(e => e.FromId == ea.Id))
                .ToHashSet();
            Console.WriteLine(emailaddressesThatSent.Count());
            var emailaddressSentEmailsPairs = emailaddressesThatSent
                .Zip(emailaddressesThatSent.Select(ea => context.Emails.Count(e => e.FromId == ea.Id)));

            var emails = context.Emails
                //.Where(e=> e.SendDate.Year > 1995)
                .Where(e =>
                context.EmailAddresses.Any(ea => ea.Id == e.FromId && ea.BelongsToEnron)
                )
                .OrderBy(e => e.SendDate)
                .ToList();

            var dates = emails
                .Select(e => e.SendDate)
                .GroupBy(sd => sd.Year)
                .ToList();

            foreach (var yearlyGrouping in dates)
            {
                Console.WriteLine($"Year: {yearlyGrouping.Key}, count: {yearlyGrouping.AsEnumerable().Count()}");
                foreach (var monthlyGrouping in yearlyGrouping.GroupBy(sd => sd.Month))
                {
                    var monthString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthlyGrouping.Key);
                    var monthlyCount = monthlyGrouping.AsEnumerable().Count();
                    Console.WriteLine($"Month: {monthString}, count: {monthlyCount}".PadRight(2));
                }
            }

            // retrieval performance test

            var sw = new Stopwatch();
            sw.Start();

            var retrievalDuration = TimeSpan.FromDays(1);
            var minDate = DateTime.Parse("01.01.2000");
            var maxDate = DateTime.Parse("01.01.2002") - retrievalDuration;
            var random = new Random();
            var length = 1000_000;
            var count = 0;
            for (int i = 0; i < length; i++)
            {
                var randomDate = random.NextDouble() * (maxDate - minDate).Days;
                var minRandomizedDate = minDate + TimeSpan.FromDays(randomDate);
                var maxRandomizedDate = minRandomizedDate + retrievalDuration;
                sw.Start();
                var relevantEmails = context.Emails.Where(e => context.EmailAddresses.Where(ea => ea.Id == e.FromId).First().BelongsToEnron && e.SendDate > minRandomizedDate && e.SendDate < maxRandomizedDate).ToList();
                sw.Stop();
                Console.WriteLine((i + 1) / sw.Elapsed.TotalSeconds);
                //var senders = relevantEmails.Select(email => email.FromId).Distinct();
                count += relevantEmails.Count(); //senders.Count();
                Console.WriteLine(count / (i + 1));
                //Console.WriteLine($"Distinct emails: {senders.Count()}");
            }

            //var emailNumber = 0;
            //foreach (var email in emails.Take(1000))
            //{
            //    Console.WriteLine($"{emailNumber} Email sent: {email.SendDate} URL: {email.URL}");
            //    emailNumber += 1;
            //}

            //var minDate = dates.Where(date => date.Year > 1980).Min();
            //var maxDate = dates.Max();
            //var avgDate = minDate + TimeSpan.FromDays(dates.Select(date => (date - minDate).TotalDays).Average());

            //var binCount = dates.BinCountSquareRoot();
            //var bins = dates.Histogram(binCount, datetime => (datetime - mindate).TotalDays);
            //foreach (var bin in bins)
            //{
            //    Console.WriteLine(bin.Count);
            //}
            //.Histogram(10);
            //var emailsAnalyzed = 0;
            //foreach (var (emailAddress, sentEmails) in emailaddressSentEmailsPairs)
            //{
            //    emailsAnalyzed += 1;
            //    Console.WriteLine($"Email: {emailAddress.Address} sent {sentEmails}. Speed: {emailsAnalyzed / se.Elapsed.TotalSeconds}");
            //}
            //foreach (var email in context.EmailAddresses
            //    .Where(ea => ea.BelongsToEnron).Take(10).ToList())
            //{
            //    Console.WriteLine($"Email: {email.Address}. Sent: {context.Emails.Where(e => e.FromId == email.Id).Count()}");
            //}
        }

        private static void PrintDatabaseStatistics(EnronContext context)
        {
            Console.WriteLine($"Email address count: {context.EmailAddresses.Count()}");
            Console.WriteLine($"Email entity count: {context.Emails.Count()}");
            Console.WriteLine($"Email destionation count: {context.DestinationEmails.Count()}");
            Console.WriteLine($"Enron Email address count: {context.EmailAddresses.Where(ea => ea.BelongsToEnron).Count()}");
        }

        private static void FillDatabase(
            EnronContext context,
            string jsonDatabaseURL,
            int printEveryNlines = 5000,
            bool insertEmailAddresses = false,
            bool insertEmailEntities = false,
            bool insertEmailDestinations = true)
        {
            if (insertEmailAddresses)
            {
                insertEmailEntities = true;
                insertEmailDestinations = true;
            }
            if (insertEmailEntities)
            {
                insertEmailDestinations = true;
            }

            var emailAddressEntities = new Dictionary<string, EmailAddress>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var linesProcessed = 0;

            void PrintProgress(string header)
            {
                if (linesProcessed % printEveryNlines == 0)
                    Console.WriteLine($"{header} - Processed lines:{linesProcessed}. Elapsed time: {stopwatch.Elapsed}. Speed: {Math.Floor(linesProcessed / stopwatch.Elapsed.TotalSeconds)}");
            }

            // insert all email addresses
            foreach (var line in YieldFileLines(jsonDatabaseURL))
            {
                linesProcessed += 1;
                var jsonObject = JsonSerializer.Deserialize<List<object>>(line);
                var from = JsonSerializer.Deserialize<List<string>>(jsonObject[0].ToString());
                var to = JsonSerializer.Deserialize<List<string>>(jsonObject[1].ToString());
                var cc = JsonSerializer.Deserialize<List<string>>(jsonObject[2].ToString());
                var bcc = JsonSerializer.Deserialize<List<string>>(jsonObject[3].ToString());

                // adding email addresses
                foreach (var emailAddress in from.Concat(to).Concat(cc).Concat(bcc))
                {
                    if (!emailAddressEntities.ContainsKey(emailAddress))
                    {
                        var emailAddressEntity = new EmailAddress
                        {
                            Address = emailAddress,
                            BelongsToEnron = IsEnronEmail(emailAddress)
                        };
                        if (insertEmailAddresses)
                            context.EmailAddresses.Add(emailAddressEntity);
                        emailAddressEntities.Add(emailAddress, emailAddressEntity);
                    }
                }

                if (insertEmailAddresses)
                    PrintProgress("Inserting email addresses.");
            }

            // performance-impactful operation
            if (insertEmailAddresses)
                SaveChanges(context);

            linesProcessed = 0;
            stopwatch.Restart();

            var emailEntitiesToProcess = new Dictionary<string, EmailObject>();

            // insert email entities
            foreach (var line in YieldFileLines(jsonDatabaseURL))
            {
                linesProcessed += 1;
                var jsonObject = JsonSerializer.Deserialize<List<object>>(line);
                var from = JsonSerializer.Deserialize<List<string>>(jsonObject[0].ToString());
                var date = ParseDatabaseDate(jsonObject[4].ToString());
                var url = jsonObject[5].ToString();

                if (from.Count == 1)
                {
                    // adding the email itself
                    var emailEntity = new EmailObject()
                    {
                        FromId = emailAddressEntities[from[0]].Id,
                        SendDate = date,
                        URL = url
                    };

                    emailEntitiesToProcess.Add(url, emailEntity);

                    if (insertEmailEntities)
                    {
                        context.Emails.Add(emailEntity);
                        PrintProgress("Inserting email entities.");
                    }
                }
                else
                {
                    Console.WriteLine($"Found an email having {from.Count} from addresses. Url: {url}");
                }
            }

            // performance-impactful operation
            if (insertEmailEntities)
                SaveChanges(context);

            linesProcessed = 0;
            printEveryNlines = 100;
            stopwatch.Restart();

            // inserting destination emails
            foreach (var line in YieldFileLines(jsonDatabaseURL))
            {
                linesProcessed += 1;
                var jsonObject = JsonSerializer.Deserialize<List<object>>(line);
                var to = JsonSerializer.Deserialize<List<string>>(jsonObject[1].ToString());
                var from = JsonSerializer.Deserialize<List<string>>(jsonObject[0].ToString());

                if (from.Count == 1)
                {
                    var cc = JsonSerializer.Deserialize<List<string>>(jsonObject[2].ToString());
                    var bcc = JsonSerializer.Deserialize<List<string>>(jsonObject[3].ToString());
                    var url = jsonObject[5].ToString();
                    // new C# 8 switch expression
                    var emailEntityId = insertEmailEntities switch
                    {
                        true => emailEntitiesToProcess[url].Id,
                        false => context.Emails.Where(e => e.URL == url).First().Id
                    };

                    IEnumerable<DestinationEmail> generateEmailTypesToDesitnationEmails(IEnumerable<string> emailAddresses, SendType sendType)
                    {
                        // covering a very rare case when someone sends to an email more than once
                        foreach (var emailAddress in emailAddresses.Distinct())
                        {
                            if (emailAddressEntities[emailAddress].Id == 0)
                            {
                                // almost always it means it is unassigned
                                emailAddressEntities[emailAddress].Id = context.EmailAddresses.Where(ea => ea.Address == emailAddress).First().Id;
                            }
                            yield return new DestinationEmail()
                            {
                                EmailId = emailEntityId,
                                EmailAddressId = emailAddressEntities[emailAddress].Id,
                                SendType = sendType,
                            };
                        }
                    }

                    // adding destination emails
                    var collectionOfDestinationEmails = generateEmailTypesToDesitnationEmails(to, SendType.TO)
                        .Concat(generateEmailTypesToDesitnationEmails(cc, SendType.CC))
                        .Concat(generateEmailTypesToDesitnationEmails(bcc, SendType.BCC))
                        .ToList();

                    if (insertEmailDestinations)
                    {
                        foreach (var email in collectionOfDestinationEmails)
                            context.DestinationEmails.Add(email);

                        PrintProgress("Inserting destination emails.");
                    }
                }
            }

            // performance-impactful operation
            if (insertEmailDestinations)
                SaveChanges(context);

            PrintDatabaseStatistics(context);
        }

        private static void SaveChanges(EnronContext context)
        {
            Console.WriteLine("Synchronizing with the database...");
            context.SaveChanges();
            Console.WriteLine("Database updated");
        }

        private static readonly Regex IsCleanEnronEmailRegex = new Regex(@"\w+(\.\w+)*@(\w+\.)*enron.com", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static bool IsEnronEmail(string email)
            => IsCleanEnronEmailRegex.IsMatch(email);
    }
}
