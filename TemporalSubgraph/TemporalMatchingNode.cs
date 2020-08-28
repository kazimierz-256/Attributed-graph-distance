using AStar;
using AttributedGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;

namespace TemporalSubgraph
{
    public class TemporalMatchingNode<V, VA, EA> : INode where EA : IComparable
    {
        // keep both original graph, just in case
        private Graph<V, VA, EA> graph1;
        private Graph<V, VA, EA> graph2;

        // sorted matchings between vertices of G1 and their respecitve matching vertices from G2
        private SortedDictionary<V, V> alreadyMatchedVertices = new SortedDictionary<V, V>();
        private SortedDictionary<EA, EA> alreadyMatchedEdges = new SortedDictionary<EA, EA>();
        // bipartite graph of valid descendant matchings
        private BipartiteGraph<V, EA> bipartitePossibilities = new BipartiteGraph<V, EA>();
        // an independent provider that estimates (*from above*) the number of remaining matches from the bipartite possibility graph
        private IHeuristic<V, EA> heuristic;

        public TemporalMatchingNode(Graph<V, VA, EA> graph1, Graph<V, VA, EA> graph2, IHeuristic<V, EA> heuristic)
        {
            this.graph1 = graph1;
            this.graph2 = graph2;

            this.heuristic = heuristic;

            // initially, all vertices from G1 could possibly lead to 
            foreach (var u1 in graph1.Vertices.Keys)
                bipartitePossibilities.potentialConnections.Add(u1, new SortedSet<V>(graph2.Vertices.Keys));
        }

        private bool heuristicAlreadyComputed = false;
        private double cachedHeuristicValue = double.NaN;
        public double GetHeuristicValue()
        {
            if (!heuristicAlreadyComputed)
            {
                // compute the *negative* of upper bound by a heuristic provider
                cachedHeuristicValue = -1 * heuristic.Compute(bipartitePossibilities);
                heuristicAlreadyComputed = true;
            }

            return cachedHeuristicValue;
        }

        public double DistanceFromSource()
        {
            return -alreadyMatchedVertices.Count;
        }

        public V Matching(V vertex1)
        {
            return alreadyMatchedVertices[vertex1];
        }

