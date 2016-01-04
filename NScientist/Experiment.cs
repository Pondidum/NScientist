using System;
using System.Diagnostics;

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
		private Action<Results> _publish;

		public ExperimentConfig(Func<TResult> action)
		{
			_control = action;
			_isEnabled = () => true;
			_publish = results => { };
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

		public ExperimentConfig<TResult> Publish(Action<Results> publish)
		{
			_publish = publish;
			return this;
		}

		public TResult Run()
		{
			var results = new Results();

			try
			{
				if (_isEnabled())
				{
					LogTime(() => _test(), elapsed => results.TryDuration = elapsed);
				}
			}
			catch (Exception ex)
			{
				results.TryException = ex;
			}

			TResult output = default(TResult);

			try
			{
				output = LogTime(() => _control(), elapsed => results.ControlDuration = elapsed);
			}
			catch (Exception ex)
			{
				results.ControlException = ex;
			}

			_publish(results);

			if (results.ControlException != null)
				throw results.ControlException;

			return output;
		}

		private T LogTime<T>(Func<T> action, Action<TimeSpan> result)
		{
			var watch = new Stopwatch();

			try
			{
				watch.Start();
				return action();
			}
			finally
			{
				watch.Stop();
				result(watch.Elapsed);
			}
		}

		private void LogTime(Action action, Action<TimeSpan> result)
		{
			var watch = new Stopwatch();

			try
			{
				watch.Start();
				action();
			}
			finally
			{
				watch.Stop();
				result(watch.Elapsed);
			}
		}
	}

	public class Results
	{
		public TimeSpan ControlDuration { get; set; }
		public TimeSpan TryDuration { get; set; }

		public Exception ControlException { get; set; }
		public Exception TryException { get; set; }
	}
}

