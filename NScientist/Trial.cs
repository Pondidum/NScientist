using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NScientist
{
	public class Trial<TResult>
	{
		public string TrialName { get; set; }
		public Observation Observation { get; private set; }

		private readonly ExperimentConfig<TResult> _experiment;
		private readonly Func<TResult> _action;

		public Trial(ExperimentConfig<TResult> experiment, Func<TResult> action)
		{
			_experiment = experiment;
			_action = action;
		}

		internal TResult Execute()
		{
			return _action();
		}

		public void Run()
		{
			var dto = new Observation();
			var sw = new Stopwatch();

			try
			{
				sw.Start();
				var result = _action();
				sw.Stop();

				dto.Name = TrialName;
				dto.Result = result;
				dto.CleanedResult  =_experiment.Cleaner(result);
			}
			catch (Exception ex)
			{
				sw.Stop();
				dto.Exception = ex;
			}
			finally
			{
				dto.Duration = sw.Elapsed;
			}

			Observation = dto;
		}

		public void Evaluate(TResult controlResult)
		{
			var trialResult = Observation.Result != null
				? (TResult)Observation.Result
				: default(TResult);

			Observation.Ignored = _experiment.Ignores.Any(check => check(controlResult, trialResult));

			if (Observation.Ignored == false)
				Observation.Matched = _experiment.Compare(controlResult, trialResult);
		}
	}
}