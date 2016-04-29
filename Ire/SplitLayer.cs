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
			List<int> Construct = new List<int> ();
            List<int> Filtered = new List<int>();
            foreach (int i in stack)
            {
                bool okayToAdd = true;
                if (i == _Target)
                    okayToAdd = false;
                for (int i2 = 0; i2 < opp.Length; i2++)
                    if (i == opp[i2])
                        okayToAdd = false;
                if (okayToAdd)
                    Filtered.Add(i);
            }
            try
            {
                if (Filtered.Count % 2 == 1)  //even :)
                {
                    //fc+1 = 4  then loop is 1, 2, 3
                    //<1> + actually 2 3 4 (5) 6 7 8
                    //first add the midpoint, then alternate 
                    Construct.Add(Filtered[(Filtered.Count + 1) / 2]);
                    if(Filtered.Count > 1)
                        for (int fc = 1; fc < ((Filtered.Count + 1) / 2); fc++)
                        {
                            Construct.Add(Filtered[fc + ((Filtered.Count + 1) / 2)]);
                            Construct.Add(Filtered[((Filtered.Count + 1) / 2) - fc]);
                        }
                }
                else //odd
                {
                    //fc/2 = 3   loops runs over 0,1,2
                    //<1> 2 3  (4mid)  5 6 7
                    for (int fc = 0; fc < (Filtered.Count / 2); fc++)
                    {
                        Construct.Add(Filtered[fc + (Filtered.Count / 2)]);
                        Construct.Add(Filtered[(Filtered.Count / 2) - 1 - fc]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Offrage of Split Layer");
                Console.WriteLine("FC:"+Filtered.Count / 2 + ":Stack:"+stack.Count);
            }
            return Construct;
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

