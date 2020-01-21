using System;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    public interface INode
    {
        double UpperBound();
        double LowerBound();
        IEnumerable<INode> Expand();
    }
}
