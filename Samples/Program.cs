using System;
using NScientist;
using Samples.SerilogKibana;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Samples
{
	public class Program
	{
		static void Main()
		{
			var example = new SerilogPublishingExample();
			example.RunExample();

			Console.WriteLine("Done.");
			Console.ReadKey();
		}

	}
}