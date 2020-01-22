using System;
using System.Collections.Generic;
using System.Text;

namespace LinearAssignmentSolver
{
    public static class LAPSolver
    {
        public static int[] SolveAssignment(double[,] costMatrix)
            => new HungarianAlgorithm(costMatrix).Run();
        public static double AssignmentCost(double[,] costMatrix, int[] assignment)
        {
            var sum = .0;
            for (int i = 0; i < assignment.Length; i++)
                sum += costMatrix[i, assignment[i]];
            return sum;
        }
    }
}
