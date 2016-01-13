using System;

namespace NScientist
{
	public class Observation
	{
		public string Name { get; set; }
		public TimeSpan Duration { get; set; }
		public Exception Exception { get; set; }
		public object Result { get; set; }
		public object CleanedResult { get; set; }
	}
}
