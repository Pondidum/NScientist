using System.Collections.Generic;
using System.Linq;

namespace NScientist
{
	public class Results
	{
		public Observation Control { get; set; }
		public Observation Trial => _observations.FirstOrDefault();
		public IEnumerable<Observation> Trials => _observations;

		public string Name { get; set; }
		public Dictionary<object, object> Context { get; set; }
		public bool ExperimentEnabled { get; set; }
		public bool Matched => _observations.Where(o => o.Ignored == false).All(o => o.Matched);
		public bool Ignored => _observations.Any(o => o.Ignored);

		private readonly List<Observation> _observations;

		public Results()
		{
			_observations = new List<Observation>();
			Context = new Dictionary<object, object>();
		}

		public void AddObservation(Observation observation)
		{
			_observations.Add(observation);
		}
	}
}
