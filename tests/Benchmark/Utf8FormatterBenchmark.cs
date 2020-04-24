using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD.Buffered;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class Utf8FormatterBenchmark
    {
        private static readonly StatsDUtf8Formatter FormatterBuffer = new StatsDUtf8Formatter("hello.world");

        private static readonly byte[] Buffer = new byte[512];

        [Benchmark]
        public void BufferBased()
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("key", " value");
            tags.Add("key2", " value2");

            FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", null), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", null), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", null), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", tags), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", tags), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", tags), 1, Buffer, out _);
        }
    }
}
