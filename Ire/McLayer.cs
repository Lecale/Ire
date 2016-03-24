using System;using System.Collections.Generic;

namespace Ire
{
	public class McLayer
	{
		public float MMSKey;
		public int Length = 0;

		private List<int> population;
		private Random r;
		public McLayer (float MMS, int Seed)
		{
			MMSKey = MMS;
			population = new List<int> ();
			population.Add (Seed);
			Length = 1;
		}

		public bool Match(float _mms)
		{
			if (_mms == MMSKey)
				return true;
			return false;
		}

		public void Shuffle()//not very random but it will do for now
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

		public void Add(int _seed)
		{
			population.Add(_seed);
			Length++;
		}

		public void Remove(int _Seed)
		{
			if (population.Count > 0) {
				population.Remove (_Seed);
				Length--;
			}
		}
	}
}

