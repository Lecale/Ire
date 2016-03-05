using System;using System.Collections.Generic;

namespace Ire
{
	public class McLayer
	{
		public int MMSKey;
		private List<int> population;

		public McLayer (int MMS, int Seed)
		{
			MMSKey = MMS;
			population.Add (Seed);
		}

		public bool Match(int Seed)
		{
			if (Seed == MMSKey)
				return true;
			return false;
		}
	}
}

