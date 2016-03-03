using System;

namespace Ire
{
	public class PairPerson
	{
		public int seed;
		private int[] opponents;

		public PairPerson (int _seed, int[] _opponents)
		{
			seed = _seed;
			opponents = _opponents;
		}

		public bool Met(int opp)
		{
			foreach (int o in opponents)
				if (o == opp)
					return true;
			return false;
		}

	}
}

