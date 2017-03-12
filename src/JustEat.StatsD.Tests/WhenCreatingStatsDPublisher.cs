﻿using System;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public class WhenCreatingStatsDPublisher
    {
        [Fact]
        public void ConfigurationIsValidWithHostName()
        {
            var validConfig = new StatsDConfiguration
            {
                Host = "someserver.somewhere.com"
            };

            var stats = new StatsDPublisher(validConfig);

            stats.ShouldNotBeNull();
        }

        [Fact]
        public void ConfigurationIsValidWithHostIp()
        {
            var validConfig = new StatsDConfiguration
            {
                Host = "10.0.1.2"
            };

            var stats = new StatsDPublisher(validConfig);

            stats.ShouldNotBeNull();
        }

        [Fact]
        public void ConfigurationIsNull()
        {
            StatsDConfiguration noConfig = null;

            Should.Throw<ArgumentNullException>(
             () => new StatsDPublisher(noConfig));
        }

        [Fact]
        public void ConfigurationHasNoCulture()
        {
            var badConfig = new StatsDConfiguration
            {
                Host = "someserver.somewhere.com",
                Culture = null
            };

            Should.Throw<ArgumentNullException>(
             () => new StatsDPublisher(badConfig));
        }

        [Fact]
        public void ConfigurationHasNoHost()
        {
            var badConfig = new StatsDConfiguration
            {
                Host = null
            };

            Should.Throw<ArgumentNullException>(
             () => new StatsDPublisher(badConfig));
        }
    }
}
