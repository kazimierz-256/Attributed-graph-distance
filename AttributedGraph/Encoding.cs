using Newtonsoft.Json;
using System;

namespace AttributedGraph
{
    public class Encoding
    {
        public static Graph<V, VA, EA> EncodeJsonToGraph<V, VA, EA>(
            string jsonGraph,
            Func<string, V> vConverter,
            Func<string, VA> vaConverter,
            Func<string, EA> eaConverter
            )
        {
            var parsed = JsonConvert.DeserializeObject<IntermediateJsonGraph>(jsonGraph);
            var isDirected = bool.Parse(parsed.directed);
            var graphToReturn = new Graph<V, VA, EA>(isDirected);
            if (parsed.vertex_attributes != null)
            {
                foreach (var vertexAttributeKVP in parsed.vertex_attributes)
                {
                    var vertex = vConverter(vertexAttributeKVP.Key);
                    var attribute = vaConverter(vertexAttributeKVP.Value);
                    graphToReturn.AddVertex(vertex, attribute);
                }

                if (parsed.edge_attributes != null)
                    foreach (var vertexKVP in parsed.edge_attributes)
                    {
                        var vertex = vConverter(vertexKVP.Key);
                        foreach (var neighbourStringKVP in vertexKVP.Value)
                        {
                            var neighbour = vConverter(neighbourStringKVP.Key);
                            var edgeAttribute = eaConverter(neighbourStringKVP.Value);
                            graphToReturn.AddEdge((vertex, neighbour), edgeAttribute);
                        }
                    }
            }

            return graphToReturn;
        }
    }
}