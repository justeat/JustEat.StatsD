﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class StatsDUdpTransport : IStatsDTransport
    {
        private static readonly SimpleObjectPool<SocketAsyncEventArgs> EventArgsPool
            = new SimpleObjectPool<SocketAsyncEventArgs>(30, pool => new PoolAwareSocketAsyncEventArgs(pool));

        private readonly IDnsEndpointMapper _endPointMapper;
        private readonly string _hostNameOrAddress;
        private readonly IPEndPoint _ipBasedEndpoint;
        private readonly int _port;

        public StatsDUdpTransport(string hostNameOrAddress, int port)
            : this(new DnsEndpointProvider(), hostNameOrAddress, port) {}

        public StatsDUdpTransport(int endpointCacheDuration, string hostNameOrAddress, int port)
            : this(new CachedDnsEndpointMapper(new DnsEndpointProvider(), endpointCacheDuration), hostNameOrAddress, port) {}

        private StatsDUdpTransport(IDnsEndpointMapper endpointMapper, string hostNameOrAddress, int port)
        {
            _endPointMapper = endpointMapper;
            _hostNameOrAddress = hostNameOrAddress;
            _port = port;

            //if we were given an IP instead of a hostname, we can happily cache it off for the life of this class
            IPAddress address;
            if (IPAddress.TryParse(hostNameOrAddress, out address))
            {
                _ipBasedEndpoint = new IPEndPoint(address, _port);
            }
        }

        public bool Send(string metric)
        {
            return Send(new[] {metric});
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is one of the rare cases where eating exceptions is OK")]
        public bool Send(IEnumerable<string> metrics)
        {
            var data = EventArgsPool.Pop();
            //firehose alert! -- keep it moving!
            if (data == null)
            {
                return false;
            }

            try
            {
                data.RemoteEndPoint = GetIpEndPoint();
                data.SendPacketsElements = metrics.ToMaximumBytePackets()
                    .Select(bytes => new SendPacketsElement(bytes, 0, bytes.Length, true))
                    .ToArray();

                using (var udpClient = GetUdpClient())
                {
                    udpClient.Client.Connect(data.RemoteEndPoint);
                    udpClient.Client.SendPacketsAsync(data);
                }

                Trace.TraceInformation("statsd: {0}", string.Join(",", metrics));

                return true;
            }
            //fire and forget, so just eat intermittent failures / exceptions
            catch (Exception e)
            {
                Trace.TraceError("General Exception when sending metric data to statsD :- Message : {0}, Inner Exception {1}, StackTrace {2}.", e.Message, e.InnerException, e.StackTrace);
            }

            return false;
        }

        public UdpClient GetUdpClient()
        {
            UdpClient client = null;
            try
            {
                client = new UdpClient()
                {
                    Client = { SendBufferSize = 0 }
                };
            }
            catch (SocketException e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Error Creating udpClient :-  Message : {0}, Inner Exception {1}, StackTrace {2}.", e.Message, e.InnerException, e.StackTrace));
            }
            return client;
        }

        private IPEndPoint GetIpEndPoint()
        {
            return _ipBasedEndpoint ?? _endPointMapper.GetIPEndPoint(_hostNameOrAddress, _port); // Only DNS resolve if we were given a hostname
        }
    }
}
