﻿using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// cache the IPEndPoint and only go to the source when it expires
    /// </summary>
    public class CachedIpEndpointSource : IPEndPointSource
    {
        private IPEndPoint _cachedValue;
        private DateTime _expiry;
        private readonly TimeSpan _cacheDuration;

        private readonly IPEndPointSource _inner;

        public CachedIpEndpointSource(IPEndPointSource inner, TimeSpan cacheDuration)
        {
            _inner = inner;
            _cachedValue = null;
            _cacheDuration = cacheDuration;
        }

        public IPEndPoint GetEndpoint()
        {
            if (NeedsRead())
            {
                _cachedValue = _inner.GetEndpoint();
                _expiry = DateTime.UtcNow.Add(_cacheDuration);
            }
            return _cachedValue;
        }

        private bool NeedsRead()
        {
            if (_cachedValue == null)
            {
                return true;
            }

            return _expiry <= DateTime.UtcNow;
        }
    }
}