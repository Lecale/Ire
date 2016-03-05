using System;using System.Collections.Generic;

namespace Ire
{
	public class McLayer
	{
		public int MMSKey;
		private List<int> population;
		private Random r;
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

		private void Shuffle()//not very random but it will do
		{
			int hold;
			int tmp;
			for (int i = 0; i < population.Count; i++) {
				tmp = r.Next (0, population.Count);
				hold = population[tmp];
				population [tmp] = population [i];
				population [i] = hold;
			}
		}

		public int GetAt(int i)
		{
			return population[i];
		}


	}
}

