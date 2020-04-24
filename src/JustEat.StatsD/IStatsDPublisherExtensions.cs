using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class containing extension methods for the <see cref="IStatsDPublisher"/> interface. This class cannot be inherited.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IStatsDPublisherExtensions
    {
        private const double DefaultSampleRate = 1.0;

        /// <summary>
        /// Publishes a counter for the specified bucket with a value of one (1).
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to increment the counter for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Increment(this IStatsDPublisher publisher, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Increment(1, DefaultSampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes a counter for the specified bucket and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to increment the counter by.</param>
        /// <param name="bucket">The bucket to increment the counter for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Increment(this IStatsDPublisher publisher, long value, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Increment(value, DefaultSampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes counter(s) for the specified bucket(s) and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to increment the counter(s) by.</param>
        /// <param name="sampleRate">The sample rate for the counter(s).</param>
        /// <param name="buckets">The bucket(s) to increment the counter(s) for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Increment(this IStatsDPublisher publisher, long value, double sampleRate, IEnumerable<string> buckets, IDictionary<string, string>? tags = null)
        {
            if (buckets == null)
            {
                return;
            }

            foreach (string bucket in buckets)
            {
                publisher.Increment(value, sampleRate, bucket, tags);
            }
        }

        /// <summary>
        /// Publishes counter(s) for the specified bucket(s) and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to increment the counter(s) by.</param>
        /// <param name="sampleRate">The sample rate for the counter(s).</param>
        /// <param name="buckets">The bucket(s) to increment the counter(s) for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Increment(this IStatsDPublisher publisher, long value, double sampleRate, IDictionary<string, string>? tags = null, params string[] buckets)
        {
            if (buckets == null || buckets.Length == 0)
            {
                return;
            }

            foreach (string bucket in buckets)
            {
                publisher.Increment(value, sampleRate, bucket, tags);
            }
        }

        /// <summary>
        /// Publishes a counter for the specified bucket with a value of minus one (-1).
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to decrement the counter for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Decrement(this IStatsDPublisher publisher, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Increment(-1, DefaultSampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes a counter decrement for the specified bucket and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to decrement the counter by.</param>
        /// <param name="bucket">The bucket to decrement the counter for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Decrement(this IStatsDPublisher publisher, long value, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Increment(value > 0 ? -value : value, DefaultSampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes a counter decrement for the specified bucket and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to decrement the counter by.</param>
        /// <param name="sampleRate">The sample rate for the counter.</param>
        /// <param name="bucket">The bucket to decrement the counter for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Decrement(this IStatsDPublisher publisher, long value, double sampleRate, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Increment(value > 0 ? -value : value, sampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes counter decrement(s) for the specified bucket(s) and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to decrement the counter(s) by.</param>
        /// <param name="sampleRate">The sample rate for the counter(s).</param>
        /// <param name="buckets">The bucket(s) to decrement the counter(s) for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Decrement(this IStatsDPublisher publisher, long value, double sampleRate, IEnumerable<string> buckets, IDictionary<string, string>? tags = null)
        {
            if (buckets == null)
            {
                return;
            }

            long adjusted = value > 0 ? -value : value;

            foreach (string bucket in buckets)
            {
                publisher.Increment(adjusted, sampleRate, bucket, tags);
            }
        }

        /// <summary>
        /// Publishes counter decrement(s) for the specified bucket(s) and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="value">The value to decrement the counter(s) by.</param>
        /// <param name="sampleRate">The sample rate for the counter(s).</param>
        /// <param name="buckets">The bucket(s) to decrement the counter(s) for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Decrement(this IStatsDPublisher publisher, long value, double sampleRate, IDictionary<string, string>? tags = null, params string[] buckets)
        {
            if (buckets == null || buckets.Length == 0)
            {
                return;
            }

            long adjusted = value > 0 ? -value : value;

            foreach (string bucket in buckets)
            {
                publisher.Increment(adjusted, sampleRate, bucket, tags);
            }
        }

        /// <summary>
        /// Publishes a timer for the specified bucket and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="duration">The value to publish for the timer.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Timing(this IStatsDPublisher publisher, TimeSpan duration, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Timing((long)duration.TotalMilliseconds, DefaultSampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes a timer for the specified bucket and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="duration">The value to publish for the timer.</param>
        /// <param name="sampleRate">The sample rate for the timer.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Timing(this IStatsDPublisher publisher, TimeSpan duration, double sampleRate, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Timing((long)duration.TotalMilliseconds, sampleRate, bucket, tags);
        }

        /// <summary>
        /// Publishes a timer for the specified bucket and value.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="duration">The value to publish for the timer.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="tags">An optional dictionary of tags.</param>
        public static void Timing(this IStatsDPublisher publisher, long duration, string bucket, IDictionary<string, string>? tags = null)
        {
            publisher.Timing(duration, DefaultSampleRate, bucket, tags);
        }
    }
}
