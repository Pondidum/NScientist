using NScientist;
using Serilog;
using Serilog.Context;

namespace Samples.SerilogKibana
{
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
}
