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
        double UpperBound(UpperBoundApproximationType upperBoundApproximationType);
        double LowerBound { get; }

        ICollection<INode> Expand();
    }
}
