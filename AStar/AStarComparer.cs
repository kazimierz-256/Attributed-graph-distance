using System.Collections.Generic;

namespace AStar
{
    public class AStarComparer<T> : IComparer<T> where T : INode
    {
        public int Compare(T x, T y)
        {
            var priority1 = (x.DistanceFromSource() + x.GetHeuristicValue()).CompareTo(y.DistanceFromSource() + y.GetHeuristicValue());
            if (priority1 != 0)
                return priority1;
            var priority2 = x.DistanceFromSource().CompareTo(y.DistanceFromSource());
            return priority2;
        }
    }
}