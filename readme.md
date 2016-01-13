# NScientist!

A C# library for carefully refactoring critical paths.  Based off of the spec of [Github's Scientist][github-scientist].

[![Build status](https://ci.appveyor.com/api/projects/status/is2kpywdfaovf01e?svg=true)](https://ci.appveyor.com/project/Pondidum/nscientist)

## Installation

```sh
PM> install-package nscientist
```

## How do I do the sciencing?

Say you wanted to change how permissions were checked in a web project.  Your new method has been carefully designed, refactored an tested, but you won't properly know how it behaves under load until it is running under a live system.

```csharp
public bool CanView()
{
  return Experiment
    .On(() => PermissionStore.Instance().Can(Permissions.View)) //old way
    .Try(() => UserPrincipal.HasClaim(Claims.View, Values.Allow) //new way
    .Run()
}
```
We wrap our existing method of checking a permission with the `On(() => { ... })` lambda, and put our new method in the `Try(() => { ... })` lambda.  The `Run()` call will always return the results of the `On()` block, but it also does a lot of things in the background:

* Decideds wheter to run the `Try()` block.
* Randomizes the order `Try()` and `On()` blocks are run.
* Stores the duration each block takes to run.
* Compares the results of both blocks.
* Swallows (and records) any exceptions thrown by the `Try()` block.
* Publishes all of this information.

The `On()` block is called the **Control**. The `Try()` block is called the **Trial**.


## Comparing Results
By default, NScientist compares the results of the `On()` and `Try()` blocks using `object.Equals(control, experiment);`.  This can be overriden by specifying a custom function with the `CompareWith()` block:

```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .CompareWith((control, trial) => string.Equals(control.Content, trial.Content, StringComparison.OrdinalIgnoreCase))
    .Run();
}
```

## Adding Context
If your methods take parameters in, it would probably be a good idea to publish these too - this can be done with the `Context()` block:

```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .Context(() => new Dictionary<object, object> {
        { "name", name },
        { "version", version }
    })
    .Run();
}
```

## Cleaning Results
Sometimes you don't want to log the full results - they might be too big, or maybe you only care about a single property of the results.  You can tell NScientist how to perform a clean of the results:

```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .Clean(result => result.ID)
    .Publish(result => {
      result.Control.Result;    // { Template ... }
      result.Control.CleanedResult;  // Guid
      result.Trial.Result;    // { Template ... }
      result.Trial.CleanedResult;  // Guid
    })
    .Run();
}
```
It is up to the publisher to decided whether to log the non-cleaned result if there is also a cleaned result.

## Ignoring Mismatches
Sometimes during development, some of your code can produce mismatches for reasons you know about, but have not yet fixed.  You can tell an experiment to ignore certain conditions so they don't get flagged as errors:

```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .Ignore((control, trial) => trial.IsDraft) //ignore drafts for now
    .Run();
}
```
You can also use multiple `Ignore()` blocks with multiple conditions.

## Enabling and Disabling Experiments
Sometimes you don't want an experiment to run at all - or only on a subset of calls to the function it wraps.  You can configure this using the `Enable()` block:

```csharp
public Template GetTemplate(string name, int version)
{
  var random = new Random();
  var triggerPercent = 20;

  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .Enable(() => triggerPercent > 0 && triggerPercent > random.Next(100))
    .Run();
}
```

## Async
NScientist also supports running your control and trial in parallel - while still shuffling the start order.

```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .Parallel()
    .Run();
}
```

## Testing
When testing, it can be useful to see all the mismatches occouring, even those you have `Ignored()`.  You can tell NScientist to throw an exception whenever there is a mismatch - **don't leave this in your production code!**
```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .ThrowMismatches()
    .Run();
}
```
`ThrowMismatches()` will cause a `MismatchException` to be raised whenever the `control` and `trial` results do not match.

## Publishing Results

Publishing results can be done either by passing in a lambda to the `Publish` block, or by passing an instance of an `IPublisher`.  For example the `SerilogPublisher`:

```csharp
public class SerilogPublisher : IPublisher
{
  public static readonly SerilogPublisher Instance = new SerilogPublisher();
  private static readonly ILogger Log = Serilog.Log.ForContext<SerilogPublisher>();

  public void Publish(Results results)
  {
    using (LogContext.PushProperty("results", results, destructureObjects: true))
    {
      Log.Information("Experiment {experimentName}", results.Name);
    }
  }
}
```
We use the static field so that we avoid recreating the publisher for each experiment run:
```csharp
public Template GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .Publish(SerilogPublisher.Instance)
    .Run();
}
```
If you publish your results to the [serilog.sinks.elasticsearch sink][nuget-serilog-es] you can then generate some pretty graphs in [Kibana][kibana].  And science is all about graphs!

![Kibana Dashboard][nscientist-dashboard]

[github-scientist]: https://github.com/github/scientist
[nscientist-dashboard]: docs/nscientist.dashboard.png
[nuget-serilog-es]: https://www.nuget.org/packages/Serilog.Sinks.ElasticSearch/
[kibana]: https://www.elastic.co/products/kibana
