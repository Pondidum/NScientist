using System;

namespace NScientist
{
	public class Experiment
	{
		public static ExperimentConfig<object> On(Action action)
		{
			return new ExperimentConfig<object>();
		}

		public static ExperimentConfig<T> On<T>(Func<T> action)
		{
			return new ExperimentConfig<T>();
		}
	}

	public class ExperimentConfig<TResult>
	{
		public ExperimentConfig<TResult> Try(Action action)
		{
			throw new NotImplementedException();
		}

		public TResult Run()
		{
			throw new NotImplementedException();
		}
	}
}

