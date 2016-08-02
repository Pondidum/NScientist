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
			Assert.Equal(4, Add(2, 2));
		}

		[Fact]
		public void When_failing()
		{
			Assert.Equal(5, Add(2, 2));
		}

		private static int Add(int x, int y) => x + y;
	}
}
