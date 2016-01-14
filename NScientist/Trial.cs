using System;
using System.Diagnostics;

namespace NScientist
{
	public class Trial<TResult>
	{
		public string TrialName { get; set; }
		private readonly Func<TResult> _action;

		public Trial(Func<TResult> action)
		{
			_action = action;
		}

		internal TResult Execute()
		{
			return _action();
		}

		public Observation Run(Func<TResult, object> cleaner)
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

			return dto;
		}
	}
}