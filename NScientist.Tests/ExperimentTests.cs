using System;
using Shouldly;
using Xunit;

namespace NScientist.Tests
{
	public class ExperimentTests
	{
		[Fact]
		public void When_running_a_void_action()
		{
			var control = false;
			var test = false;

			Experiment
				.On(new Action(() => control = true))
				.Try(() => test = true)
				.Run();

			control.ShouldBe(true);
			test.ShouldBe(true);
		}

		[Fact]
		public void When_the_try_throws_an_exception()
		{
			var control = false;

			Experiment
				.On(new Action(() => control = true))
				.Try(() => { throw new NotSupportedException(); })
				.Run();

			control.ShouldBe(true);
		}

		[Fact]
		public void When_running_an_action_with_result()
		{
			var control = 0;
			var test = 0;

			var result = Experiment
				.On(() => control += 10)
				.Try(() => test += 20)
				.Run();

			control.ShouldBe(10);
			test.ShouldBe(20);
			result.ShouldBe(10);
		}

		[Fact]
		public void When_the_experiment_is_disabled()
		{
			var control = 0;
			var test = 0;

			var result = Experiment
				.On(() => control += 10)
				.Try(() => test += 20)
				.Enabled(() => false)
				.Run();

			control.ShouldBe(10);
			test.ShouldBe(0);
			result.ShouldBe(10);
		}
	}
}
