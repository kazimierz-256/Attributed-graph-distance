using LinearAssignmentSolver;
using System;
using System.Diagnostics;

namespace LinearAssignmentPerformanceBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            double[,] generateRandomMatrix(int n)
            {
                var random = new Random();
                var matrix = new double[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        matrix[i, j] = Math.Log(Math.Cos(i) + 2) / Math.Pow(3 + Math.Sin(j) + Math.Sin(i), 2);
                    }
                }
                return matrix;
            }

            var n = 50;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //for (int i = 0; i < n * n; i++)
            //{
            var matrix = generateRandomMatrix(n);
            var result = LAPSolver.Solve(matrix);
            //}
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds);
            var sum = 0.0;
            for (int i = 0; i < n; i++)
            {
                sum += matrix[i, result[i]];
            }
            Console.WriteLine(sum);
        }
    }
}
