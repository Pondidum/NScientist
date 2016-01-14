using System.Collections.Generic;
using System.Linq;

namespace NScientist
{
	public class Results
	{
		public Observation Control { get; set; }
		public Observation Trial => Trials.FirstOrDefault();
		public IEnumerable<Observation> Trials { get; set; }

		public string Name { get; set; }
		public Dictionary<object, object> Context { get; set; }
		public bool ExperimentEnabled { get; set; }
		public bool Matched => Trials.Where(o => o.Ignored == false).All(o => o.Matched);
		public bool Ignored => Trials.Any(o => o.Ignored);

		public Results()
		{
			Context = new Dictionary<object, object>();
		}
	}
}
