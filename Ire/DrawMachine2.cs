using System;
using System.Collections.Generic;

namespace Ire
{
	public class DrawMachine2 : AbstractDrawMachina//Aka simple pairing
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
		private bool[] LookUpBull;

		public DrawMachine2 ( List<Player> ply, List<Pairing> _History, 
			int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			plys = ply;
            History = _History;
			LookUpTable = new int[ply.Count];
			LookUpBull = new bool[ply.Count];
			Pairs = new List<Pairing>();
			MaxHandi = _MaxHandi;
			AdjHandi = _AdjHandi;
			HandiAboveBar = _HandiAboveBar;
			plys.Sort (); //just in case
			int d=0;
			foreach (Player pp in plys) {
				LookUpBull[d] = false;
				pp.Deed = d++;
			}
			BigM = new List<McLayer>();
			//populate BigM
			BigM.Add (new McLayer (plys [1].getMMS (), plys [1].Deed));
			for(int i=2; i<plys.Count; i++)
			{
				if (plys [i].getMMS () == BigM [BigM.Count-1].MMSKey) {
					BigM [BigM.Count-1].Add (plys [i].Deed);
				}
				else {
					BigM.Add(new McLayer(plys [i].getMMS (), plys [i].Deed));
				}
			}
			//Shuffle
			foreach (McLayer mcl in BigM)
				mcl.Shuffle ();
			for (int j = 0; j < LookUpTable.Length; j++)
				LookUpTable [plys [j].Deed] = j; //This is not safe actually!
		//	Console.WriteLine ("DrawMachine1 Draw");
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
			Console.WriteLine ("Calling Draw() start:" + start);
			bool found = false;
			for (int i = start+1; i <= plys.Count -1; i++) { //foreachPlayer
				// but this is not the good loop?
				//Console.WriteLine ("looking at i :" + i);
				if (LookUpBull [i] == true) {
					//Console.WriteLine (i + " had been paired previously");
					found = true;
				}else
					found =false;
				while (found == false) {
					foreach (McLayer mcl in BigM) { //foreachLayer
                        if(found==false)
							for (int j = 0; j < mcl.Length; j++) {
								//Console.WriteLine ("  trying at j :" + j);
								tmp = new Pairing (plys[mcl.GetAt (j)], top); //not correct?
								//how to set above line well
								if (History.Contains(tmp) == false && Blocked.Contains(tmp) == false 
									&& LookUpBull[plys[mcl.GetAt (j)].Deed] ==false && plys[mcl.GetAt (j)].Deed != top.Deed)
        	                    {
									LookUpBull[top.Deed ] = true;
									LookUpBull[plys[mcl.GetAt (j)].Deed] = true;
            	                    Pairs.Add(tmp);
                	                found = true;
									top = plys[0]; 
									for (int k = 0; k < LookUpBull.Length; k++)
										if ( LookUpBull[k] == false) {
											top = plys [k];
											k = LookUpBull.Length + 1;
										}
                    	            break; //out of j
                        	    }
							}
					}//foreachLayer
                    if (found == false)
                    {
                        Console.WriteLine("No valid pairing was found");
                        //add to block
                        Blocked.Add(Pairs[Pairs.Count - 1]);
						//update lookups
						LookUpBull [Pairs [Pairs.Count - 1].black.Deed]=false;
						LookUpBull[Pairs [Pairs.Count - 1].white.Deed]=false;
                        //rm last pairing added
                        Pairs.RemoveAt(Pairs.Count - 1);
                        //call at what level ?
						DRAW(i-2); 
                    }                              
				}

			}//end foreachplayer
		}


		public List<Pairing> GetCurrentPairings()
		{
			return Pairs;
		}

		public void AddPairing (List<Pairing> completedRnd)
		{
			History.AddRange (completedRnd);
		}
	}
}


