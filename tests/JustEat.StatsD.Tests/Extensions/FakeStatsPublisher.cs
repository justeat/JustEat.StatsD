using System;
using System.Collections.Generic;

namespace JustEat.StatsD.Extensions
{
    public class FakeStatsPublisher : IStatsDPublisher
    {
        public FakeStatsPublisher()
        {
            BucketNames = new List<string>();
        }

        public int CallCount { get; set; }

        public int DisposeCount { get; set; }

        public TimeSpan LastDuration { get; set; }

        public List<string> BucketNames { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }

        public void Increment(long value, double sampleRate, string bucket, IDictionary<string, string>? tags)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Gauge(double value, string bucket, Operation operation, IDictionary<string, string>? tags)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Timing(long duration, double sampleRate, string bucket, IDictionary<string, string>? tags)
        {
            CallCount++;
            LastDuration = TimeSpan.FromMilliseconds(duration);
            BucketNames.Add(bucket);
        }
    }
}
