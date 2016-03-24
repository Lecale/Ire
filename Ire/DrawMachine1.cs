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
        List<Pairing> Blocked = new List<Pairing>();
		private int[] LookUpTable;

		public DrawMachine1 ( List<Player> ply, int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			Console.WriteLine ("DrawMachine1");
			plys = ply;
			LookUpTable = new int[ply.Count];
			Pairs = new List<Pairing>();
			MaxHandi = _MaxHandi;
			AdjHandi = _AdjHandi;
			HandiAboveBar = _HandiAboveBar;
			plys.Sort (); //just in case
			Console.WriteLine ("DrawMachine1 McL");
			Console.WriteLine ("DrawMachine1 plys count " + plys.Count);
			BigM = new List<McLayer>();
			//populate BigM
			BigM.Add (new McLayer (plys [1].getMMS (), plys [1].Seed));
			for(int i=2; i<plys.Count; i++)
			{

				Console.WriteLine ("DrawMachine1 McLoop " + i);
				if (plys [i].getMMS () == BigM [BigM.Count-1].MMSKey) {
					BigM [BigM.Count-1].Add (plys [i].Seed);
				}
				else {
					BigM.Add(new McLayer(plys [i].getMMS (), plys [i].Seed));
				}
			}
			Console.WriteLine ("DrawMachine1 Shuffle");
			//Shuffle
			foreach (McLayer mcl in BigM)
				mcl.Shuffle ();
			for (int j = 0; j < LookUpTable.Length; j++)
				LookUpTable [plys [j].Seed] = j; 
			Console.WriteLine ("DrawMachine1 Draw");
			DRAW ();
		}

		//find top player
		//take layer
		//take player 'at random'
		//if valid pair else retry
		//   Next layer
		//      Nothing? block and retry
		public void DRAW(int start=0)
		{
			Player top = plys [start];
			Pairing tmp;
			Pairing holdLastPairing;
			
			bool found = false;
			for (int i = start+1; i < plys.Count - 1; i++) { //foreachPlayer
				found =false;
				while (found == false) {
					foreach (McLayer mcl in BigM) { //foreachLayer
                        if(found==false)
						for (int j = 0; j < mcl.Length; j++) {
							tmp = new Pairing (plys[mcl.GetAt (j)], top);
                            if (History.Contains(tmp) == false && Blocked.Contains(tmp) == false)
                            {
                                //holdLastPairing = tmp;
                                Pairs.Add(tmp);
                                found = true;
                                break; //out of j
                            }
						}
					}//foreachLayer
                    if (found == false)
                    {
                        Console.WriteLine("No valid pairing was found");
                        //add to block
                        Blocked.Add(Pairs[Pairs.Count - 1]);
                        //rm last pairing added
                        Pairs.RemoveAt(Pairs.Count - 1);
                        //call at what level ?
						DRAW(i-2); 
                    }                              
				}

			}//end foreachplayer
		}

	}
}


