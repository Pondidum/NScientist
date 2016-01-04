using System;

namespace NScientist
{
	public class Experiment
	{
		public static ExperimentConfig<object> On(Action action)
		{
			return new ExperimentConfig<object>(() =>
			{
				action();
				return null;
			});
		}

		public static ExperimentConfig<T> On<T>(Func<T> action)
		{
			return new ExperimentConfig<T>(action);
		}
	}

	public class ExperimentConfig<TResult>
	{
		private readonly Func<TResult> _control;
		private Action _test;
		private Func<bool> _isEnabled;

		public ExperimentConfig(Func<TResult> action)
		{
			_control = action;
			_isEnabled = () => true;
		}

		public ExperimentConfig<TResult> Try(Action action)
		{
			_test = action;
			return this;
		}

		public ExperimentConfig<TResult> Enabled(Func<bool> isEnabled)
		{
			_isEnabled = isEnabled;
			return this;
		}

		public TResult Run()
		{
			try
			{
				if (_isEnabled())
					_test();
			}
			catch (Exception)
			{
				//not yet...
			}
			
			return _control();
		}

	}
}

