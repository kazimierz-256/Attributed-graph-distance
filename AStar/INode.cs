using System;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    public interface INode : IComparable
    {
        double UpperBound();
        double LowerBound();
        ICollection<INode> Expand();
    }
}
