using System;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    public enum UpperBoundApproximationType
    {
        Greedy,
        Random
    }
    public interface INode : IComparable
    {
        double UpperBound(UpperBoundApproximationType lowerBoundApproximationType = UpperBoundApproximationType.Best);
        double LowerBound();
        ICollection<INode> Expand();
    }
}
