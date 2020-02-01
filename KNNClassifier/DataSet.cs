using System.Collections.Generic;
using AttributedGraph;

namespace KNNClassifier
{
    public class DataSet<V, VA, EA>
    {
        public List<Graph<V, VA, EA>> trainingSet = new List<Graph<V, VA, EA>>();
        public List<Graph<V, VA, EA>> validationSet = new List<Graph<V, VA, EA>>();
        public List<Graph<V, VA, EA>> testSet = new List<Graph<V, VA, EA>>();

        public override string ToString()
            => $"Training count: {trainingSet.Count}. Validating count: {validationSet.Count}. Testing count: {testSet.Count}";
    }
}