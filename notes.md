


public void GetTemplate(string name, Brands brand)
{
  var experiment = new Experiment(
    "some-test",
    use: () => GetTemplateEmbedded(name, brand),
    try: () => GetTemplateService(name, brand)
  );

  experiment.Run();
}




public void GetTemplate(string name, Brands brand)
{
  Experiment
    .Create("some-test")
    .Use(() => GetTemplateEmbedded(name, brand))
    .Try(() => GetTemplateService(name, brand))
    .Run();
}

public void GetTemplate(string name, Brands brand)
{
  Experiment
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
