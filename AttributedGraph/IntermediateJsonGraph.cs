using System.Collections.Generic;

namespace AttributedGraph
{
    internal class IntermediateJsonGraph
    {
        public Dictionary<string, string> vertex_attributes;
        public Dictionary<string, Dictionary<string, string>> edge_attributes;
        public string directed;
    }
}