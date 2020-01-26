using System;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    public interface INode : IComparable
    {
        double UpperBound { get; }
        double LowerBound { get; }

        List<INode> Expand();
    }
}
