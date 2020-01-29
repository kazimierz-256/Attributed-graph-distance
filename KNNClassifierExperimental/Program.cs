using System;
using System.Linq;
using Postgres_enron_database.Data;

namespace KNNClassifierExperimental
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new EnronContext();
            var dateFrom = DateTime.Parse("2002 Dec 01");
            var dateTo = DateTime.Parse("2002 Dec 01");
            Console.WriteLine(context.Emails.Select(
                email => email.SendDate < dateTo && email.SendDate > dateFrom
                ).Count());
        }
    }
}
