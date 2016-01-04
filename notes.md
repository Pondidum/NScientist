
* [x] It decides whether or not to run the try block,
* [ ] Randomizes the order in which use and try blocks are run,
* [x] Measures the durations of all behaviors,
* [ ] Compares the result of try to the result of use,
* [x] Swallows (but records) any exceptions raised in the try block, and
* [ ] Publishes all this information.


```csharp
public string GetTemplate(string name, Brands brand)
{
  return Experiment
    .Create("some-test")
    .Context(context => {
      context.name = name;
      context.brand = brand;
    })
    .Use(() => GetTemplateEmbedded(name, brand))
    .Try(() => GetTemplateService(name, brand))
    .CompareWith((control, test) => String.Equals(control.body, test.body, StringComparison.OrdinalIgnoreCase))
    .Enabled(() => true)
    .Publish(results => {
        //results.context
    })
    .Run();
}
```
