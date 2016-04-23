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
		private List<int> Registry = new List<int>();
		private int[] lookUpTable;
		private bool[] lookUpBull;
        private string path = "";
        private int totalPairs = -1;
		#endregion

		public DrawMachine2 ( List<Player> ply, List<Pairing> _History, int _Rnd,
			int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			plys = ply; //We forgot to handle Byes
            History = _History;
			lookUpTable = new int[ply.Count];
		    lookUpBull = new bool[ply.Count];
			Pairs = new List<Pairing>();
			Pairing.setStatics(_MaxHandi, _AdjHandi, _HandiAboveBar);
			plys.Sort (); //just in case
            foreach (Player pd in plys)
            {
                if (pd.getParticipation(_Rnd))
                    totalPairs++;
            }
            foreach (Player pd in plys)
            {
                if (pd.getParticipation(_Rnd) == false)
                {
                    Console.WriteLine("rm a player with a bye");
                    plys.Remove(pd);
                }
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
            Console.WriteLine("FoldLayers.Count:" + Fold.Count);
			DRAW ();
		}

        // two things
        // maybe we can add self to the GetOpponent call 
        // if blocked list is populated, we should also add those numbers (only on the first pass)(?)
		public void DRAW(int start=0)
		{
			Player top = plys [start];
			Console.WriteLine ("Calling Draw() start:" + start);
			bool found = false;
			for (int i = start+1; i <= plys.Count -1; i++) { //foreachPlayer
				if (Registry.Contains(plys [i].Seed) == true) {
					found = true; //should be unreachable
				}else
					found =false;
				while (found == false) {
					foreach (FoldLayer mcl in Fold) { //foreachLayer
                        if (found == false) { 
								// NEW LOGIC
								// request suggestions and browse for valid
								// if find valid, Eject it
								// 
								string test;
								lSuggestions = mcl.Offer(top.Seed,top.GetOpposition()); //not self, not history
                                Console.WriteLine(":n.sug:"+lSuggestions.Count);
                                foreach (int ls in lSuggestions) {
									test = path + " " + top.Seed + "," + ls; 
								//if not a blocked path AND not a registered suggestion
								if (Paths.Contains (test) == false && Registry.Contains(ls)==false) { 
										found = true;
										//j = mcl.Length + 1;
										Pairs.Add(new Pairing(top,plys[lookUpTable[ls]]));
										path += " " + top.Seed + "," + plys[lookUpTable[ls]].Seed;
                                        Console.WriteLine("pathupdate"+path);
										mcl.Eject (ls);
										if (Pairs.Count == totalPairs)
											return; //best way to exit
									//Set to true the registered state of top and choice
									Registry.Add(top.Seed);
									Registry.Add (plys [lookUpTable [ls]].Seed);
									//find next top
									for (int rp = 0; rp < plys.Count; rp++)
										if (Registry.Contains (plys[rp].Seed) == false) {
											top = plys [rp];
											rp = plys.Count + 4;
										}
									break;
									} 
                                }//end foreach suggestion
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
                        if (penultimateSpace > 1)
                            path = path.Remove(penultimateSpace);
                        else
                            Console.WriteLine("path cannot be removed as too small");

						//update lookups
						Registry.Remove (Pairs [Pairs.Count - 1].black.Seed);
						Registry.Remove (Pairs[Pairs.Count - 1].white.Seed);
                        Pairs.RemoveAt(Pairs.Count - 1);

						for (int rp = 0; rp < plys.Count; rp++)
							if (Registry.Contains (plys[rp].Seed) == false) {
								DRAW(rp);
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


