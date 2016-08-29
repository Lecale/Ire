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
			stack = new List<int>();
			population = new List<int>();
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
			Console.WriteLine("Horrid error in MonradLayer:Eject()");
			return -1;
		}

		public List<int> Offer(int _Target, int[] opp)
		{

			List<int> Construct = new List<int>();
			for (int i = 0; i < stack.Count; i++)
				if (stack[i] != _Target)
					Construct.Add(stack[i]);
			return Construct;
		}

		/*
		 *  Use to re-inject a player
		 */
		public void Push(int _Seed)
		{
			int origin = -1;
			bool found = false;
			for (int i = population.Count - 1; i > -1; i--)
				if (population[i] == _Seed)
				{
					origin = i;
					break;
				}
			int nigiro = 99999;
			for (int j = stack.Count - 1; j > -1; j--)
			{
				for (int i = population.Count - 1; i > -1; i--)
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
			if (found == false)
				stack.Add(_Seed);
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

