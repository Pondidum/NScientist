using NScientist;
using Serilog.Core;
using Serilog.Events;

namespace Samples.SerilogKibana
{
	public class ObservationDestructuringPolicy : IDestructuringPolicy
	{
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
		{
			var observation = value as Observation;

			if (observation == null)
			{
				result = null;
				return false;
			}

			var dto = new
			{
				observation.Result,
				observation.CleanedResult,
				observation.Exception,
				Duration = observation.Duration.TotalMilliseconds
			};

			result = propertyValueFactory.CreatePropertyValue(dto, true);
			return true;
		}
	}
}