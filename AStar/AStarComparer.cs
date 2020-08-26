using System.Collections.Generic;

namespace AStar
{
    public class AStarComparer<T> : IComparer<INode>
    {
        public int Compare(INode x, INode y) => (x.DistanceFromSource() + x.GetHeuristicValue()).CompareTo(y.DistanceFromSource() + y.GetHeuristicValue());
    }
}