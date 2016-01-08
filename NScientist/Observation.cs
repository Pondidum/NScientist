using System;

namespace NScientist
{
	public class Observation
	{
		public TimeSpan Duration { get; set; }
		public Exception Exception { get; set; }
		public object Result { get; set; }
		public object CleanedResult { get; set; }
	}
}
