using System;
using System.Collections.Generic;

namespace AStar
{
    public class AStarAlgorithm<T> where T : INode

    {
        public SortedSet<T> Queue { get; } = new SortedSet<T>(new AStarComparer<T>());

        public AStarAlgorithm(T initialNode)
        {
            Queue.Add(initialNode);
        }

        public AStarAlgorithm(ICollection<T> initialNodes)
        {
            if (initialNodes.Count == 0)
                throw new Exception("No nodes to expand for the A Star algorithm.");

            foreach (var node in initialNodes)
                Queue.Add(node);
        }

        public T BestNode => Queue.Min;

        /// <summary>
        /// Expands the best node, removes it, and inserts its descendants into the queue.
        /// </summary>
        /// <returns>Did the best node expand into more nodes (is there still nodes to expand)</returns>
        public bool ExpandBestNode()
        {
            var bestNode = BestNode;
            var expandedNodes = bestNode.Expand();

            foreach (var node in expandedNodes)
                Queue.Add((T)node);

            return expandedNodes.Count > 0;
        }
    }
}
