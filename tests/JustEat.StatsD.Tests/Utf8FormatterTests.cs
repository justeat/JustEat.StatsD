using System;
using System.Collections.Generic;
using System.Text;
using JustEat.StatsD.Buffered;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class Utf8FormatterTests
    {
        private static readonly byte[] Buffer = new byte[512];
        private static readonly StatsDUtf8Formatter Formatter = new StatsDUtf8Formatter("prefix");

        [Fact]
        public static void CounterSampled()
        {
            var message = StatsDMessage.Counter(128, "bucket", null);
            Check(message, 0.5, "prefix.bucket:128|c|@0.5");
        }

        [Fact]
        public static void CounterRegular()
        {
            var message = StatsDMessage.Counter(128, "bucket", null);
            Check(message, "prefix.bucket:128|c");
        }

        [Fact]
        public static void CounterRegularWithTags()
        {
            var tags = new Dictionary<string, string>
            {
                ["foo"] = "bar",
                ["another"] = "tag"
            };
            var message = StatsDMessage.Counter(128, "bucket", tags);
            Check(message, "prefix.bucket;foo=bar;another=tag:128|c");
        }

        [Fact]
        public static void CounterNegative()
        {
            var message = StatsDMessage.Counter(-128, "bucket", null);
            Check(message, "prefix.bucket:-128|c");
        }

        [Fact]
        public static void Timing()
        {
            var message = StatsDMessage.Timing(128, "bucket", null);
            Check(message, "prefix.bucket:128|ms");
        }

        [Fact]
        public static void TimingWithTags()
        {
            var tags = new Dictionary<string, string>
            {
                ["foo"] = "bar",
                ["another"] = "tag"
            };

            var message = StatsDMessage.Timing(128, "bucket", tags);
            Check(message, "prefix.bucket;foo=bar;another=tag:128|ms");
        }

        [Fact]
        public static void TimingSampled()
        {
            var message = StatsDMessage.Timing(128, "bucket", null);
            Check(message, 0.5, "prefix.bucket:128|ms|@0.5");
        }

        [Fact]
        public static void GaugeIncrement()
        {
            var message = StatsDMessage.Gauge(128, "bucket", null, Operation.Increment);
            Check(message, "prefix.bucket:+128|g");
        }

        [Fact]
        public static void GaugeIncrementWithTags()
        {
            var tags = new Dictionary<string, string>
            {
                ["foo"] = "bar",
                ["another"] = "tag"
            };

            var message = StatsDMessage.Gauge(128, "bucket", tags, Operation.Increment);
            Check(message, "prefix.bucket;foo=bar;another=tag:+128|g");
        }

        [Fact]
        public static void GaugeDecrement()
        {
            var message = StatsDMessage.Gauge(128, "bucket", null, Operation.Decrement);
            Check(message, "prefix.bucket:-128|g");
        }

        [Fact]
        public static void GaugeDecrementWithTags()
        {
            var tags = new Dictionary<string, string>
            {
                ["foo"] = "bar",
                ["another"] = "tag"
            };

            var message = StatsDMessage.Gauge(128, "bucket", tags, Operation.Decrement);
            Check(message, "prefix.bucket;foo=bar;another=tag:-128|g");
        }

        [Fact]
        public static void GaugeIntegral()
        {
            var message = StatsDMessage.Gauge(128, "bucket", null);
            Check(message, "prefix.bucket:128|g");
        }

        [Fact]
        public static void GaugeFloat()
        {
            var message = StatsDMessage.Gauge(128.5, "bucket", null);
            Check(message, "prefix.bucket:128.5|g");
        }

        [Fact]
        public static void MessagesLargerThenAvailableBufferShouldNotBeFormatted()
        {
            var buffer = new byte[128];
            var hugeBucket = new string('x', 256);
            var message = StatsDMessage.Gauge(128.5, hugeBucket, null);
            Formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(false);
            written.ShouldBe(0);
        }

        [Theory]
        [InlineData(1, 'z')]
        [InlineData(2, 'z')]
        [InlineData(4, 'z')]
        [InlineData(8, 'z')]
        [InlineData(16, 'z')]
        [InlineData(32, 'z')]
        [InlineData(64, 'z')]
        [InlineData(128, 'z')]
        [InlineData(256, 'z')]
        [InlineData(512, 'z')]
        [InlineData(1024, 'z')]
        [InlineData(2048, 'z')]
        [InlineData(1, 'Ж')]
        [InlineData(2, 'Ж')]
        [InlineData(4, 'Ж')]
        [InlineData(8, 'Ж')]
        [InlineData(16, 'Ж')]
        [InlineData(32, 'Ж')]
        [InlineData(64, 'Ж')]
        [InlineData(128, 'Ж')]
        [InlineData(256, 'Ж')]
        [InlineData(512, 'Ж')]
        [InlineData(1024, 'Ж')]
        [InlineData(2048, 'Ж')]
        public static void GetMaxBufferSizeCalculatesValidBufferSizes(int bucketSize, char ch)
        {
            var hugeBucket = new string(ch, bucketSize);
            var message = StatsDMessage.Gauge(128.5, hugeBucket, null);
            var expected = $"prefix.{hugeBucket}:128.5|g";

            var buffer = new byte[Formatter.GetMaxBufferSize(message)];

            Formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(true);
            var actual = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
            actual.ShouldBe(expected);
        }

        private static void Check(StatsDMessage message, string expected)
        {
            Check(message, 1, expected);
        }

        private static void Check(StatsDMessage message, double sampleRate, string expected)
        {
            Formatter.TryFormat(message, sampleRate, Buffer, out int written).ShouldBe(true);
            var result = Encoding.UTF8.GetString(Buffer.AsSpan(0, written));
            result.ShouldBe(expected);
        }
    }
}
