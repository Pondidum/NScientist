using Shouldly;
using Xunit;

namespace NScientist.Tests
{
	public class Scratchpad
	{
		[Fact]
		public void When_testing_something()
		{
			
		}

		[Fact]
		public void When_passing()
		{
			Add(2, 2).ShouldBe(4);
		}

		[Fact]
		public void When_failing()
		{
			Add(2, 2).ShouldBe(5);
		}

		private static int Add(int x, int y) => x + y;
	}
}
