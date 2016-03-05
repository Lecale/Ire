/*
 * Convenient way to query 
 */ 
using System;
using System.Collections.Generic;

namespace Ire
{
	public class TiebreakBox
	{
		public int Seed;
		private int tRounds;
		private bool SOS;
		private bool MOS;
		private List<int> Opponents;
		private float[] imSOS; //just for debug
		private float[] imMOS;
		private float[] McMahon;

		public TiebreakBox (int _seed, int _rounds, bool _SOS, bool _MOS)
		{
			Seed = _seed;
			tRounds = _rounds;
			SOS = _SOS;
			MOS = _MOS;
			Opponents = new List<int> ();
		}


		public List<int> giveAllOpponents()
		{
			return Opponents;
		}

		public void addOpponent(int op)
		{
			Opponents.Add(op);
		}

		public void eraseRound(int _rnd)
		{
			Opponents.RemoveAt (_rnd - 1);
			imSOS[_rnd - 1] = 0;
			imMOS[_rnd - 1] = 0;
			//reset or not necessary?
		}

		public void takeMcMahon(float[] _McMahon, int _rnd)
		{
			McMahon = _McMahon;
			imSOS [_rnd - 1] = 0; //just incase of funny business
			imMOS[_rnd-1] = 0;
			int missing = 0;
			float MAX = -1;
			float MIN = 99999;
			for (int i = 0; i < McMahon.Length; i++) {
				if (McMahon [i] <= 0)
					missing++;
				else {
					if (MOS && _rnd > 2) { //need MAX and MIN
						if (McMahon [i] < MIN)
							MIN = McMahon [i];
						if (McMahon [i] > MAX)
							MAX = McMahon [i];
					}
					imSOS [_rnd - 1] += McMahon [i];
					imMOS [_rnd - 1] += McMahon [i];
				}
				if (MOS && _rnd > 2) {
					imMOS [_rnd - 1] -= MAX;
					imMOS [_rnd - 1] -= MIN; 
					if (missing > 0)
						imMOS [_rnd - 1] += (missing / (_rnd - 2)) * imMOS [_rnd - 1]; // add m.avg for each missing opponent
				}
				if (SOS) {
					if (missing > 0)
						imSOS [_rnd - 1] += (missing / _rnd ) * imSOS [_rnd - 1]; // add avg for each missing opponent
				}
			}

		}//end

		public float getSOS(int _rnd)
		{
			return imSOS [_rnd - 1];
		}

		public float getMOS(int _rnd)
		{
			return imMOS [_rnd - 1];
		}
	}
}

