using System;
using System.Threading;
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
			var published = false;

			Experiment
				.On(new Action(() => control = true))
				.Try(() => { throw new TestException(); })
				.Publish(results =>
				{
					results.TryException.ShouldBeOfType<TestException>();
					published = true;
				})
				.Run();

			control.ShouldBe(true);
			published.ShouldBe(true);
		}

		[Fact]
		public void When_the_control_throws_an_exception()
		{
			var published = false;

			Should.Throw<TestException>(() =>
			{
				Experiment
					.On(() => { throw new TestException(); })
					.Try(() => { throw new AlternateException(); })
					.Publish(results =>
					{
						results.ControlException.ShouldBeOfType<TestException>();
						published = true;
					})
					.Run();
			});

			published.ShouldBe(true);
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

		[Fact]
		public void When_measuring_time_taken()
		{
			var published = false;

			Experiment
				.On(() => Thread.Sleep(20))
				.Try(() => Thread.Sleep(10))
				.Publish(results =>
				{
					results.ControlDuration.ShouldBeInRange(TimeSpan.FromMilliseconds(15), TimeSpan.FromMilliseconds(25));
					results.TryDuration.ShouldBe(TimeSpan.FromMilliseconds(5), TimeSpan.FromMilliseconds(15));
					published = true;
				})
				.Run();

			published.ShouldBe(true);
		}
	}
}
