﻿using System;
using System.Globalization;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class WhenRecordingGauges
    {
        [Fact]
        public static void GaugeMetricsAreFormattedCorrectly()
        {
            string statBucket = "gauge-bucket";
            long magnitude = new Random().Next(100);

            var culture = new CultureInfo("en-US");
            var target = new StatsDMessageFormatter(culture);

            string actual = target.Gauge(magnitude, statBucket);

            actual.ShouldBe(string.Format(culture, "{0}:{1}|g", statBucket, magnitude));
        }

        [Fact]
        public static void GaugeMetricsAreFormattedCorrectlyWhenTheyContainATimestamp()
        {
            string statBucket = "gauge-bucket";
            long magnitude = new Random().Next(100);

            var culture = new CultureInfo("en-US");
            var target = new StatsDMessageFormatter(culture);

            var timestamp = DateTime.Now;

            string actual = target.Gauge(magnitude, statBucket, timestamp);

            actual.ShouldBe(string.Format(culture, "{0}:{1}|g|@{2}", statBucket, magnitude, timestamp.AsUnixTime()));

        }

        [Fact]
        public static void GaugeMetricsAreFormattedCorrectlyIfTheCultureIsIncompatible()
        {
            // Setup a value that will create a value where '.' and ',' have opposite meanings to US/UK English.
            var culture = new CultureInfo("da-DK");
            long magnitude = 100000000;

            string statBucket = "gauge-bucket";
            string incompatibleValue = magnitude.ToString("0.00", culture);
            
            var target = new StatsDMessageFormatter(culture);

            string actual = target.Gauge(magnitude, statBucket);

            actual.ShouldNotContain(incompatibleValue);
            actual.ShouldBe(string.Format(culture, "{0}:{1}|g", statBucket, magnitude));
        }
    }
}
