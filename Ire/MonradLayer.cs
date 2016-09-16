using System;
using System.Collections.Generic;

namespace Ire
{
	public class MonradLayer
	{
		public float MMSKey;
		private List<int> population;
		private List<int> stack;

		public MonradLayer(float MMS, int Seed)
		{
			MMSKey = MMS;
			stack = new List<int>(); //available or current
			population = new List<int>(); //original
			population.Add(Seed);
			stack.Add(Seed);
		}

		public void Add(int _seed)
		{
			if (population.Contains(_seed) == false)
			{
				population.Add(_seed);
				stack.Add(_seed);
			}
		}

		//use this after Offer
		public int Eject(int Request)
		{
			for (int i = stack.Count - 1; i > -1; i--)
				if (stack[i] == Request)
				{
					stack.RemoveAt(i);
					return Request;
				}
			Console.WriteLine("Horrid error in MonradLayer:Eject()");
			return -1;
		}

		public List<int> Offer(int _Target, int[] opp)
		{
			List<int> Construct = new List<int>();
			for (int i = 0; i < stack.Count; i++)
				if (stack[i] != _Target)
						Construct.Add(stack[i]);
            for (int j = 0; j < opp.Length; j++ ) //clearer logic but maybe slower
                if(Construct.Contains(opp[j]))
                    Construct.Remove(opp[j]);
            return Construct;
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
                if(_SeedOrigPosn < above)
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
		{ return stack.Count; }

        public bool Contains(int i)
        {
            foreach (int s in stack)
                if (s == i)
                    return true;
            return false;
        }
        public bool Contained(int i)
        {
            foreach (int p in population)
                if (p == i)
                    return true;
            return false;
        }

		public override string ToString()
		{
			string s = "MMS:" + MMSKey + ":";
			foreach (int i in stack)
			{
				s += i;
				s += ",";
			}
			return s;
		}
	}
}

