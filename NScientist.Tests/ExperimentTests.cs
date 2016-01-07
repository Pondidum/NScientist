using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Shouldly;
using Xunit;

namespace NScientist.Tests
{
	public class ExperimentTests
	{
		private Results _result;

		private void ToThis(Results results)
		{
			_result = results;
		}

		[Fact]
		public void When_the_try_throws_an_exception()
		{
			Experiment
				.On(() => true)
				.Try(() => { throw new TestException(); })
				.Publish(ToThis)
				.Run();

			_result.ExperimentException.ShouldBeOfType<TestException>();
			_result.ControlResult.ShouldBe(true);
		}

		[Fact]
		public void When_the_control_throws_an_exception()
		{
			Should.Throw<TestException>(() =>
			{
				Experiment
					.On<bool>(() => { throw new TestException(); })
					.Try(() => { throw new AlternateException(); })
					.Publish(ToThis)
					.Run();
			});

			_result.ControlException.ShouldBeOfType<TestException>();
		}

		[Fact]
		public void When_running_an_action_with_result()
		{
			var result = Experiment
				.On(() => 10)
				.Try(() => 20)
				.Publish(ToThis)
				.Run();

			_result.ControlResult.ShouldBe(10);
			_result.ExperimentResult.ShouldBe(20);
			result.ShouldBe(10);
		}

		[Fact]
		public void When_the_experiment_is_disabled()
		{
			var output = Experiment
				.On(() => 10)
				.Try(() => 20)
				.Enabled(() => false)
				.Publish(ToThis)
				.Run();

			_result.ShouldBe(null);
			output.ShouldBe(10);
		}

		[Fact]
		public void When_measuring_time_taken()
		{
			var published = false;

			Experiment
				.On(() => { Thread.Sleep(20); return true; })
				.Try(() => { Thread.Sleep(10); return true; })
				.Publish(results =>
				{
					results.ControlDuration.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(20));
					results.ExperimentDuration.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(10));
					published = true;
				})
				.Run();

			published.ShouldBe(true);
		}

		[Fact]
		public void When_running_the_order_is_random()
		{
			var last = "";
			var runs = new HashSet<string>();

			var ex = Experiment
				.On(() => last = "control")
				.Try(() => last = "try");

			for (int i = 0; i < 1000; i++)
			{
				ex.Run();
				runs.Add(last);
			}

			runs.Distinct().Count().ShouldBeGreaterThan(1);
		}

		[Fact]
		public void When_the_control_and_experiment_match()
		{
			Experiment
				.On(() => "test")
				.Try(() => "test")
				.Publish(ToThis)
				.Run();

			_result.Matched.ShouldBe(true);
		}

		[Fact]
		public void When_the_control_and_the_experiment_dont_match()
		{
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Publish(ToThis)
				.Run();

			_result.Matched.ShouldBe(false);
		}

		[Fact]
		public void When_using_a_custom_comparer()
		{
			Experiment
				.On(() => "test")
				.Try(() => "TEST")
				.Publish(ToThis)
				.CompareWith((control, experiment) => string.Equals(control, experiment, StringComparison.OrdinalIgnoreCase))
				.Run();

			_result.Matched.ShouldBe(true);
		}

		[Fact]
		public void When_adding_information_to_the_context()
		{
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Context(() => new Dictionary<object, object>
				{
					{"one", "two" }
				})
				.Publish(ToThis)
				.Run();

			_result.Context["one"].ShouldBe("two");
		}

		[Fact]
		public void When_an_experiment_has_no_name()
		{
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Publish(ToThis)
				.Run();

			_result.Name.ShouldBe("Unnamed Experiment");
		}

		[Fact]
		public void When_setting_the_experiments_name()
		{
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Called("experiment 01")
				.Publish(ToThis)
				.Run();

			_result.Name.ShouldBe("experiment 01");
		}

		[Fact]
		public void When_there_is_no_cleaner_specified()
		{
			Experiment
				.On(() => new[] { "1", "2", "3" })
				.Try(() => new[] { "" })
				.Publish(ToThis)
				.Run();

			_result.ExperimentCleanedResult.ShouldBe(null);
			_result.ControlCleanedResult.ShouldBe(null);
		}

		[Fact]
		public void When_cleaning_results()
		{
			Experiment
				.On(() => new[] { "1", "2", "3" })
				.Try(() => new string[0])
				.Clean(results => results.Select(r => Convert.ToInt32(r)))
				.Publish(ToThis)
				.Run();

			_result.ExperimentCleanedResult.ShouldBe(Enumerable.Empty<int>());
			_result.ControlCleanedResult.ShouldBe(new[] { 1, 2, 3 });
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("base", "base", true)]
		public void When_ignoring_all_missmatches(string baseline, string attempt, bool matches)
		{
			Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, experiment) => true)
				.Publish(ToThis)
				.Run();

			_result.Matched.ShouldBe(matches);
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("base", "base", true)]
		[InlineData("something", "experiment", false)]
		public void When_ignoring_a_specific_missmatch(string baseline, string attempt, bool matches)
		{
			Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, experiment) => control == "base")
				.Publish(ToThis)
				.Run();

			_result.Matched.ShouldBe(matches);
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("another", "different", false)]
		[InlineData("something", "experiment", true)]
		public void When_ignoring_multiple_mismatches(string baseline, string attempt, bool matches)
		{
			var exp = Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, experiment) => control == "base")
				.Ignore((control, experiment) => experiment == "experiment")
				.Publish(ToThis)
				.Run();

			_result.Matched.ShouldBe(matches);
		}

	}
}
