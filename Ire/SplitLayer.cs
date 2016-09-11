﻿using System;using System.Collections.Generic;

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
			Console.WriteLine ("HFatal error in SplitLayer:Eject()"); 
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
			if(Filtered.Count <2) // 0 or 1
				return Filtered;
           
            if (Filtered.Count % 2 == 1)  //even :)
            {
				try
				{
                    //fc+1 = 4  then loop is 1, 2, 3
                    //<1> + actually 2 3 4 (5) 6 7 8
                    //first add the midpoint, then alternate 
                    Construct.Add(Filtered[(Filtered.Count ) / 2]);
                    for (int fc = 1; fc < ((Filtered.Count ) / 2); fc++)
                    {
                        Construct.Add(Filtered[fc + ((Filtered.Count ) / 2)]);
                        Construct.Add(Filtered[((Filtered.Count ) / 2) - fc]);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception in EVEN Offrage of Split Layer");
					Console.WriteLine("FC:"+Filtered.Count / 2 + ":Stack:"+stack.Count);
					Console.WriteLine (e.Message);
				}
            }
            else //odd
            {
				try{
                    //fc/2 = 3   loops runs over 0,1,2
                    //<1> 2 3  (4mid)  5 6 7
                    for (int fc = 0; fc < (Filtered.Count / 2); fc++)
                    {
                        Construct.Add(Filtered[fc + (Filtered.Count / 2)]);
						Construct.Add(Filtered[(Filtered.Count / 2) - (1 + fc)]);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception in ODD Offrage of Split Layer");
					Console.WriteLine("FC:"+Filtered.Count / 2 + ":Stack:"+stack.Count);
					Console.WriteLine (e.Message);
				}

              
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

		public bool Contains(int i)
		{
			foreach (int s in stack)
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

