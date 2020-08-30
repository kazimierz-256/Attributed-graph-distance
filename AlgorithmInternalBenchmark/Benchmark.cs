using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AlgorithmInternalBenchmark
{
    public class Benchmark<T>
    {
        private readonly Dictionary<T, Stopwatch> timer = new Dictionary<T, Stopwatch>();
        public void StartBenchmark(T category)
        {
            var elementExists = timer.TryGetValue(category, out var stopwatch);
            if (!elementExists)
            {
                stopwatch = new Stopwatch();
                timer.Add(category, stopwatch);
            }

            if (timer[category].IsRunning)
                throw new Exception($"Timer {category} is already running");

            stopwatch.Start();
        }

        public TimeSpan GetIntermittentResult(T category) => timer[category].Elapsed;

        public void StopBenchmark(T category)
        {
            if (!timer[category].IsRunning)
                throw new Exception($"Timer {category} is not running");

            timer[category].Stop();
        }

        public string ExportResults(int padRight = 20)
        {
            var builder = new StringBuilder();

            foreach (var kvp in timer.OrderBy(kvp => -kvp.Value.Elapsed))
            {
                builder.Append($"{kvp.Key}: ".PadRight(padRight) + $"{kvp.Value.Elapsed.TotalMilliseconds} ms");
                if (kvp.Value.IsRunning)
                    builder.Append(" (still running)");
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