        public List<INode> Expand()
        {
            var descendants = new List<INode>();

            // does not need to be from U1, does not need to be random
            // TODO: experiment with different vertex choosing heuristics
            // TODO: should choose form an "envelope" rather than randomly!
            var candidate1 = chooseCandidateVertex(bipartitePossibilities.potentialConnections.Keys);
            V chooseCandidateVertex(IEnumerable<V> candidates) => candidates.First();

            var alreadyMatchedEdgeList = new List<EA>(alreadyMatchedEdges.Keys);

            // match candidate1 with a valid candidate2 counterpart
            foreach (var candidate2 in bipartitePossibilities.potentialConnections[candidate1])
            {
                // copy and augment existing matchings
                // TODO: experiment with an immutable data structure for performance reasons
                var descendantMatchedVertices = new SortedDictionary<V, V>(alreadyMatchedVertices)
                {
                    { candidate1, candidate2 }
                };
                var descendantEdgeMatchings = new SortedDictionary<EA, EA>(alreadyMatchedEdges);

                // by definition, this is a valid matching

                // IMPORTANT TODO: augment temporal connections from neighbours from graph1
                var temporalOrderViolated = false;
                if (graph1.IncomingEdges.ContainsKey(candidate1))
                    foreach (var incomingVertex in graph1.IncomingEdges[candidate1])
                    {
                        if (alreadyMatchedVertices.ContainsKey(incomingVertex) && alreadyMatchedEdges.ContainsKey(graph1.Edges[(incomingVertex, candidate1)]))
                        {
                            // make sure the connection does not violate the temporal order, otherwise skip adding this to the descendant
                            var edgeAttribute = graph1.Edges[(incomingVertex, candidate1)];
                            var matchingEdgeAttribute = alreadyMatchedEdges[edgeAttribute];

                            var afterEdgeValueIndex = -alreadyMatchedEdgeList.BinarySearch(edgeAttribute);
                            var beforeEdgeValueIndex = afterEdgeValueIndex - 1;

                            if (beforeEdgeValueIndex >= 0)
                            {
                                var matchedBeforeEdgeValue = alreadyMatchedEdgeList[beforeEdgeValueIndex];
                                if (edgeAttribute.CompareTo(matchedBeforeEdgeValue) != matchingEdgeAttribute.CompareTo(alreadyMatchedEdges[matchedBeforeEdgeValue]))
                                {
                                    temporalOrderViolated = true;
                                    break;
                                }
                                else
                                {
                                    descendantEdgeMatchings.Add(edgeAttribute, matchingEdgeAttribute);
                                }
                            }

                            if (afterEdgeValueIndex < alreadyMatchedEdgeList.Count)
                            {
                                var matchedAfterEdgeValue = alreadyMatchedEdgeList[afterEdgeValueIndex];
                                if (edgeAttribute.CompareTo(matchedAfterEdgeValue) != matchingEdgeAttribute.CompareTo(alreadyMatchedEdges[matchedAfterEdgeValue]))
                                {
                                    temporalOrderViolated = true;
                                    break;
                                }
                                else
                                {
                                    descendantEdgeMatchings.Add(edgeAttribute, matchingEdgeAttribute);
                                }
                            }
                        }
                    }

                if (temporalOrderViolated)
                    continue;

                if (graph1.OutgoingEdges.ContainsKey(candidate1))
                    foreach (var outgoingVertex in graph1.OutgoingEdges[candidate1])
                    {
                        if (alreadyMatchedVertices.ContainsKey(outgoingVertex) && alreadyMatchedEdges.ContainsKey(graph1.Edges[(candidate1, outgoingVertex)]))
                        {
                            // make sure the connection does not violate the temporal order, otherwise skip adding this to the descendant
                            var edgeAttribute = graph1.Edges[(candidate1, outgoingVertex)];
                            var matchingEdgeAttribute = alreadyMatchedEdges[edgeAttribute];

                            var afterEdgeValueIndex = -alreadyMatchedEdgeList.BinarySearch(edgeAttribute);
                            var beforeEdgeValueIndex = afterEdgeValueIndex - 1;

                            if (beforeEdgeValueIndex >= 0)
                            {
                                var matchedBeforeEdgeValue = alreadyMatchedEdgeList[beforeEdgeValueIndex];
                                if (edgeAttribute.CompareTo(matchedBeforeEdgeValue) != matchingEdgeAttribute.CompareTo(alreadyMatchedEdges[matchedBeforeEdgeValue]))
                                {
                                    temporalOrderViolated = true;
                                    break;
                                }
                                else
                                {
                                    descendantEdgeMatchings.Add(edgeAttribute, matchingEdgeAttribute);
                                }
                            }

                            if (afterEdgeValueIndex < alreadyMatchedEdgeList.Count)
                            {
                                var matchedAfterEdgeValue = alreadyMatchedEdgeList[afterEdgeValueIndex];
                                if (edgeAttribute.CompareTo(matchedAfterEdgeValue) != matchingEdgeAttribute.CompareTo(alreadyMatchedEdges[matchedAfterEdgeValue]))
                                {
                                    temporalOrderViolated = true;
                                    break;
                                }
                                else
                                {
                                    descendantEdgeMatchings.Add(edgeAttribute, matchingEdgeAttribute);
                                }
                            }
                        }
                    }

                if (temporalOrderViolated)
                    continue;

                // TODO: consider leaving out the candidate1

                // all neighbouring temporal edges are valid, proceed with generating the descendant

                // TODO: generate the bipartite graph (remove potential connections... maybe integrate with previous steps?)

                // confirm descendant matching whether they are still valid: after augmenting connections from candidate1 their connections should remain valid
                var descendantBipartitePossibilities = new BipartiteGraph<V, EA>();

                foreach (var potentialConnection in bipartitePossibilities.potentialConnections)
                {
                    // eliminate the candidate1 and candidate2 connections
                    if (potentialConnection.Key.Equals(candidate1))
                        continue;

                    // given a connection
                    foreach (var connection2 in potentialConnection.Value)
                    {
                        // make sure its neighbouring connections (whenever matched) in graph1 do not violate the temporal order

                        if (graph1.IncomingEdges.ContainsKey(potentialConnection.Key))
                            foreach (var incomingVertex in graph1.IncomingEdges[potentialConnection.Key])
                            {
                                if (alreadyMatchedVertices.ContainsKey(incomingVertex) && alreadyMatchedEdges.ContainsKey(graph1.Edges[(incomingVertex, potentialConnection.Key)]))
                                {
                                    // make sure the connection does not violate the temporal order, otherwise skip adding this to the descendant
                                }
                            }
                        alreadyMatchedEdges.
                    }
                }

                // restrict the descendant bipartite graph
                // find an upper bound for bipartite matching between U1 and U2
                //var descendantBipartitePossibilities = new BipartiteGraph<V, EA>();
                //{
                //    // for each existing connection in sorted order
                //    var preexistingMatchIteratorAfter = bipartitePossibilities.potentialEdgeMatchings.GetEnumerator();
                //    var preexistingMatchCurrentBefore = preexistingMatchIteratorAfter.Current;
                //    var beforeIteratorStarted = false;
                //    var afterIteratorEnded = false;
                //    var existsElement = preexistingMatchIteratorAfter.MoveNext();
                //    if (!existsElement)
                //    {
                //        throw new Exception("There should be at least 1 preexistent matching!");
                //    }

                //    foreach (var candidateMatch in bipartitePossibilities.potentialEdgeMatchings)
                //    {
                //        if (candidateMatch.Value.FromVertex == candidate1 || candidateMatch.Value.ToVertex == candidate2)
                //            continue;

                //        // invariant: after.key is >= candidateMatch.key
                //        while (preexistingMatchIteratorAfter.Current.Key.CompareTo(candidateMatch.Key) < 0)
                //        {
                //            preexistingMatchCurrentBefore = preexistingMatchIteratorAfter.Current;
                //            beforeIteratorStarted = true;
                //            var nextElementExists = preexistingMatchIteratorAfter.MoveNext();
                //            if (!nextElementExists)
                //            {
                //                afterIteratorEnded = true;
                //                break;
                //            }
                //        }

                //        if (beforeIteratorStarted
                //            && candidateMatch.Key.CompareTo(preexistingMatchCurrentBefore.Key) != candidateMatch.Value.EdgeValue.CompareTo(preexistingMatchCurrentBefore.Value))
                //            continue;

                //        if (!afterIteratorEnded
                //            && candidateMatch.Key.CompareTo(preexistingMatchIteratorAfter.Current.Key) != candidateMatch.Value.EdgeValue.CompareTo(preexistingMatchIteratorAfter.Current.Value))
                //            continue;

                //        descendantBipartitePossibilities.potentialEdgeMatchings.Add(candidateMatch.Key, candidateMatch.Value);
                //    }
                //}

                var descendant = new TemporalMatchingNode<V, VA, EA>(
                    graph1,
                    graph2,
                    descendantMatchedVertices,
                    descendantEdgeMatchings,
                    descendantBipartitePossibilities,
                    heuristic
                );
                descendants.Add(descendant);
            }


            // TODO: consider removing candidate1

            //  try not matching candidate1 with anything

            // try each connection from the bipartite graph
            // mark down the new matching
            // carefully create descendant nodes

            foreach (var candidate2 in bipartitePossibilities.potentialConnections[candidate1])
            {
                if (bipartiteValidConnections.connections.Contains((candidate1, candidate2)))
                {
                    var descendantMatchings = new Dictionary<V, V>(alreadyMatchedVertices);
                    // verify the matching is existant and valid!
                    descendantMatchings.Add(candidate1, candidate2);
                    var newBipartiteValidConnections = RestrictBipartiteGraph(
                        descendantU1,
                        descendantU2,
                        bipartiteValidConnections,
                        descendantMatchings
                        );
                }
            }

            // remove candidate1 and expand its descendants here!

            return null.Expand();
        }

        public TemporalMatchingNode(
            Graph<V, VA, EA> graph1,
            Graph<V, VA, EA> graph2,
            SortedDictionary<V, V> alreadyMatchedVertices,
            SortedDictionary<EA, EA> alreadyMatchedEdges,
            BipartiteGraph<V, EA> bipartitePossibilities,
            IHeuristic<V, EA> heuristic)
        {
            this.graph1 = graph1;
            this.graph2 = graph2;
            this.alreadyMatchedVertices = alreadyMatchedVertices;
            this.alreadyMatchedEdges = alreadyMatchedEdges;
            this.bipartitePossibilities = bipartitePossibilities;
            this.heuristic = heuristic;
        }
    }
}
