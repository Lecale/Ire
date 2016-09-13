using System;

namespace Ire
{
	public class Person
	{
		public string Name;
		public string Club;
		public string Country;
		public int Rating;
		public int Seed;
		public string Rank;

		public Person (string _name, int _rating, string _club = "BLF", string _country = "IE", string _rank="1k")
		{
			Name = _name;
			Club = _club;
			Country = _country;
			Rank = _rank;
			Rating = _rating;
		}

		public Person (string _name, int _rating, string _club, string _country, string _rank, int _seed)
		{
			Name = _name;
			Club = _club;
			Country = _country;
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

