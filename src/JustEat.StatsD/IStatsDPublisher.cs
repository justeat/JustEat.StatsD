using System.Collections.Generic;

namespace JustEat.StatsD
{
    /// <summary>
    /// Defines an interface for publishing counters, gauges and timers to StatsD.
    /// </summary>
    public interface IStatsDPublisher
    {
        /// <summary>
        /// Publishes a counter for the specified bucket and value.
        /// </summary>
        /// <param name="value">The value to increment the counter by.</param>
        /// <param name="sampleRate">The sample rate for the counter.</param>
        /// <param name="bucket">The bucket to increment the counter for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        void Increment(long value, double sampleRate, string bucket, IDictionary<string, string>? tags);

        /// <summary>
        /// Publishes a gauge for the specified bucket and value.
        /// </summary>
        /// <param name="value">The value to publish for the gauge.</param>
        /// <param name="bucket">The bucket to publish the gauge for.</param>
        /// <param name="operation">The gauge operation.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        void Gauge(double value, string bucket, Operation operation, IDictionary<string, string>? tags);

        /// <summary>
        /// Publishes a timer for the specified bucket and value.
        /// </summary>
        /// <param name="duration">The value to publish for the timer.</param>
        /// <param name="sampleRate">The sample rate for the timer.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        void Timing(long duration, double sampleRate, string bucket, IDictionary<string, string>? tags);
    }
}
