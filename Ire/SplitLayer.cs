using System;using System.Collections.Generic;

namespace Ire
{
	public class SplitLayer
	{
		public float MMSKey;
		private List<int> population;
        private List<int> stack;

		public SplitLayer (float MMS, int Seed)
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

		//use this after Offer
		// stack is available 
		//  population is original
		public int Eject(int Request)
		{
			for (int i = stack.Count-1; i > -1; i--)
				if (stack [i] == Request) {
					stack.RemoveAt (i);
					return Request;
				}
			Console.WriteLine ("Horrid error in FoldLayer:Eject()"); 
			return -1;
		}

		//this is an ordered list of available opponents in the Fold Layer
		//can only return opponents in the Stack (the active list)
		public List<int> Offer(int _Target , int []opp)
		{
			//string dbg = " ";
			List<int> Offrage = new List<int> ();
			return Offrage;
		}

        //Push is only used for re-injection
        public void Push(int _Seed)
        {
            int origin = -1; 
            bool found = false;
            for (int i = population.Count-1; i > -1; i--)
                if (population[i] == _Seed)
                {
                    origin = i;
                    break;
                }
            int nigiro = 99999;
            for (int j = stack.Count-1; j > -1; j--)
            {
                for (int i = population.Count-1; i > -1; i--)
                    if (population[i] == stack[j])
                    {
                        nigiro = i;
                        break;
                    }
                if (nigiro < origin)
                {
                    stack.Insert(j + 1, _Seed);
                    found = true;
                    j = -2;
                }
            }
            if(found==false)
                stack.Add(_Seed);
        }


        public int StackSize()
        { return stack.Count;  }
	}
}

