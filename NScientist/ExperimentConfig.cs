using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;

namespace NScientist
{
	public class ExperimentConfig<TResult>
	{


		private Func<bool> _isEnabled;
		private Action<Results> _publish;
		private Func<TResult, TResult, bool> _compare;
		private Func<Dictionary<object, object>> _createContext;
		private Func<TResult, object> _cleaner;
		private bool _throwMismatches;
		private bool _parallel;


		private readonly Trial<TResult> _control;
		private readonly List<Func<TResult, TResult, bool>> _ignores;
		private readonly List<Trial<TResult>> _tests;

		public ExperimentConfig(Func<TResult> action)
		{
			_control = new Trial<TResult>(action) { TrialName = "Unnamed Experiment" };
			_tests = new List<Trial<TResult>>();
			_ignores = new List<Func<TResult, TResult, bool>>();

			_isEnabled = () => true;
			_publish = results => { };
			_compare = (control, experiment) => Equals(control, experiment);
			_createContext = () => new Dictionary<object, object>();
			_cleaner = results => null;
			_throwMismatches = false;
			_parallel = false;
		}

		public ExperimentConfig<TResult> Try(Func<TResult> action)
		{
			return Try("Trial " + _tests.Count, action);
		}

		public ExperimentConfig<TResult> Try(string trialName, Func<TResult> action)
		{
			_tests.Add(new Trial<TResult>(action) { TrialName = trialName });
			return this;
		}

		public ExperimentConfig<TResult> Enabled(Func<bool> isEnabled)
		{
			_isEnabled = isEnabled;
			return this;
		}

		public ExperimentConfig<TResult> CompareWith(Func<TResult, TResult, bool> compare)
		{
			_compare = compare;
			return this;
		}

		public ExperimentConfig<TResult> Ignore(Func<TResult, TResult, bool> ignore)
		{
			_ignores.Add(ignore);
			return this;
		}

		public ExperimentConfig<TResult> Context(Func<Dictionary<object, object>> createContext)
		{
			_createContext = createContext;
			return this;
		}

		public ExperimentConfig<TResult> Parallel()
		{
			_parallel = true;
			return this;
		}

		public ExperimentConfig<TResult> Publish(IPublisher publisher)
		{
			return Publish(publisher.Publish);
		}

		public ExperimentConfig<TResult> Publish(Action<Results> publish)
		{
			_publish = publish;
			return this;
		}

		public ExperimentConfig<TResult> Called(string name)
		{
			_control.TrialName = name;
			return this;
		}

		public ExperimentConfig<TResult> Clean<TCleaned>(Func<TResult, TCleaned> cleaner)
		{
			_cleaner = results => cleaner(results);
			return this;
		}

		public ExperimentConfig<TResult> ThrowMismatches()
		{
			_throwMismatches = true;
			return this;
		}

		public TResult Run()
		{
			var enabled = _isEnabled();

			if (enabled == false)
				return _control.Execute();

			var actions = new List<Action>();

			actions.Add(() => _control.Run(_cleaner));
			actions.AddRange(_tests.Select(trial => (Action)(() => trial.Run(_cleaner))));

			actions.Shuffle();

			if (_parallel)
				actions.AsParallel().ForAll(action => action());
			else
				actions.ForEach(action => action());

			var controlResult = _control.Observation.Result != null
				? (TResult)_control.Observation.Result
				: default(TResult);

			foreach (var trial in _tests)
				trial.Evaluate(_ignores, _compare, controlResult);

			var results = new Results
			{
				Name = _control.TrialName,
				Context = _createContext(),
				ExperimentEnabled = true,
				Control = _control.Observation,
				Trials = _tests.Select(t => t.Observation)
			};

			_publish(results);

			if (_throwMismatches && results.Trials.Any(o => o.Matched == false))
				throw new MismatchException(results);

			if (results.Control.Exception != null)
				throw results.Control.Exception;

			return (TResult)_control.Observation.Result;
		}
	}
}
