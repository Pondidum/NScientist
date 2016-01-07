# NScientist!

A C# library for carefully refactoring critical paths.  Based off of the spec of [Github's Scientist][github-scientist].

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

The `On()` block is called the **Control**. The `Try()` block is called the **Experiment**.


## Comparing Results
By default, NScientist compares the results of the `On()` and `Try()` blocks using `object.Equals(control, experiment);`.  This can be overriden by specifying a custom function with the `CompareWith()` block:

```csharp
public string GetTemplate(string name, int version)
{
  return Experiment
    .On(() => OldStore.GetTemplate(name, version))
    .Try(() => TemplateService.Fetch(name, version))
    .CompareWith((control, exp) => string.Equals(control, experiment, StringComparison.OrdinalIgnoreCase))
    .Run();
}
```

## Adding Context
If your methods take parameters in, it would probably be a good idea to publish these too - this can be done with the `Context()` block:

```csharp
public string GetTemplate(string name, int version)
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

[github-scientist]: https://github.com/github/scientist
