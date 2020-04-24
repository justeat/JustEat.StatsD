using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class TimerExtensionsTests
    {
        [Fact]
        public static void StartTimerThrowsIfPublisherIsNull()
        {
            // Arrange
            IStatsDPublisher? publisher = null;
            string bucket = "bucket";

            // Act and Assert
            Assert.Throws<ArgumentNullException>("publisher", () => publisher!.StartTimer(bucket));
        }

        [Fact]
        public static void TimeThrowsIfActionIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Action? action = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
        }

        [Fact]
        public static void TimeThrowsIfActionForTimerIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Action<IDisposableTimer>? action = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
        }

        [Fact]
        public static async Task TimeThrowsIfFuncForTaskIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Func<Task>? action = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
        }

        [Fact]
        public static async Task TimeThrowsIfFuncForTaskWithTimerIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Func<IDisposableTimer, Task>? action = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
        }

        [Fact]
        public static void TimeThrowsIfFuncOfTIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Func<int>? func = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
        }

        [Fact]
        public static void TimeThrowsIfFuncOfTWithTimerIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Func<IDisposableTimer, int>? func = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
        }

        [Fact]
        public static async Task TimeThrowsIfFuncForTaskOfTIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Func<Task<int>>? func = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
        }

        [Fact]
        public static async Task TimeThrowsIfFuncForTaskOfTWithTimerIsNull()
        {
            // Arrange
            var publisher = Mock.Of<IStatsDPublisher>();
            string bucket = "bucket";

            Func<IDisposableTimer, Task<int>>? func = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
        }

        [Fact]
        public static void CanTimeAction()
        {
            // Arrange
            var publisher = new Mock<IStatsDPublisher>();
            string bucket = "bucket";

            bool timed = false;

            // Act
            publisher.Object.Time(bucket, () => { timed = true; });

            // Assert
            timed.ShouldBeTrue();
            publisher.Verify(
                (p) => p.Timing(It.IsAny<long>(), 1, bucket,
                    It.IsAny<IDictionary<string, string>>()),
                    Times.Once());
        }

        [Fact]
        public static void CanTimeActionWithTimerOfT()
        {
            // Arrange
            var publisher = new Mock<IStatsDPublisher>();
            string bucket = "bucket";

            // Act
            int actual = publisher.Object.Time(
                bucket,
                (timer) =>
                {
                    timer.ShouldNotBeNull();
                    timer.Bucket.ShouldBe(bucket);

                    return 42;
                });

            // Assert
            actual.ShouldBe(42);
            publisher.Verify(
                (p) => p.Timing(It.IsAny<long>(), 1, bucket,
                    It.IsAny<IDictionary<string, string>>()),
                    Times.Once());
        }

        [Fact]
        public static async Task CanTimeAsyncActionWithTimerOfT()
        {
            // Arrange
            var publisher = new Mock<IStatsDPublisher>();
            string bucket = "bucket";

            bool timed = false;

            // Act
            await publisher.Object.Time(
                bucket,
                (timer) =>
                {
                    timer.ShouldNotBeNull();
                    timer.Bucket.ShouldBe(bucket);

                    timed = true;

                    return Task.CompletedTask;
                });

            // Assert
            timed.ShouldBeTrue();
            publisher.Verify(
                (p) => p.Timing(It.IsAny<long>(), 1, bucket,
                    It.IsAny<IDictionary<string, string>>()),
                    Times.Once());
        }

        [Fact]
        public static async Task CanTimeAsyncFuncWithTimerOfT()
        {
            // Arrange
            var publisher = new Mock<IStatsDPublisher>();
            string bucket = "bucket";

            // Act
            int actual = await publisher.Object.Time(
                bucket,
                (timer) =>
                {
                    timer.ShouldNotBeNull();
                    timer.Bucket.ShouldBe(bucket);

                    return Task.FromResult(42);
                });

            // Assert
            actual.ShouldBe(42);
            publisher.Verify(
                (p) => p.Timing(It.IsAny<long>(), 1, bucket,
                    It.IsAny<IDictionary<string, string>>()),
                    Times.Once());
        }
    }
}
