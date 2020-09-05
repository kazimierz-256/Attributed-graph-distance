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
        private readonly Dictionary<T, int> startEventCounter = new Dictionary<T, int>();
        private readonly Dictionary<T, T> relativeTo = new Dictionary<T, T>();

        public void StartBenchmark(T category)
        {
            var elementExists = timer.TryGetValue(category, out var stopwatch);
            if (!elementExists)
            {
                stopwatch = new Stopwatch();
                timer.Add(category, stopwatch);
                startEventCounter.Add(category, 0);
            }

            if (timer[category].IsRunning)
                throw new Exception($"Timer {category} is already running");

            startEventCounter[category]++;
            stopwatch.Start();
        }

        public TimeSpan GetIntermittentResult(T category)
        {
            var categoryExists = timer.TryGetValue(category, out var elapsed);
            return categoryExists ? elapsed.Elapsed : TimeSpan.Zero;
        }

        public int GetIntermittentCount(T category)
        {
            var categoryExists = startEventCounter.TryGetValue(category, out var count);
            return categoryExists ? count : 0;
        }

        public void DisplayRelativeTo(T dependentCategory, T referenceCategory)
        {
            if (relativeTo.TryGetValue(dependentCategory, out var oldCategory))
            {
                if (!oldCategory.Equals(referenceCategory))
                    relativeTo[dependentCategory] = referenceCategory;
            }
            else
                relativeTo.Add(dependentCategory, referenceCategory);
        }
        public void StopBenchmark(T category)
        {
            if (!timer[category].IsRunning)
                throw new Exception($"Timer {category} is not running");

            timer[category].Stop();
        }

        public string ExportResults(int padRight = 25)
        {
            var builder = new StringBuilder();

            var orderedTimers = timer.OrderBy(kvp => -kvp.Value.Elapsed).ToList();

            foreach (var kvp in orderedTimers)
            {
                builder.Append($"{kvp.Key}: ".PadRight(padRight));

                var relativeCategoryName = orderedTimers[0].Key;
                var relativeCategoryStopwatch = orderedTimers[0].Value;
                if (relativeTo.TryGetValue(kvp.Key, out var newRelativeCategory))
                {
                    relativeCategoryName = newRelativeCategory;
                    relativeCategoryStopwatch = timer[newRelativeCategory];
                }

                builder.Append($" {kvp.Value.Elapsed.TotalMilliseconds * 100d / relativeCategoryStopwatch.Elapsed.TotalMilliseconds:00.00} % (relative to {relativeCategoryName})");
                //builder.Append($" {kvp.Value.Elapsed.TotalMilliseconds} ms");
                if (kvp.Value.IsRunning)
                    builder.Append(" (still running)");
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
