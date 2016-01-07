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
		[Fact]
		public void When_the_try_throws_an_exception()
		{
			var control = false;
			var published = false;

			Experiment
				.On(() => control = true)
				.Try(() => { throw new TestException(); })
				.Publish(results =>
				{
					results.ExperimentException.ShouldBeOfType<TestException>();
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
					.On<bool>(() => { throw new TestException(); })
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
			Results result = null;

			var output = Experiment
				.On(() => 10)
				.Try(() => 20)
				.Enabled(() => false)
				.Publish(r => result = r)
				.Run();

			result.ShouldBe(null);
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
			Results result = null;

			Experiment
				.On(() => "test")
				.Try(() => "test")
				.Publish(r => result = r)
				.Run();

			result.Matched.ShouldBe(true);
		}

		[Fact]
		public void When_the_control_and_the_experiment_dont_match()
		{
			Results result = null;

			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Publish(r => result = r)
				.Run();

			result.Matched.ShouldBe(false);
		}

		[Fact]
		public void When_using_a_custom_comparer()
		{
			Results result = null;

			Experiment
				.On(() => "test")
				.Try(() => "TEST")
				.Publish(r => result = r)
				.CompareWith((control, experiment) => string.Equals(control, experiment, StringComparison.OrdinalIgnoreCase))
				.Run();

			result.Matched.ShouldBe(true);
		}

		[Fact]
		public void When_adding_information_to_the_context()
		{
			Results result = null;
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Context(() => new Dictionary<object, object>
				{
					{"one", "two" }
				})
				.Publish(r => result = r)
				.Run();

			result.Context["one"].ShouldBe("two");
		}

		[Fact]
		public void When_an_experiment_has_no_name()
		{
			Results result = null;
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Publish(r => result = r)
				.Run();

			result.Name.ShouldBe("Unnamed Experiment");
		}

		[Fact]
		public void When_setting_the_experiments_name()
		{
			Results result = null;
			Experiment
				.On(() => "omg")
				.Try(() => "oh noes")
				.Called("experiment 01")
				.Publish(r => result = r)
				.Run();

			result.Name.ShouldBe("experiment 01");
		}

		[Fact]
		public void When_there_is_no_cleaner_specified()
		{
			Results result = null;

			Experiment
				.On(() => new[] { "1", "2", "3" })
				.Try(() => new[] { "" })
				.Publish(r => result = r)
				.Run();

			result.ExperimentCleanedResult.ShouldBe(null);
			result.ControlCleanedResult.ShouldBe(null);
		}

		[Fact]
		public void When_cleaning_results()
		{
			Results result = null;

			Experiment
				.On(() => new[] { "1", "2", "3" })
				.Try(() => new string[0])
				.Clean(results => results.Select(r => Convert.ToInt32(r)))
				.Publish(r => result = r)
				.Run();

			result.ExperimentCleanedResult.ShouldBe(Enumerable.Empty<int>());
			result.ControlCleanedResult.ShouldBe(new[] { 1, 2, 3 });
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("base", "base", true)]
		public void When_ignoring_all_missmatches(string baseline, string attempt, bool matches)
		{
			Results result = null;

			Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, experiment) => true)
				.Publish(r => result = r)
				.Run();

			result.Matched.ShouldBe(matches);
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("base", "base", true)]
		[InlineData("something", "experiment", false)]
		public void When_ignoring_a_specific_missmatch(string baseline, string attempt, bool matches)
		{
			Results result = null;

			Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, experiment) => control == "base")
				.Publish(r => result = r)
				.Run();

			result.Matched.ShouldBe(matches);
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("another", "different", false)]
		[InlineData("something", "experiment", true)]
		public void When_ignoring_multiple_mismatches(string baseline, string attempt, bool matches)
		{
			Results result = null;

			var exp = Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, experiment) => control == "base")
				.Ignore((control, experiment) => experiment == "experiment")
				.Publish(r => result = r)
				.Run();

			result.Matched.ShouldBe(matches);
		}

	}
}
