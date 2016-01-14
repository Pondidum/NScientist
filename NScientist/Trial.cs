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

		private readonly Func<TResult> _action;

		public Trial(Func<TResult> action)
		{
			_action = action;
		}

		internal TResult Execute()
		{
			return _action();
		}

		public void Run(Func<TResult, object> cleaner)
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
				dto.CleanedResult = cleaner(result);
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

		public void Evaluate(List<Func<TResult, TResult, bool>> ignores, Func<TResult, TResult, bool> compare, TResult controlResult)
		{
			var trialResult = Observation.Result != null
				? (TResult)Observation.Result
				: default(TResult);

			Observation.Ignored = ignores.Any(check => check(controlResult, trialResult));

			if (Observation.Ignored == false)
				Observation.Matched = compare(controlResult, trialResult);
		}
	}
}