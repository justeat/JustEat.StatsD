using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.Buffered
{
    internal sealed class StatsDUtf8Formatter
    {
        private readonly byte[] _utf8Prefix;

        public StatsDUtf8Formatter(string prefix)
        {
            _utf8Prefix = string.IsNullOrWhiteSpace(prefix) ?
                Array.Empty<byte>() :
                Encoding.UTF8.GetBytes(prefix + ".");
        }

        public int GetMaxBufferSize(in StatsDMessage msg)
        {
            const int MaxSerializedDoubleSymbols = 32;
            const int ColonBytes = 1;

            const int MaxMessageKindSuffixSize = 3;
            const int MaxSamplingSuffixSize = 2;

            return _utf8Prefix.Length
                    + Encoding.UTF8.GetByteCount(msg.StatBucket)
                    + ColonBytes
                    + MaxSerializedDoubleSymbols
                    + MaxMessageKindSuffixSize
                    + MaxSamplingSuffixSize
                    + MaxSerializedDoubleSymbols;
        }

        public bool TryFormat(in StatsDMessage msg, double sampleRate, Span<byte> destination, out int written)
        {
            var buffer = new Buffer(destination);

            bool isFormattingSuccessful =
                  TryWriteBucketNameWithColon(ref buffer, msg)
               && TryWritePayloadWithMessageKindSuffix(ref buffer, msg)
               && TryWriteSampleRateIfNeeded(ref buffer, sampleRate);

            written = isFormattingSuccessful ? buffer.Written : 0;
            return isFormattingSuccessful;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteBucketNameWithColon(ref Buffer buffer, StatsDMessage msg)
        {
            // prefix + msg.Bucket + tags + ":"

            return buffer.TryWriteBytes(_utf8Prefix)
                && buffer.TryWriteUtf8String(msg.StatBucket)
                && TryWriteTags(ref buffer, msg.Tags)
                && buffer.TryWriteByte((byte)':');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryWriteTags(ref Buffer buffer, IDictionary<string, string>? tags)
        {
            // key=value,key=value

            if (tags == null || tags.Count == 0)
            {
                return true;
            }

            var sb = new StringBuilder();
            foreach (var tag in tags)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, ";{0}={1}", tag.Key, tag.Value);
            }

            return buffer.TryWriteUtf8String(sb.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryWritePayloadWithMessageKindSuffix(ref Buffer buffer, in StatsDMessage msg)
        {
            // msg.Value + (<oneOf> "|ms", "|c", "|g")

            var integralMagnitude = (long)msg.Magnitude;

            switch (msg.MessageKind)
            {
                case StatsDMessageKind.Counter:
                    {
                        return buffer.TryWriteInt64(integralMagnitude)
                            && buffer.TryWriteBytes((byte)'|', (byte)'c');
                    }
                case StatsDMessageKind.Timing:
                    {
                        return buffer.TryWriteInt64(integralMagnitude)
                            && buffer.TryWriteBytes((byte)'|', (byte)'m', (byte)'s');
                    }
                case StatsDMessageKind.Gauge:
                    {
                        // check if magnitude is integral, integers are written significantly faster
                        bool isMagnitudeIntegral = msg.Magnitude == integralMagnitude;

                        bool successSoFar = true;

                        if (msg.Operation == Operation.Increment)
                        {
                            successSoFar &= buffer.TryWriteByte((byte) '+');
                        }

                        if (msg.Operation == Operation.Decrement)
                        {
                            successSoFar &= buffer.TryWriteByte((byte) '-');
                        }

                        successSoFar &= isMagnitudeIntegral ?
                            buffer.TryWriteInt64(integralMagnitude) :
                            buffer.TryWriteDouble(msg.Magnitude);

                        return successSoFar && buffer.TryWriteBytes((byte)'|', (byte)'g');
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(msg));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryWriteSampleRateIfNeeded(ref Buffer buffer, double sampleRate)
        {
            // {<optional> "|@" + sampleRate}

            if (sampleRate < 1.0 && sampleRate > 0.0)
            {
                return buffer.TryWriteBytes((byte)'|', (byte)'@')
                    && buffer.TryWriteDouble(sampleRate);
            }

            return true;
        }
    }
}
