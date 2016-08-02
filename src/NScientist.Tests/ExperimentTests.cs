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

			_result.Trial.Exception.ShouldBeOfType<TestException>();
			_result.Control.Result.ShouldBe(true);
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

			_result.Control.Exception.ShouldBeOfType<TestException>();
		}

		[Fact]
		public void When_running_an_action_with_result()
		{
			var result = Experiment
				.On(() => 10)
				.Try(() => 20)
				.Publish(ToThis)
				.Run();

			_result.Control.Result.ShouldBe(10);
			_result.Trial.Result.ShouldBe(20);
			_result.Trials.Single().ShouldBe(_result.Trial);
			result.ShouldBe(10);
		}

		[Fact]
		public void When_running_multiple_actions()
		{
			var result = Experiment
				.On(() => 10)
				.Try(() => 20)
				.Try(() => 30)
				.Publish(ToThis)
				.Run();

			_result.Control.Result.ShouldBe(10);
			_result.Trials.Select(o => o.Result).ShouldBe(new object[] { 20, 30 });
			result.ShouldBe(10);
		}

		[Fact]
		public void When_running_a_named_trial()
		{
			Experiment
				.On(() => 10)
				.Try("Test 1", () => 20)
				.Publish(ToThis)
				.Run();

			_result.Trial.Name.ShouldBe("Test 1");
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
			Experiment
				.On(() => { Thread.Sleep(20); return true; })
				.Try(() => { Thread.Sleep(10); return true; })
				.Publish(ToThis)
				.Run();

			_result.Control.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(20));
			_result.Trial.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(10));
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
		public void When_running_in_parallel_the_order_is_random()
		{
			var last = "";
			var runs = new HashSet<string>();

			var ex = Experiment
				.On(() => last = "control")
				.Try(() => last = "try")
				.Parallel();

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
				.CompareWith((control, trial) => string.Equals(control, trial, StringComparison.OrdinalIgnoreCase))
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
			_result.Control.Name.ShouldBe("Unnamed Experiment");
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
			_result.Control.Name.ShouldBe("experiment 01");
		}

		[Fact]
		public void When_there_is_no_cleaner_specified()
		{
			Experiment
				.On(() => new[] { "1", "2", "3" })
				.Try(() => new[] { "" })
				.Publish(ToThis)
				.Run();

			_result.Trial.CleanedResult.ShouldBe(null);
			_result.Control.CleanedResult.ShouldBe(null);
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

			_result.Trial.CleanedResult.ShouldBe(Enumerable.Empty<int>());
			_result.Control.CleanedResult.ShouldBe(new[] { 1, 2, 3 });
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("base", "base", true)]
		public void When_ignoring_all_mismatches(string baseline, string attempt, bool ignored)
		{
			Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, trial) => true)
				.Publish(ToThis)
				.Run();

			_result.Ignored.ShouldBe(ignored);
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("base", "base", true)]
		[InlineData("something", "experiment", false)]
		public void When_ignoring_a_specific_mismatch(string baseline, string attempt, bool ignored)
		{
			Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, trial) => control == "base")
				.Publish(ToThis)
				.Run();

			_result.Ignored.ShouldBe(ignored);
		}

		[Theory]
		[InlineData("base", "experiment", true)]
		[InlineData("another", "different", false)]
		[InlineData("something", "experiment", true)]
		public void When_ignoring_multiple_mismatches(string baseline, string attempt, bool ignored)
		{
			var exp = Experiment
				.On(() => baseline)
				.Try(() => attempt)
				.Ignore((control, trial) => control == "base")
				.Ignore((control, trial) => trial == "experiment")
				.Publish(ToThis)
				.Run();

			_result.Ignored.ShouldBe(ignored);
		}

		[Fact]
		public void When_throwing_mismatches_and_they_match()
		{
			Experiment
				.On(() => "baseline")
				.Try(() => "baseline")
				.Publish(ToThis)
				.ThrowMismatches()
				.Run();

			_result.Matched.ShouldBe(true);
		}

		[Fact]
		public void When_throwing_mismatches_and_they_dont_match()
		{
			Should.Throw<MismatchException>(() =>
				Experiment
					.On(() => "baseline")
					.Try(() => "something else")
					.Publish(ToThis)
					.ThrowMismatches()
					.Run()
				);
		}

		[Fact]
		public void When_throwing_mismatches_and_they_dont_match_and_the_experiment_is_ignored()
		{
			Should.Throw<MismatchException>(() =>
				Experiment
					.On(() => "baseline")
					.Try(() => "something else")
					.Ignore((control, trial) => true)
					.Publish(ToThis)
					.ThrowMismatches()
					.Run()
				);
		}

		[Fact]
		public void When_publishing_to_a_class()
		{
			var publisher = new TestPublisher();

			Experiment
				.On(() => true)
				.Try(() => true)
				.Publish(publisher)
				.Run();

			publisher.Results.Matched.ShouldBe(true);
		}

		[Fact]
		public void When_switch_to_trial_is_false()
		{
			var result = Experiment
				.On(() => true)
				.Try(() => false)
				.SwitchToTrial(() => false)
				.Run();

			result.ShouldBeTrue();
		}

		[Fact]
		public void When_switch_to_trial_is_true_and_only_one_trial_set()
		{
			var result = Experiment
				.On(() => true)
				.Try(() => false)
				.SwitchToTrial(() => true)
				.Publish((r) => { string str = null; str.ToString(); })
				.Run();

			result.ShouldBeFalse();
		}

		[Fact]
		public void When_switch_to_trial_is_true_and_more_than_one_trial_set()
		{
			var result = Experiment
				.On(() => true)
				.Try(() => false)
				.Try(() => false)
				.SwitchToTrial(() => true)
				.Run();

			result.ShouldBeTrue();
		}

		private class TestPublisher : IPublisher
		{
			public Results Results { get; private set; }

			public void Publish(Results results)
			{
				Results = results;
			}
		}
	}
}
