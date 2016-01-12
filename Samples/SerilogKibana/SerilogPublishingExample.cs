using System;
using System.Threading;
using NScientist;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Samples.SerilogKibana
{
	public class SerilogPublishingExample
	{
		private static readonly Random Random = new Random();

		public void RunExample()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.ColoredConsole()
				//an example kibana instance, using docker and the sebp/elk image
				.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://192.168.99.100:9200")) { AutoRegisterTemplate = true })
				//To get pretty graphs in kibana, you need to use the custom
				//destructuring policy so that TimeSpan get converted to milliseconds
				.Destructure.With<ObservationDestructuringPolicy>()
				.Enrich.FromLogContext()
				.CreateLogger();

			for (int i = 0; i < 1000; i++)
				GetTemplate();
		}


		private bool GetTemplateOriginal()
		{
			Thread.Sleep(Random.Next(50, 1500));
			return true;
		}

		private bool GetTemplateReplacement()
		{
			Thread.Sleep(Random.Next(50, 750));
			return Random.Next(0, 100) >= 10;
		}

		public void GetTemplate()
		{
			Experiment
				.On(() => GetTemplateOriginal())
				.Try(() => GetTemplateReplacement())
				.Publish(SerilogPublisher.Instance)
				.Run();
		}
	}
}
