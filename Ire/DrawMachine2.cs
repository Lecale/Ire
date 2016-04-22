using System;
using System.Collections.Generic;

namespace Ire
{
	public class DrawMachine2 //Aka fold pairing
	{
		#region variables
		private List<Player> plys; //this will be all players for convenience of LookUp
		private List<Pairing> Pairs;
		private List<int> lSuggestions = new List<int>();
		private List<FoldLayer> Fold;
		private List<Pairing> History = new List<Pairing>(); //previous rounds
        private List<string> Paths = new List<string>();
		private int[] lookUpTable;
		private bool[] lookUpBull;
        private string path = "";
        private int totalPairs = -1;
		#endregion

		public DrawMachine2 ( List<Player> ply, List<Pairing> _History, int _Rnd,
			int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			plys = ply;
            History = _History;
			lookUpTable = new int[ply.Count];
		    lookUpBull = new bool[ply.Count];
			Pairs = new List<Pairing>();
			Pairing.setStatics(_MaxHandi, _AdjHandi, _HandiAboveBar);
			plys.Sort (); //just in case
            //Here we use Deed just to construct paths
			int d=0;
            foreach (Player pd in plys)
            {
                if (pd.getParticipation(_Rnd))
                    totalPairs++;
                pd.Deed = d++;
            }
            totalPairs = totalPairs / 2;
            for (int i = 0; i < plys.Count; i++)
            {
                lookUpTable[plys[i].Seed - 1] = i;
                lookUpBull[plys[i].Seed - 1] = false;
            }
            //end DO WE NEED
			Fold = new List<FoldLayer>();
			//populate Fold layers which use Seed and not Deed
			Fold.Add (new FoldLayer (plys [1].getMMS (), plys [1].Seed));
			for(int i=2; i<plys.Count; i++)
			{
				if (plys [i].getMMS () == Fold [Fold.Count-1].MMSKey) {
					Fold [Fold.Count-1].Add (plys [i].Seed);
				}
				else {
					Fold.Add(new FoldLayer(plys [i].getMMS (), plys [i].Seed));
				}
			}
			DRAW ();
		}

        // two things
        // maybe we can add self to the GetOpponent call 
        // if blocked list is populated, we should also add those numbers (only on the first pass)(?)
		public void DRAW(int start=0, bool BLOCK = false)
		{
			Player top = plys [start];
			Pairing tmp;
			Console.WriteLine ("Calling Draw() start:" + start);
			bool found = false;
			for (int i = start+1; i <= plys.Count -1; i++) { //foreachPlayer
				if (lookUpBull [i] == true) {
					found = true;
				}else
					found =false;
				while (found == false) {
					foreach (FoldLayer mcl in Fold) { //foreachLayer
                        if(found==false)
							for (int j = 0; j < mcl.Length; j++) {
                                //request a number 

								// NEW LOGIC
								// request suggestions and browse for valid
								// if find valid, Eject it
								// 
								string test;
								lSuggestions = mcl.Offer(top.Seed,top.GetOpposition()); //not self, not history
								foreach (int ls in lSuggestions) {
									test = path + " " + top.Seed + "," + ls; 
									if (Paths.Contains (test) == false) {
										found = true;
										j = mcl.Length + 1;
										Pairs.Add(new Pairing(top,plys[lookUpTable[ls]]));
										path += " " + top.Seed + "," + plys[lookUpTable[ls]].Seed;
										mcl.Eject (ls);
										if (Pairs.Count == totalPairs)
											return; //best way to exit
										break;
									}
									//else
									// we go to next mcl 
                                }
							}
					}//foreachLayer

                    //This might be more complex
                    // remove a blocked pair, but when we restart it will be given back as a suggestion automatically?
                    if (found == false)
                    {
						if (Pairs.Count == totalPairs) //should be unreachable
							return;
                        Console.WriteLine("No valid pairing was found. Retry.");
                        //add to block
						string sp = path;
						Paths.Add(sp);
						CleanBlocked(sp);
						int penultimateSpace = path.LastIndexOf(" ");
						path = path.Remove(penultimateSpace);

						//update lookups
						lookUpBull [Pairs [Pairs.Count - 1].black.Seed]=false;
						lookUpBull[Pairs [Pairs.Count - 1].white.Seed]=false;
                        Pairs.RemoveAt(Pairs.Count - 1);

						for (int re = 0; re < lookUpBull.Length; re++ )
							if (lookUpBull[re] == false)
							{
								DRAW(re);
							}
						//DRAW(i-2,true); //not correct 
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

        public void CleanBlocked(string END)
        {
            List<int> iHold = new List<int>();
            for (int i = 0; i < Paths.Count - 1; i++)
                if (Paths[i].StartsWith(END))
                    iHold.Add(i);
            //			if(iHold.Count>0)
            //				Console.WriteLine ("cleanBlocked() rm " + iHold.Count);
            for (int j = iHold.Count - 1; j > -1; j--)
                Paths.RemoveAt(iHold[j]);
        }
	}
}


