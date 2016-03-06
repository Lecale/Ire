using System;
using System.Collections.Generic;

namespace Ire
{
	public class DrawMachine1 //Aka simple pairing
	{
		private List<Player> plys;
		private int MaxHandi;
		private int AdjHandi;
		private bool HandiAboveBar;
		private List<Pairing> Pairs;
		private List<McLayer> BigM;
		private List<Pairing> History = new List<Pairing>(); //previous rounds
		private int[] LookUpTable;

		public DrawMachine1 ( List<Player> ply, int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			plys = ply;
			LookUpTable = new int[ply.Count];
			Pairs = new List<Pairing>();
			MaxHandi = _MaxHandi;
			AdjHandi = _AdjHandi;
			HandiAboveBar = _HandiAboveBar;
			plys.Sort (); //just in case
			BigM = new List<McLayer>();
			//populate BigM
			BigM.Add (new McLayer (plys [1].getMMS (), plys [1].Seed));
			for(int i=2; i<plys.Count; i++)
			{
				if (plys [i].getMMS () == BigM [BigM.Count-1].MMSKey) {
					BigM [BigM.Count-1].Add (plys [i].Seed);
				}
				else {
					BigM.Add(new McLayer(plys [i].getMMS (), plys [i].Seed));
				}
			}
			//Shuffle
			foreach (McLayer mcl in BigM)
				mcl.Shuffle ();
			for (int j = 0; j < LookUpTable.Length; j++)
				LookUpTable [plys [j].Seed] = j; 
			DRAW ();
		}

		//find top player
		//take layer
		//take player 'at random'
		//if valid pair else retry
		//   Next layer
		//      Nothing? block and retry
		public void DRAW()
		{
			Player top = plys [0];
			Pairing tmp;
			Pairing holdLastPairing;
			List<Pairing> Blocked = new List<Pairing> ();
			bool found = false;
			for (int i = 1; i < plys.Count - 1; i++) { //foreachPlayer
				found =false;
				while (found == false) {
					foreach (McLayer mcl in BigM) { //foreachLayer
						for (int j = 0; j < mcl.Length; j++) {
							tmp = new Pairing (plys[mcl.GetAt (j)], top);
							if (History.Contains (tmp) == false && Blocked.Contains (tmp) == false)
								found = true;
						}
					}//foreachLayer
				}

			}//end foreachplayer
		}

	}
}


