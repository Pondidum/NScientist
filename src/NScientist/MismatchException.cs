using System;
using System.Text;

namespace NScientist
{
	public class MismatchException : Exception
	{
		public MismatchException(Results results)
			: base(BuildMessage(results))
		{
		}

		private static string BuildMessage(Results results)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Experiment {results.Name} observations mismatched:");
			sb.AppendLine($"  Control: {results.Control.Result}");
			sb.AppendLine($"  Trial: {results.Trial.Result}");

			return sb.ToString();
		}
	}
}
