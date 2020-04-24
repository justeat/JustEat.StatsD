using System;
using System.Collections.Generic;
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
        [SkippableTheory]
        [InlineData("localhost", SocketProtocol.IP)]
        [InlineData("localhost", SocketProtocol.Udp)]
        [InlineData("127.0.0.1", SocketProtocol.IP)]
        [InlineData("127.0.0.1", SocketProtocol.Udp)]
        public static async Task Can_Send_Metrics_To_StatsD_Using_Udp(
            string host,
            SocketProtocol socketProtocol)
        {
            Skip.If(Environment.GetEnvironmentVariable("CI") == null, "By default, this test is only run during continuous integration.");

            // Arrange
            var config = new StatsDConfiguration
            {
                Host = host,
                Prefix = Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.Ordinal),
                SocketProtocol = socketProtocol,
            };

            using var publisher = new StatsDPublisher(config);

            // Act and Assert
            await AssertMetrics(config, publisher);
        }

        private static async Task AssertMetrics(StatsDConfiguration config, IStatsDPublisher publisher)
        {
            // Act - Create a counter
            publisher.Increment("apple");

            // Act - Create and change a counter
            publisher.Increment("bear");              // 1
            publisher.Increment(10, "bear");          // 11
            publisher.Increment(10, 0, "bear", null); // 11
            publisher.Decrement("bear");              // 10
            publisher.Decrement(5, "bear");           // 5
            publisher.Decrement(5, 0, "bear");        // 5

            // Act - Create and change a counter with tags
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("key", "value");
            tags.Add("key2", "value2");
            publisher.Increment("ant", tags);              // 1
            publisher.Increment(15, "ant", tags);          // 16
            publisher.Decrement(5,"ant", tags);            // 11

            // Act - Create multiple counters with tags and change them
            tags = new Dictionary<string, string>();
            tags.Add("key", "value");
            tags.Add("key2", "value2");
            publisher.Increment(100, 1, tags, new string[] { "gmc", "ford" });        // 100
            publisher.Decrement(5, 1, tags, new string[] { "gmc", "ford" });          // 95

            // Act - Create multiple counters without tags and change them
            publisher.Increment(150, 1, null, new string[] { "jaguar", "kia" });      // 100
            publisher.Decrement(20, 1, null, new string[] { "jaguar", "kia" });       // 130

            // Act - Create a counter with tags
            tags = new Dictionary<string, string>();
            tags.Add("key", "value");
            tags.Add("key2", "value2");
            publisher.Increment("orange", tags);      // 1
            publisher.Increment(50, "orange", tags);  // 51

            // Act - Create multiple counters with tags
            tags = new Dictionary<string, string>();
            tags.Add("type", "vehicle");
            publisher.Increment(60, 1, tags, new string[] { "mazda", "fiat" });

            // Act - Create multiple counters without tags
            publisher.Increment(25, 1, null, new string[] { "ferrari", "jeep" });

            // Act - Mark an event (which is a counter)
            publisher.Increment("fish");

            // Act - Create a gauge
            publisher.Gauge(3.141, "circle", Operation.Set, null);

            // Act - Create and change a gauge
            publisher.Gauge(10, "dog", Operation.Set, null);
            publisher.Gauge(42, "dog", Operation.Set, null);

            // Act - Create a gauge with tags
            tags = new Dictionary<string, string>();
            tags.Add("foo", "bar");
            tags.Add("lorem", "ipsum");
            publisher.Gauge(5.5, "square", Operation.Set, tags);

            // Act - Create a gauge and decrement it
            publisher.Gauge(2020, "year", Operation.Set, null);
            publisher.Gauge(10, "year", Operation.Decrement, null);

            // Act - Create a gauge and increment it
            publisher.Gauge(15, "score", Operation.Set, null);
            publisher.Gauge(2, "score", Operation.Increment, null);

            // Act - Create a timer
            publisher.Timing(123, "elephant");
            publisher.Timing(TimeSpan.FromSeconds(2), "fox");
            publisher.Timing(456, 1, "goose", null);
            publisher.Timing(TimeSpan.FromSeconds(3.5), 1, "hen");

            // Act - Create a timer with tags
            tags = new Dictionary<string, string>();
            tags.Add("class", "mammal");
            tags.Add("genus", "panthera");
            publisher.Timing(128, "leopard", tags);

            // Act - Increment multiple counters
            publisher.Increment(7, 1, new string[] { "green", "red" });       // 7
            publisher.Increment(2, 0, new List<string>() { "green", "red" }); // 7
            publisher.Decrement(1, 0, new string[] { "red", "green" });       // 7
            publisher.Decrement(4, 1, new List<string>() { "red", "green" }); // 3

            // Allow enough time for metrics to be registered
            await Task.Delay(TimeSpan.FromSeconds(1.0));

            // Assert
            var result = await SendCommandAsync("counters");
            result.Value<int>(config.Prefix + ".apple").ShouldBe(1, result.ToString());
            result.Value<int>(config.Prefix + ".bear").ShouldBe(5, result.ToString());
            result.Value<int>(config.Prefix + ".ant;key=value;key2=value2").ShouldBe(11, result.ToString());
            result.Value<int>(config.Prefix + ".gmc;key=value;key2=value2").ShouldBe(95, result.ToString());
            result.Value<int>(config.Prefix + ".ford;key=value;key2=value2").ShouldBe(95, result.ToString());
            result.Value<int>(config.Prefix + ".jaguar").ShouldBe(130, result.ToString());
            result.Value<int>(config.Prefix + ".kia").ShouldBe(130, result.ToString());
            result.Value<int>(config.Prefix + ".orange;key=value;key2=value2").ShouldBe(51, result.ToString());
            result.Value<int>(config.Prefix + ".mazda;type=vehicle").ShouldBe(60, result.ToString());
            result.Value<int>(config.Prefix + ".fiat;type=vehicle").ShouldBe(60, result.ToString());
            result.Value<int>(config.Prefix + ".ferrari").ShouldBe(25, result.ToString());
            result.Value<int>(config.Prefix + ".jeep").ShouldBe(25, result.ToString());
            result.Value<int>(config.Prefix + ".fish").ShouldBe(1, result.ToString());
            result.Value<int>(config.Prefix + ".green").ShouldBe(3, result.ToString());
            result.Value<int>(config.Prefix + ".red").ShouldBe(3, result.ToString());

            result = await SendCommandAsync("gauges");
            result.Value<double>(config.Prefix + ".circle").ShouldBe(3.141, result.ToString());
            result.Value<int>(config.Prefix + ".dog").ShouldBe(42, result.ToString());
            result.Value<double>(config.Prefix + ".square;foo=bar;lorem=ipsum").ShouldBe(5.5, result.ToString());
            result.Value<int>(config.Prefix + ".year").ShouldBe(2010, result.ToString());
            result.Value<int>(config.Prefix + ".score").ShouldBe(17, result.ToString());

            result = await SendCommandAsync("timers");
            result[config.Prefix + ".elephant"].Values<int>().ShouldBe(new[] { 123 }, result.ToString());
            result[config.Prefix + ".leopard;class=mammal;genus=panthera"].Values<int>().ShouldBe(new[] { 128 }, result.ToString());
            result[config.Prefix + ".fox"].Values<int>().ShouldBe(new[] { 2000 }, result.ToString());
            result[config.Prefix + ".goose"].Values<int>().ShouldBe(new[] { 456 }, result.ToString());
            result[config.Prefix + ".hen"].Values<int>().ShouldBe(new[] { 3500 }, result.ToString());
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

                var stream = client.GetStream();

                await stream.WriteAsync(input);
                bytesRead = await stream.ReadAsync(output);

                output = output.AsSpan(0, bytesRead).ToArray();

                json = Encoding.UTF8.GetString(output).Replace("END", string.Empty, StringComparison.Ordinal);
            }

            return JObject.Parse(json);
        }
    }
}
