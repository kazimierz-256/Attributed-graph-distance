using System;
using System.Collections.Generic;

namespace AStar
{
    public class AStarAlgorithm
    {
        public SortedSet<INode> Queue { get; } = new SortedSet<INode>(new AStarComparer<INode>());
        public AStarAlgorithm(INode initialNode) : this(new[] { initialNode })
        {
        }

        public AStarAlgorithm(ICollection<INode> initialNodes)
        {
            if (initialNodes.Count == 0)
                throw new Exception("No nodes to expand for the A Star algorithm.");

            foreach (var node in initialNodes)
                Queue.Add(node);
        }

        public INode BestNode => Queue.Min;

        /// <summary>
        /// Expands the best node, removes it, and inserts its descendants into the queue.
        /// </summary>
        /// <returns>Did the best node expand into more nodes (is there still nodes to expand)</returns>
        public bool ExpandBestNode()
        {
            var bestNode = BestNode;
            var expandedNodes = bestNode.Expand();
            Queue.Remove(bestNode);
            foreach (var node in expandedNodes)
                Queue.Add(node);

            return expandedNodes.Count > 0;
        }
    }
}
