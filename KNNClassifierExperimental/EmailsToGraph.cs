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
            foreach (var triplet in
                emails
                .Join(
                    context.DestinationEmails,
                    eo => eo.Id,
                    de => de.EmailId,
                    (eo, de) => new { eo, de }
                    )
                .Join(
                    context.EmailAddresses,
                    (pair) => pair.de.EmailAddressId,
                    ea => ea.Id,
                    (pair, ea) => new { pair.eo, ea } // email object and one of the senders
                    )
                .Where(pair => pair.ea.BelongsToEnron)
                .Join(
                    context.EmailAddresses,
                    (pair) => pair.eo.FromId,
                    ea => ea.Id,
                    (pair, ea) => new { pair.eo, pair.ea, eaFrom = ea }
                    )
                .Where(triplet => triplet.eaFrom.BelongsToEnron)
                )
            {
                var emailTo = triplet.ea;
                var email = triplet.eo;
                var fromAddress = triplet.eaFrom;

                enronAddresses.Add(fromAddress.Address);
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

            foreach (var enronAddress in enronAddresses)
                graph.AddVertex(enronAddress);

            foreach (var directedEmailKVP in directedEmails)
            {
                var edge = directedEmailKVP.Key;
                if (!graph.Directed && graph.ContainsEdge(edge))
                {
                    graph[edge] = Math.Min(graph[edge], directedEmailKVP.Value.Count);
                }
                else
                {
                    graph.AddEdge(edge, directedEmailKVP.Value.Count);
                }
            }

            return graph;
        }

    }
}