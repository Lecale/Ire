using System;using System.Collections.Generic;

namespace Ire
{
	public class SplitLayer : GenericLayer
	{
		public SplitLayer(float MMS, int Seed)
			: base(MMS, Seed)
		{ }

		//use this after Offer
		// stack is available 
		//  population is original
		public int Eject(int Request)
		{
			for (int i = stack.Count - 1; i > -1; i--)
				if (stack[i] == Request)
				{
					stack.RemoveAt(i);
					return Request;
				}
			Console.WriteLine("Fatal error in SplitLayer:Eject()");
			return -1;
		}

		//this is an ordered list of available opponents in the Fold Layer
		//can only return opponents in the Stack (the active list)
		public List<int> Offer(int _Target, int[] opp)
		{
			//string dbg = " ";
			List<int> Construct = new List<int>();
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
			if (Filtered.Count < 2) // 0 or 1
				return Filtered;

			if (Filtered.Count % 2 == 1)  //even :)
			{
				try
				{
					//fc+1 = 4  then loop is 1, 2, 3
					//<1> + actually 2 3 4 (5) 6 7 8
					//first add the midpoint, then alternate 
					Construct.Add(Filtered[(Filtered.Count) / 2]);
					for (int fc = 1; fc < ((Filtered.Count) / 2); fc++)
					{
						Construct.Add(Filtered[fc + ((Filtered.Count) / 2)]);
						Construct.Add(Filtered[((Filtered.Count) / 2) - fc]);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception in EVEN Offrage of Split Layer");
					Console.WriteLine("FC:" + Filtered.Count / 2 + ":Stack:" + stack.Count);
					Console.WriteLine(e.Message);
				}
			}
			else //odd
			{
				try
				{
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
					Console.WriteLine("FC:" + Filtered.Count / 2 + ":Stack:" + stack.Count);
					Console.WriteLine(e.Message);
				}

			}
			return Construct;
		}
	}
}
