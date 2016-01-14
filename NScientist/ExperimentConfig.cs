using System;
using System.Collections.Generic;
using System.Linq;

namespace NScientist
{
	public class ExperimentConfig<TResult>
	{
		private Func<bool> _isEnabled;
		private Action<Results> _publish;

		private Func<Dictionary<object, object>> _createContext;
		private bool _throwMismatches;
		private bool _parallel;

		public Func<TResult, TResult, bool> Compare { get; private set; }
		public Func<TResult, object> Cleaner { get; private set; }
		public List<Func<TResult, TResult, bool>> Ignores { get; }

		private readonly Trial<TResult> _control;
		private readonly List<Trial<TResult>> _trials;

		public ExperimentConfig(Func<TResult> action)
		{
			_control = new Trial<TResult>(this, action) { TrialName = "Unnamed Experiment" };
			_trials = new List<Trial<TResult>>();

			Ignores = new List<Func<TResult, TResult, bool>>();
			Compare = (control, experiment) => Equals(control, experiment);
			Cleaner = results => null;

			_isEnabled = () => true;
			_publish = results => { };
			_createContext = () => new Dictionary<object, object>();
			_throwMismatches = false;
			_parallel = false;
		}

		public ExperimentConfig<TResult> Try(Func<TResult> action)
		{
			return Try("Trial " + _trials.Count, action);
		}

		public ExperimentConfig<TResult> Try(string trialName, Func<TResult> action)
		{
			_trials.Add(new Trial<TResult>(this, action) { TrialName = trialName });
			return this;
		}

		public ExperimentConfig<TResult> Enabled(Func<bool> isEnabled)
		{
			_isEnabled = isEnabled;
			return this;
		}

		public ExperimentConfig<TResult> CompareWith(Func<TResult, TResult, bool> compare)
		{
			Compare = compare;
			return this;
		}

		public ExperimentConfig<TResult> Ignore(Func<TResult, TResult, bool> ignore)
		{
			Ignores.Add(ignore);
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
			Cleaner = results => cleaner(results);
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

			actions.Add(() => _control.Run());
			actions.AddRange(_trials.Select(trial => (Action)trial.Run));

			actions.Shuffle();

			if (_parallel)
				actions.AsParallel().ForAll(action => action());
			else
				actions.ForEach(action => action());

			var controlResult = _control.Observation.Result != null
				? (TResult)_control.Observation.Result
				: default(TResult);

			foreach (var trial in _trials)
				trial.Evaluate(controlResult);

			var results = new Results
			{
				Name = _control.TrialName,
				Context = _createContext(),
				ExperimentEnabled = true,
				Control = _control.Observation,
				Trials = _trials.Select(t => t.Observation)
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
