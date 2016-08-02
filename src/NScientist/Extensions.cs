using System;
using System.Collections.Generic;

namespace NScientist
{
	internal static class Extensions
	{
		private static readonly Random Randomiser = new Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			var n = list.Count;

			while (n > 1)
			{
				n--;
				var k = Randomiser.Next(n + 1);
				var value = list[k];

				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
