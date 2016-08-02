using System;

namespace NScientist
{
	public class Experiment
	{
		public static ExperimentConfig<T> On<T>(Func<T> action)
		{
			return new ExperimentConfig<T>(action);
		}
	}
}

