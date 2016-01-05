using System;
using System.Collections.Generic;

namespace NScientist
{
	public class Results
	{
		public TimeSpan ControlDuration { get; set; }
		public TimeSpan TryDuration { get; set; }

		public Exception ControlException { get; set; }
		public Exception TryException { get; set; }

		public object ControlResult { get; set; }
		public object TryResult { get; set; }

		public bool ExperimentEnabled { get; set; }

		public bool Matched { get; set; }

		public Dictionary<object, object> Context { get; set; }
	}
}
