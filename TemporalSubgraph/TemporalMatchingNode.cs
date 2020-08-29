﻿using AStar;
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

        // sorted dually-matched edge values between the graphs, allows for checking descendant consistency
        private SortedDictionary<EA, EA> alreadyMatchedEdges = new SortedDictionary<EA, EA>();

        // bipartite graph of valid descendant potential matchings
        private BipartiteGraph<V, EA> bipartitePossibilities = new BipartiteGraph<V, EA>();

        // an independent provider that estimates (*from above*) the number of remaining matches from the bipartite possibility graph
        private IHeuristic<V, EA> heuristic;

        public TemporalMatchingNode(Graph<V, VA, EA> graph1, Graph<V, VA, EA> graph2, IHeuristic<V, EA> heuristic)
        {
            this.graph1 = graph1;
            this.graph2 = graph2;

            this.heuristic = heuristic;

            // initially, all vertices from G1 lead to a valid match with vertices from G2
            foreach (var u1 in graph1.Vertices.Keys)
                bipartitePossibilities.potentialConnections.Add(u1, new SortedSet<V>(graph2.Vertices.Keys));
        }

        // caching the heuristic value
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
            return -1 * alreadyMatchedVertices.Count;
        }

        // peek at the final matching
        public V Matching(V vertex1)
        {
            return alreadyMatchedVertices[vertex1];
        }

        public List<INode> Expand()
        {
            if (bipartitePossibilities.potentialConnections.Keys.Count == 0)
                return new List<INode>(capacity: 0);

            var descendants = new List<INode>();

            // does not need to be from U1, does not need to be random
            // TODO: experiment with different vertex choosing heuristics
            // TODO: should choose form an "envelope" rather than randomly!
            V candidate1 = default;
            var candidate1assigned = false;
            // ensure the next vertex is a neighbour
            foreach (var candidate in bipartitePossibilities.potentialConnections.Keys)
            {
                var validCandidate = true;
                if (alreadyMatchedVertices.Count > 0)
                {
                    validCandidate = false;
                    if (graph1.IncomingEdges.ContainsKey(candidate))
                        foreach (var otherVertex in graph1.IncomingEdges[candidate])
                        {
                            if (alreadyMatchedVertices.ContainsKey(otherVertex))
                            {
                                validCandidate = true;
                                break;
                            }
                        }
                    if (!validCandidate && graph1.OutgoingEdges.ContainsKey(candidate))
                        foreach (var otherVertex in graph1.OutgoingEdges[candidate])
                        {
                            if (alreadyMatchedVertices.ContainsKey(otherVertex))
                            {
                                validCandidate = true;
                                break;
                            }
                        }
                }
                if (validCandidate)
                {
                    candidate1 = candidate;
                    candidate1assigned = true;
                    break;
                }
            }

            if (!candidate1assigned)
                return new List<INode>(capacity: 0);

            // this vertex should be connected to 

            var alreadyMatchedEdgeList = new List<EA>(alreadyMatchedEdges.Keys);
            alreadyMatchedEdgeList.Sort();

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


                bool isTemporalOrderViolated(List<EA> edgeList, SortedDictionary<EA, EA> edgeMatchings, EA edgeAttribute, EA matchingEdgeAttribute)
                {
                    var afterEdgeValueIndex = ~edgeList.BinarySearch(edgeAttribute);
                    var beforeEdgeValueIndex = afterEdgeValueIndex - 1;

                    if (beforeEdgeValueIndex >= 0)
                    {
                        var matchedBeforeEdgeValue = edgeList[beforeEdgeValueIndex];
                        if (edgeAttribute.CompareTo(matchedBeforeEdgeValue) != matchingEdgeAttribute.CompareTo(edgeMatchings[matchedBeforeEdgeValue]))
                        {
                            return true;
                        }
                    }

                    if (afterEdgeValueIndex < edgeList.Count)
                    {
                        var matchedAfterEdgeValue = edgeList[afterEdgeValueIndex];
                        if (edgeAttribute.CompareTo(matchedAfterEdgeValue) != matchingEdgeAttribute.CompareTo(edgeMatchings[matchedAfterEdgeValue]))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                var temporalMatchingToBeAdded = new SortedDictionary<EA, EA>();

                var temporalOrderViolated = false;
                if (graph1.IncomingEdges.ContainsKey(candidate1))
                    foreach (var incomingVertex in graph1.IncomingEdges[candidate1])
                    {
                        if (alreadyMatchedVertices.ContainsKey(incomingVertex))
                        {
                            // make sure the connection does not violate the temporal order, otherwise skip adding this to the descendant
                            var edgeAttribute = graph1.Edges[(incomingVertex, candidate1)];
                            var matchingEdge = (alreadyMatchedVertices[incomingVertex], candidate2);
                            if (graph2.ContainsEdge(matchingEdge))
                            {
                                var matchingEdgeAttribute = graph2.Edges[matchingEdge];

                                descendantEdgeMatchings.Add(edgeAttribute, matchingEdgeAttribute);
                                temporalMatchingToBeAdded.Add(edgeAttribute, matchingEdgeAttribute);
                            }
                        }
                    }

                if (temporalOrderViolated)
                    continue;

                if (graph1.OutgoingEdges.ContainsKey(candidate1))
                    foreach (var outgoingVertex in graph1.OutgoingEdges[candidate1])
                    {
                        if (alreadyMatchedVertices.ContainsKey(outgoingVertex))
                        {
                            // make sure the connection does not violate the temporal order, otherwise skip adding this to the descendant
                            var edgeAttribute = graph1.Edges[(candidate1, outgoingVertex)];
                            var matchingEdge = (candidate2, alreadyMatchedVertices[outgoingVertex]);
                            if (graph2.ContainsEdge(matchingEdge))
                            {
                                var matchingEdgeAttribute = graph2.Edges[matchingEdge];

                                descendantEdgeMatchings.Add(edgeAttribute, matchingEdgeAttribute);
                                temporalMatchingToBeAdded.Add(edgeAttribute, matchingEdgeAttribute);
                            }
                        }
                    }

                if (temporalOrderViolated)
                    continue;

                // all neighbouring temporal edges are valid, proceed with generating the descendant

                // TODO: generate the bipartite graph (remove potential connections... maybe integrate with previous steps?)
                var descendantEdgeMatchingList = new List<EA>(descendantEdgeMatchings.Keys);
                descendantEdgeMatchingList.Sort();

                // confirm descendant matching whether they are still valid: after augmenting connections from candidate1 their connections should remain valid
                var descendantBipartitePossibilities = new BipartiteGraph<V, EA>();

                foreach (var potentialConnection in bipartitePossibilities.potentialConnections)
                {
                    // eliminate the candidate1 and candidate2 connections
                    if (potentialConnection.Key.Equals(candidate1))
                        continue;

                    // given a potentially valid connection
                    foreach (var connection2 in potentialConnection.Value)
                    {
                        if (connection2.Equals(candidate2))
                            continue;

                        // make sure its neighbouring connections (whenever matched) in graph1 do not violate the (new) temporal order
                        temporalOrderViolated = false;

                        if (descendantEdgeMatchingList.Count > 0 && graph1.IncomingEdges.ContainsKey(potentialConnection.Key))
                            foreach (var incomingVertex in graph1.IncomingEdges[potentialConnection.Key])
                            {
                                if (descendantMatchedVertices.ContainsKey(incomingVertex))
                                {
                                    var edge = graph1.Edges[(incomingVertex, potentialConnection.Key)];

                                    var counterpartEdge = (descendantMatchedVertices[incomingVertex], connection2);
                                    if (graph2.ContainsEdge(counterpartEdge))
                                    {
                                        if (isTemporalOrderViolated(descendantEdgeMatchingList, descendantEdgeMatchings, edge, graph2.Edges[counterpartEdge]))
                                        {
                                            temporalOrderViolated = true;
                                            break;
                                        }
                                    }
                                }
                            }

                        if (temporalOrderViolated)
                            continue;

                        if (descendantEdgeMatchingList.Count > 0 && graph1.OutgoingEdges.ContainsKey(potentialConnection.Key))
                            foreach (var outgoingVertex in graph1.OutgoingEdges[potentialConnection.Key])
                            {
                                if (descendantMatchedVertices.ContainsKey(outgoingVertex))
                                {
                                    var edge = graph1.Edges[(potentialConnection.Key, outgoingVertex)];

                                    var counterpartEdge = (connection2, descendantMatchedVertices[outgoingVertex]);
                                    if (graph2.ContainsEdge(counterpartEdge))
                                    {
                                        if (isTemporalOrderViolated(descendantEdgeMatchingList, descendantEdgeMatchings, edge, graph2.Edges[counterpartEdge]))
                                        {
                                            temporalOrderViolated = true;
                                            break;
                                        }
                                    }
                                }
                            }

                        if (temporalOrderViolated)
                            continue;

                        if (!descendantBipartitePossibilities.potentialConnections.ContainsKey(potentialConnection.Key))
                            descendantBipartitePossibilities.potentialConnections.Add(potentialConnection.Key, new SortedSet<V>());
                        descendantBipartitePossibilities.potentialConnections[potentialConnection.Key].Add(connection2);
                    }
                }

                var descendant = new TemporalMatchingNode<V, VA, EA>(
                    graph1,
                    graph2,
                    descendantMatchedVertices,// taken care of
                    descendantEdgeMatchings,// taken care of
                    descendantBipartitePossibilities,// TODO
                    heuristic
                );
                descendants.Add(descendant);
            }


            // also consider removing candidate1
            // simply create a descendant without candidate1
            var bipartitePossibilitiesWithoutCandidate1 = bipartitePossibilities.Clone();
            bipartitePossibilitiesWithoutCandidate1.potentialConnections.Remove(candidate1);
            var descendantWithoutCandidate1 = new TemporalMatchingNode<V, VA, EA>(
                graph1,
                graph2,
                new SortedDictionary<V, V>(alreadyMatchedVertices),
                new SortedDictionary<EA, EA>(alreadyMatchedEdges),
                bipartitePossibilitiesWithoutCandidate1,
                heuristic
                );
            descendants.Add(descendantWithoutCandidate1);

            return descendants;
        }

        // convenient way to create a descendant node
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
