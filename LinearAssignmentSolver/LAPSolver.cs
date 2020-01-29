using System;
using System.Collections.Generic;
using System.Text;

namespace LinearAssignmentSolver
{
    public static class LAPSolver
    {
        public static int[] SolveAssignment(double[,] costMatrix)
            => new HungarianAlgorithm(costMatrix).Run();
        public static int[] SolveAssignmentMax(double[,] costMatrix)
        {
            var negatedWeights = new double[costMatrix.GetLength(0), costMatrix.GetLength(1)];
            for (int i = 0; i < costMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < costMatrix.GetLength(1); j++)
                {
                    negatedWeights[i, j] = -costMatrix[i, j];
                }
            }
            return new HungarianAlgorithm(negatedWeights).Run();
        }
        public static double AssignmentCost(double[,] costMatrix, int[] assignment)
        {
            var sum = .0;
            for (int i = 0; i < assignment.Length; i++)
                sum += costMatrix[i, assignment[i]];
            return sum;
        }
    }
}
