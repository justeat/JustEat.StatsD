using System;
using System.Collections.Generic;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing the default implementation of <see cref="IStatsDPublisher"/> that
    /// publishes counters, gauges and timers to StatsD. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Metrics are published synchronously immediately and are not batched.
    /// </remarks>
    public sealed class StatsDPublisher : IStatsDPublisher, IDisposable
    {
        private readonly IStatsDPublisher _inner;
        private readonly IStatsDTransport _transport;
        private readonly bool _disposeTransport;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDPublisher"/> class for the specified transport.
        /// </summary>
        /// <param name="configuration">The StatsD configuration to use.</param>
        /// <param name="transport">The transport implementation to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="configuration"/> or <paramref name="transport"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="configuration"/> is invalid, such as an invalid hostname or IP address.
        /// </exception>
        public StatsDPublisher(StatsDConfiguration configuration, IStatsDTransport transport)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            _inner = new BufferBasedStatsDPublisher(configuration, transport);

            _transport = transport;
            _disposeTransport = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDPublisher"/> class using the default transport.
        /// </summary>
        /// <param name="configuration">The StatsD configuration to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="configuration"/> is invalid, such as an invalid hostname or IP address.
        /// </exception>
        public StatsDPublisher(StatsDConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.Host))
            {
                throw new ArgumentException("No hostname or IP address is set.", nameof(configuration));
            }

            var endpointSource = EndPointFactory.MakeEndPointSource(
                configuration.Host!, configuration.Port, configuration.DnsLookupInterval);

            var transport = new SocketTransport(endpointSource, configuration.SocketProtocol);

            _transport = transport;
            _disposeTransport = true;

            _inner = new BufferBasedStatsDPublisher(configuration, transport);
        }

        /// <inheritdoc />
        public void Increment(long value, double sampleRate, string bucket, IDictionary<string, string>? tags = null)
        {
            _inner.Increment(value, sampleRate, bucket, tags);
        }

        /// <inheritdoc />
        public void Gauge(double value, string bucket, Operation operation = Operation.Set, IDictionary<string, string>? tags = null)
        {
            _inner.Gauge(value, bucket, operation, tags);
        }

        /// <inheritdoc />
        public void Timing(long duration, double sampleRate, string bucket, IDictionary<string, string>? tags = null)
        {
            _inner.Timing(duration, sampleRate, bucket, tags);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_disposeTransport && _transport is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
