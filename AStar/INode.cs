using System;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    public interface INode
    {
        double GetHeuristicValue();
        double DistanceFromSource();

        List<INode> Expand();
    }
}
