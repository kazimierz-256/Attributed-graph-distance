using System.Collections.Generic;
using AttributedGraph;

namespace KNNClassifier
{
    public class DataSet<V, VA, EA, Label>
    {
        public List<(Graph<V, VA, EA>, Label)> trainingSet = new List<(Graph<V, VA, EA>, Label)>();
        public List<(Graph<V, VA, EA>, Label)> validationSet = new List<(Graph<V, VA, EA>, Label)>();
        public List<(Graph<V, VA, EA>, Label)> testSet = new List<(Graph<V, VA, EA>, Label)>();

        public override string ToString()
            => $"Training count: {trainingSet.Count}. Validating count: {validationSet.Count}. Testing count: {testSet.Count}";
    }
}