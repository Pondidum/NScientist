using System;
using System.Collections.Generic;

namespace NScientist
{
	public class Results
	{
		public TimeSpan ControlDuration { get; set; }
		public TimeSpan ExperimentDuration { get; set; }

		public Exception ControlException { get; set; }
		public Exception ExperimentException { get; set; }

		public object ControlResult { get; set; }
		public object ExperimentResult { get; set; }

		public object ControlCleanedResult { get; set; }
		public object ExperimentCleanedResult { get; set; }

		public string Name { get; set; }
		public Dictionary<object, object> Context { get; set; }
		public bool ExperimentEnabled { get; set; }
		public bool Matched { get; set; }

		public Results()
		{
			Matched = true;
			Context = new Dictionary<object, object>();
		}
	}
}
