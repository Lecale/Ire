using System;using System.Collections.Generic;

namespace Ire
{
	public class FoldLayer
	{
		public float MMSKey;
		public int Length = 0;
		private List<int> population;
        private List<int> stack;

		public FoldLayer (float MMS, int Seed)
		{
			MMSKey = MMS;
			population = new List<int> ();
			population.Add (Seed);
            stack.Add(Seed);
		}

		public bool Match(float _mms)
		{
			if (_mms == MMSKey)
				return true;
			return false;
		}

		public void Add(int _seed)
		{
			population.Add(_seed);
            stack.Add(_seed);
		}

		public int Pop(int _Seed, int[] opp)
		{

            return -1;
		}

        //Push is only used for re-injection
        public void Push(int _Seed)
        {
            stack.Insert(0,_Seed);
        }


        public int StackSize()
        { return stack.Count;  }
	}
}

