using System.Collections.Generic;

namespace AStar
{
    public class AStarComparer<T> : IComparer<T> where T : INode
    {
        public int Compare(T x, T y) => (x.DistanceFromSource() + x.GetHeuristicValue()).CompareTo(y.DistanceFromSource() + y.GetHeuristicValue());
    }
}