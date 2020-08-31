using AlgorithmInternalBenchmark;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AStar
{
    public class AStarAlgorithm<T> where T : INode
    {
        public Action<string> Logger { get; set; }

        public Benchmark<string> Benchmark { get; }
        public SortedSet<T> Queue { get; }

        public AStarAlgorithm(T initialNode, Benchmark<string> benchmark = null)
        {
            if (benchmark == null)
                benchmark = new Benchmark<string>();
            Benchmark = benchmark;

            Queue = new SortedSet<T>(new AStarComparer<T>(benchmark))
            {
                initialNode
            };
        }

        public AStarAlgorithm(ICollection<T> initialNodes, Benchmark<string> benchmark = null)
        {
            if (initialNodes.Count == 0)
                throw new Exception("No nodes to expand for the A Star algorithm.");

            if (benchmark == null)
                benchmark = new Benchmark<string>();
            Benchmark = benchmark;

            Queue = new SortedSet<T>(new AStarComparer<T>(benchmark));

            foreach (var node in initialNodes)
                Queue.Add(node);
        }

        public T BestNode => Queue.Min;
        public T WorstNode => Queue.Max;

        ///// <summary>
        ///// Expands the best node, removes it, and inserts its descendants into the queue.
        ///// </summary>
        ///// <returns>Did the best node expand into more nodes (is there still nodes to expand)</returns>
        //public bool ExpandBestNode()
        //{
        //    var bestNode = BestNode;
        //    var expandedNodes = bestNode.Expand();

        //    foreach (var node in expandedNodes)
        //        Queue.Add((T)node);

        //    return expandedNodes.Count > 0;
        //}

        public T ExpandRecursively()
        {
            var absolutelyBestNode = BestNode;
            var lowestAnalyzedDistanceValue = absolutelyBestNode.DistanceFromSource();
            while (Queue.Count > 0)
            {
                var bestNode = BestNode;
                var nodeDistance = bestNode.DistanceFromSource();
                var nodeAstarValue = nodeDistance + bestNode.GetHeuristicValue();
                Logger?.Invoke($"Considering node having distance: {nodeDistance}. Astar value: {nodeAstarValue}. Queue size: {Queue.Count}");
                // the algorithm is more complex than vanilla A* since we might not reach the target node
                // (i.e. maximum common subgraph might not have as many nodes as either input graph)
                if (nodeDistance < lowestAnalyzedDistanceValue)
                {
                    lowestAnalyzedDistanceValue = nodeDistance;
                    absolutelyBestNode = bestNode;
                    Logger?.Invoke($"Found better solution: {lowestAnalyzedDistanceValue}");

                    Benchmark.StartBenchmark("Worst Node pruning");
                    var worstNode = WorstNode;
                    var removedNodesCount = 0;
                    while (worstNode.DistanceFromSource() + worstNode.GetHeuristicValue() > lowestAnalyzedDistanceValue)
                    {
                        Queue.Remove(worstNode);
                        worstNode = WorstNode;
                        removedNodesCount++;
                    }
                    Benchmark.StopBenchmark("Worst Node pruning");
                    if (removedNodesCount > 0)
                        Logger?.Invoke($"Removed {removedNodesCount} worst nodes");
                }
                else if (nodeAstarValue > lowestAnalyzedDistanceValue)
                    // there is absolutely no chance of finding a better solution
                    break;

                // expand best node
                Benchmark?.StartBenchmark("Expand outer");
                var expandedNodes = bestNode.Expand();
                Benchmark?.StopBenchmark("Expand outer");

                Benchmark?.StartBenchmark("Adding descendants");
                foreach (var node in expandedNodes)
                    Queue.Add((T)node);
                Benchmark?.StopBenchmark("Adding descendants");

                Queue.Remove(bestNode);
            }

            return absolutelyBestNode;
        }
    }
}
