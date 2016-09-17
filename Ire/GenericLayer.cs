using System;using System.Collections.Generic;

namespace Ire
{
	public class GenericLayer 
	{
		public float MMSKey;
		protected List<int> population;
        protected List<int> stack;

		public GenericLayer (float MMS, int Seed)
		{
			MMSKey = MMS;
            stack = new List<int>();
            population = new List<int> ();
			population.Add (Seed);
            stack.Add(Seed);
		}

		public void Add(int _seed)
		{
			if (population.Contains (_seed) == false) {
				population.Add (_seed);
				stack.Add (_seed);
			}
		}


		/*
		*  Use to re-inject a player
		*/
		public void Push(int _Seed)
		{
			int _SeedOrigPosn = OriginalPositionWas(_Seed);
			int above;
			for (int i = 0; i < stack.Count; i++)
			{
				above = OriginalPositionWas(stack[i]);
				if (_SeedOrigPosn < above)
				{
					stack.Insert(i, _Seed);
					return;
				}
			}
			stack.Add(_Seed);
		}

		private int OriginalPositionWas(int Item)
		{
			for (int i = 0; i < population.Count; i++)
				if (population[i] == Item)
					return i;
			return 99999;
		}

		public int StackSize()
        { return stack.Count;  }

		public bool Contains(int i)
		{
			foreach (int s in stack)
				if (s == i)
					return true;
			return false;
		}
		public bool Contained(int i)
		{
			foreach (int s in population)
				if (s == i)
					return true;
			return false;
		}

		public override string ToString()
		{
			string s = "MMS:" + MMSKey + ":";
			foreach (int i in stack) 
				s = s + i + ",";
			return s;
		}
	}
}

