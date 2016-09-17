using System;
using System.Collections.Generic;

namespace Ire
{
	public class MonradLayer : GenericLayer
	{
		public MonradLayer(float MMS, int Seed)
			:base(MMS,Seed)
		{}

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
            for (int j = 0; j < opp.Length; j++ ) 
                if(Construct.Contains(opp[j]))
                    Construct.Remove(opp[j]);
            return Construct;
		}


	}
}

