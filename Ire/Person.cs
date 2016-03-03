using System;

namespace Ire
{
	public class Person
	{
		public string Name;
		public string Clubtry;
		public int Rating;
		public int Seed;
		public string Rank;

		public Person (string _name, string _clubtry, string _rank, int _rating)
		{
			Name = _name;
			Clubtry = _clubtry;
			Rank = _rank;
			Rating = _rating;
		}

		public Person (string _name, string _clubtry, string _rank, int _rating, int _seed)
		{
			Name = _name;
			Clubtry = _clubtry;
			Rank = _rank;
			Rating = _rating;
			Seed = _seed;
		}

		public void SetSeed(int s)
		{
			Seed = s;
		}
	}
}

