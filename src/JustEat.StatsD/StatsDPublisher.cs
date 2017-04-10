﻿using System;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    ///     Will synchronously publish stats at statsd as you make calls; will not batch sends.
    /// </summary>
    public class StatsDPublisher : IStatsDPublisher
    {
        private readonly StatsDMessageFormatter _formatter;
        private readonly IStatsDTransport _transport;

        public StatsDPublisher(StatsDConfiguration configuration, IStatsDTransport transport)
        {
            if (configuration == null)
            {
               throw new ArgumentNullException(nameof(configuration));
            }

            _transport = transport ?? throw new ArgumentNullException(nameof(transport));

            _formatter = new StatsDMessageFormatter(configuration.Culture, configuration.Prefix);
        }

        public StatsDPublisher(StatsDConfiguration configuration)
        {
            if (configuration == null)
            {
               throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.Host))
            {
                throw new ArgumentNullException(nameof(configuration.Host));
            }

            _formatter = new StatsDMessageFormatter(configuration.Culture, configuration.Prefix);

            var endpointSource = EndpointParser.MakeEndPointSource(
                configuration.Host, configuration.Port, configuration.DnsLookupInterval);
            _transport = new UdpTransport(endpointSource);
        }

        public void Increment(string bucket)
        {
            _transport.Send(_formatter.Increment(bucket));
        }

        public void Increment(long value, string bucket)
        {
            _transport.Send(_formatter.Increment(value, bucket));
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            _transport.Send(_formatter.Increment(value, sampleRate, bucket));
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            _transport.Send(_formatter.Increment(value, sampleRate, buckets));
        }

        public void Decrement(string bucket)
        {
            _transport.Send(_formatter.Decrement(bucket));
        }

        public void Decrement(long value, string bucket)
        {
            _transport.Send(_formatter.Decrement(value, bucket));
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            _transport.Send(_formatter.Decrement(value, sampleRate, bucket));
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            _transport.Send(_formatter.Decrement(value, sampleRate, buckets));
        }

        public void Gauge(long value, string bucket)
        {
            _transport.Send(_formatter.Gauge(value, bucket));
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            _transport.Send(_formatter.Gauge(value, bucket, timestamp));
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            _transport.Send(_formatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), bucket));
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            _transport.Send(_formatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), sampleRate, bucket));
        }

        public void MarkEvent(string name)
        {
            _transport.Send(_formatter.Event(name));
        }
    }
}
