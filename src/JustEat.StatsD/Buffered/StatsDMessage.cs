using System.Collections.Generic;

namespace JustEat.StatsD.Buffered
{
    internal readonly struct StatsDMessage
    {
        public readonly string StatBucket;
        public readonly double Magnitude;
        public readonly StatsDMessageKind MessageKind;
        public readonly Operation Operation;
        public readonly IDictionary<string, string>? Tags;

        private StatsDMessage(
            string statBucket,
            double magnitude,
            StatsDMessageKind messageKind,
            IDictionary<string, string>? tags,
            Operation operation = default)
        {
            StatBucket = statBucket;
            Magnitude = magnitude;
            MessageKind = messageKind;
            Operation = operation;
            Tags = tags;
        }

        public static StatsDMessage Timing(
            long milliseconds,
            string statBucket,
            IDictionary<string, string>? tags)
        {
            return new StatsDMessage(statBucket, milliseconds, StatsDMessageKind.Timing, tags);
        }

        public static StatsDMessage Counter(
            long magnitude,
            string statBucket,
            IDictionary<string, string>? tags)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Counter, tags);
        }

        public static StatsDMessage Gauge(
            double magnitude,
            string statBucket,
            IDictionary<string, string>? tags,
            Operation operation = Operation.Set)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Gauge, tags, operation);
        }
    }
}
