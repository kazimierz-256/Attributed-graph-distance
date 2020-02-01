using System;
using System.Collections.Generic;
using AttributedGraph;
using Postgres_enron_database.Models;
using Postgres_enron_database.Data;
using System.Linq;

namespace KNNClassifierExperimental
{
    public static class EmailToGraph
    {
        public static Graph<string, double, double> GetGraph(EnronContext context, IQueryable<EmailObject> emails)
        {
            var graph = new Graph<string, double, double>(directed: true);
            var enronAddresses = new HashSet<string>();
            var directedEmails = new Dictionary<(string, string), List<EmailObject>>();
            // for each email that day
            foreach (var email in emails)
            {
                // check if the sender is from Enron
                var fromAddress = context.EmailAddresses.Where(ea => ea.Id == email.FromId).First();
                if (fromAddress.BelongsToEnron)
                {
                    enronAddresses.Add(fromAddress.Address);
                    // check if the recipient is from Enron
                    foreach (var de in context.DestinationEmails
                        .Where(de => de.EmailId == email.Id)
                        .ToList()
                        )
                    {
                        var emailTo = context.EmailAddresses
                            .Where(ea => ea.Id == de.EmailAddressId && ea.BelongsToEnron)
                            .FirstOrDefault();
                        if (emailTo != default)
                        {
                            enronAddresses.Add(emailTo.Address);

                            var key = (fromAddress.Address, emailTo.Address);

                            if (directedEmails.ContainsKey(key))
                            {
                                // the same email could have been cloned to a different directory
                                if (directedEmails[key].FirstOrDefault(em => em.SendDate == email.SendDate) == default)
                                    directedEmails[key].Add(email);
                            }
                            else
                                directedEmails.Add(key, new List<EmailObject>() { email });
                        }
                    }
                }
            }

            foreach (var enronAddress in enronAddresses)
            {
                graph.AddVertex(enronAddress, 0);
            }
            foreach (var directedEmailKVP in directedEmails)
            {
                graph.AddEdge(directedEmailKVP.Key, directedEmailKVP.Value.Count);
            }

            return graph;
        }

    }
}