# JustEat.StatsD

[![NuGet version](https://buildstats.info/nuget/JustEat.StatsD?includePreReleases=false)](http://www.nuget.org/packages/JustEat.StatsD)

| | Linux | Windows |
|:-:|:-:|:-:|
| **Build Status** | [![Build status](https://img.shields.io/travis/justeat/JustEat.StatsD/master.svg)](https://travis-ci.org/justeat/JustEat.StatsD) | [![Build status](https://img.shields.io/appveyor/ci/justeattech/justeat-statsd/master.svg)](https://ci.appveyor.com/project/justeattech/justeat-statsd) [![codecov](https://codecov.io/gh/justeat/JustEat.StatsD/branch/master/graph/badge.svg)](https://codecov.io/gh/justeat/JustEat.StatsD) |
| **Build History** | [![Build history](https://buildstats.info/travisci/chart/justeat/JustEat.StatsD?branch=master&includeBuildsFromPullRequest=false)](https://travis-ci.org/justeat/JustEat.StatsD) |  [![Build history](https://buildstats.info/appveyor/chart/justeattech/justeat-statsd?branch=master&includeBuildsFromPullRequest=false)](https://ci.appveyor.com/project/justeattech/justeat-statsd) |

## Summary

We use this library within our components to publish [StatsD](http://github.com/etsy/statsd) metrics from .NET code to a server. We've been using this in most of our things, since around 2013.

### Supported platforms

`JustEat.StatsD` version 4.1.0 is built for these target frameworks:

* `net461`
* `netstandard2.0`
* `netstandard2.1`
* `netcoreapp2.1`
* `netcoreapp2.2`

### Features

* Easy to use.
* Robust and proven.
* Tuned for high performance and low resource usage using [BenchmarkDotNet](https://benchmarkdotnet.org/). Typically zero allocation on sending a metric on target frameworks where `Span<T>` is available.
* Works well with modern .NET apps - `async ... await`, .NET Core, .NET Standard 2.0.
* Supports standard StatsD primitives: `Increment`, `Decrement`, `Timing` and `Gauge`.
* Supports tagging on `Increment`, `Decrement`, `Timing` and `Gauge`.
* Supports sample rate for cutting down of sends of high-volume metrics.
* Helpers to make it easy to time a delegate such as a `Func<T>` or `Action<T>`, or a code block inside a `using` statement.
* Send stats over UDP or IP.
* Send stats to a server by name or IP address.

#### Publishing statistics

`IStatsDPublisher` is the interface that you will use to send stats. The concrete class that implements `IStatsDPublisher` is `StatsDPublisher`. The `StatsDPublisher` constructor takes an instance of `StatsDConfiguration`.

For the configuration's values, you will always need the StatsD server host name or IP address. Optionally, you can also change the port from the default (`8125`). You can also prepend a prefix to all stats. These values often come from configuration as the host name and/or prefix may vary between test and production environments.

It is best practice to create a `StatsDPublisher` at application start and use it for the lifetime of the application, instead of creating a new one for each usage.

## Setting up StatsD

### Simple example of setting up a StatsDPublisher

An example of a very simple StatsD publisher configuration, using the default values for most things.

#### .NET Core

Using `IServiceCollection` and the built-in DI container:

```csharp
// Registration
services.AddStatsD("metrics_server.mycompany.com");
services.AddSingleton<MyService>();

// Consumption
public class MyService
{
    public MyService(IStatsDPublisher statsPublisher)
    {
        // Use the publisher
    }
}

```

#### Simplest example

No service registration or IoC. Works for both .NET 4.5.1 and .NET Core.

```csharp
var statsDConfig = new StatsDConfiguration { Host = "metrics_server.mycompany.com" };
IStatsDPublisher statsDPublisher = new StatsDPublisher(statsDConfig);
```

### IoC Examples

#### .NET Core

An example of registering StatsD dependencies using `IServiceCollection`:

```csharp
services.AddStatsD(
    (provider) =>
    {
        var options = provider.GetRequiredService<MyOptions>().StatsD;

        return new StatsDConfiguration()
        {
            Host = options.HostName,
            Port = options.Port,
            Prefix = options.Prefix,
            OnError = ex => LogError(ex)
        };
    });
```

#### .NET Core (AWS Lambda functions)

An example of registering StatsD dependencies using `IServiceCollection` when using the IP transport, e.g. for an AWS Lambda function:

```csharp

services.AddStatsD(
    (provider) =>
    {
        var options = provider.GetRequiredService<MyOptions>().StatsD;

        return new StatsDConfiguration
        {
            Prefix = options.Prefix,
            Host = options.HostName,
            SocketProtocol = SocketProtocol.IP
        };
    });
```

#### .NET 4.5.1

An example of IoC in Ninject for StatsD publisher with values for all options, read from configuration:

```csharp
// read config values from app settings
string statsdHostName =  ConfigurationManager.AppSettings["statsd.hostname"];
int statsdPort = int.Parse(ConfigurationManager.AppSettings["statsd.port"]);
string statsdPrefix =  ConfigurationManager.AppSettings["statsd.prefix"];

// assemble a StatsDConfiguration object
// since the configuration doesn't change for the lifetime of the app,
// it can be a preconfigured singleton instance
var statsDConfig = new StatsDConfiguration
{
  Host = statsdHostName,
  Port = statsdPort,
  Prefix = statsdPrefix,
  OnError = ex => LogError(ex)
};

// register with NInject
Bind<StatsDConfiguration>().ToConstant(statsDConfig>);
Bind<IStatsDPublisher>().To<StatsDPublisher>().InSingletonScope();
```

## StatsDConfiguration fields

| Name              | Type                    | Default                        | Comments                                                                                                |
|-------------------|-------------------------|--------------------------------|---------------------------------------------------------------------------------------------------------|
| Host              | `string`                |                                | The host name or IP address of the StatsD server. There is no default, this must be set.                |
| Port              | `int`                   | `8125`                         | The StatsD port.                                                                                        |
| DnsLookupInterval | `TimeSpan?`             | `5 minutes`                    | Length of time to cache the host name to IP address lookup. Only used when "Host" contains a host name. |
| Prefix            | `string`                | `string.Empty`                 | Prepend a prefix to all stats.
| SocketProtocol    | `SocketProtocol`, one of `Udp`, `IP`| `Udp`              | Type of socket to use when sending stats to the server.                                                                          |
| OnError           | `Func<Exception, bool>` | `null`                         | Function to receive notification of any exceptions.                                                     |

`OnError` is a function to receive notification of any errors that occur when trying to publish a metric. This function should return:

* `true` if the exception was handled and no further action is needed
* `false` if the exception should be thrown

The default behaviour is to ignore the error.

#### Example of using the interface

Given an existing instance of `IStatsDPublisher` called `stats` you can do for e.g.:

```csharp
stats.Increment("DoSomething.Attempt");
var stopWatch = Stopwatch.StartNew();
var success = DoSomething();

stopWatch.Stop();

var statName = "DoSomething." + success ? "Success" : "Failure";
stats.Timing(statName, stopWatch.Elapsed);
```

#### Simple timers

This syntax for timers less typing in simple cases, where you always want to time the operation, and always with the same stat name. Given an existing instance of `IStatsDPublisher` you can do:

```csharp
//  timing a block of code in a using statement:
using (stats.StartTimer("someStat"))
{
    DoSomething();
}

// also works with async
using (stats.StartTimer("someStat"))
{
    await DoSomethingAsync();
}
```

The `StartTimer` returns an `IDisposableTimer` that wraps a stopwatch and implements `IDisposable`.
The stopwatch is automatically stopped and the metric sent when it falls out of scope and is disposed on the closing `}` of the `using` statement.

##### Changing the name of simple timers

Sometimes the decision of which stat to send should not be taken before the operation completes. e.g. When you are timing http operations and want different status codes to be logged under different stats.

The `IDisposableTimer` has a `Bucket` property to set or change the stat bucket - i.e. the name of the stat. To use this you need a reference to the timer, e.g. `using (var timer = stats.StartTimer("statName"))` instead of `using (stats.StartTimer("statName"))`

The stat name must be set to a non-empty string at the end of the `using` block.

```csharp
using (var timer = stats.StartTimer("SomeHttpOperation."))
{
    var response = DoSomeHttpOperation();
    timer.Bucket = timer.Bucket + (int)response.StatusCode;
    return response;
}
```

##### Functional style

```csharp
//  timing an action without a return value:
stats.Time("someStat", t => DoSomething());

//  timing a function with a return value:
var result = stats.Time("someStat", t => GetSomething());

// and it correctly times async code using the usual syntax:
await stats.Time("someStat", async t => await DoSomethingAsync());
var result = await stats.Time("someStat", async t => await GetSomethingAsync());
```

In all these cases the function or delegate is supplied with a `IDisposableTimer t` so that the stat name can be changed if need be.

##### Credits

The idea of "disposable timers" for using statements is an old one, see for example [this StatsD client](https://github.com/Pereingo/statsd-csharp-client) and [MiniProfiler](https://miniprofiler.com/dotnet/HowTo/ProfileCode).

### How to contribute

See [CONTRIBUTING.md](.github/CONTRIBUTING.md).

### How to release

See [CONTRIBUTING.md](.github/CONTRIBUTING.md#releases).
