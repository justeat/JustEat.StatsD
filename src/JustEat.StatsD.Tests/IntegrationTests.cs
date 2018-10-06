using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class IntegrationTests
    {
        [SkippableFact]
        public static async Task Can_Send_Metrics_To_StatsD()
        {
            Skip.If(Environment.GetEnvironmentVariable("CI") == null, "By default, this test is only run during continuous integration.");

            // Arrange
            var config = new StatsDConfiguration
            {
                Host = "localhost",
                Prefix = Guid.NewGuid().ToString().Replace("-", string.Empty)
            };

            var publisher = new StatsDPublisher(config);

            // Act - Create a counter
            publisher.Increment("apple");

            // Act - Create and change a counter
            publisher.Increment("bear");        // 1
            publisher.Increment(10, "bear");    // 11
            publisher.Increment(10, 0, "bear"); // 11
            publisher.Decrement("bear");        // 10
            publisher.Decrement(5, "bear");     // 5
            publisher.Decrement(5, 0, "bear");  // 5

            // Act - Mark an event (which is a counter)
            publisher.MarkEvent("fish");

            // Act - Create a gauge
            publisher.Gauge(3.141, "circle");

            // Act - Create and change a gauge
            publisher.Gauge(10, "dog");
            publisher.Gauge(42, "dog");

            // Act - Create a timer
            publisher.Timing(123, "elephant");
            publisher.Timing(TimeSpan.FromSeconds(2), "fox");
            publisher.Timing(456, 1, "goose");
            publisher.Timing(TimeSpan.FromSeconds(3.5), 1, "hen");

            // Act - Increment multiple counters
            publisher.Increment(7, 1, "green", "red"); // 7
            publisher.Increment(2, 0, "green", "red"); // 7
            publisher.Decrement(1, 0, "red", "green"); // 7
            publisher.Decrement(4, 1, "red", "green"); // 3

            // Assert
            var result = await SendCommandAsync("counters");
            result.Value<int>(config.Prefix + ".apple").ShouldBe(1);
            result.Value<int>(config.Prefix + ".bear").ShouldBe(5);
            result.Value<int>(config.Prefix + ".fish").ShouldBe(1);
            result.Value<int>(config.Prefix + ".green").ShouldBe(3);
            result.Value<int>(config.Prefix + ".red").ShouldBe(3);

            result = await SendCommandAsync("gauges");
            result.Value<double>(config.Prefix + ".circle").ShouldBe(3.141);
            result.Value<int>(config.Prefix + ".dog").ShouldBe(42);

            result = await SendCommandAsync("timers");
            result[config.Prefix + ".elephant"].Values<int>().ShouldBe(new[] { 123 });
            result[config.Prefix + ".fox"].Values<int>().ShouldBe(new[] { 2000 });
            result[config.Prefix + ".goose"].Values<int>().ShouldBe(new[] { 456 });
            result[config.Prefix + ".hen"].Values<int>().ShouldBe(new[] { 3500 });
        }

        private static async Task<JObject> SendCommandAsync(string command)
        {
            string json;

            using (var client = new TcpClient())
            {
                client.Connect("localhost", 8126);

                byte[] input = Encoding.UTF8.GetBytes(command);
                byte[] output = new byte[client.ReceiveBufferSize];

                int bytesRead;

                using (var stream = client.GetStream())
                {
                    await stream.WriteAsync(input);
                    bytesRead = await stream.ReadAsync(output);
                }

                output = output.AsSpan(0, bytesRead).ToArray();

                json = Encoding.UTF8.GetString(output).Replace("END", string.Empty);
            }

            return JObject.Parse(json);
        }
    }
}
