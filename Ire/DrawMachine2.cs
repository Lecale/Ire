using System;
using System.Collections.Generic;

namespace Ire
{
	public class DrawMachine2 //Aka fold pairing
	{
		#region variables
		private List<Player> plys = new List<Player>(); //this will be all players for convenience of LookUp
		private List<Pairing> Pairs;
		private List<int> lSuggestions = new List<int>();
		private List<FoldLayer> Fold;
		private List<Pairing> History = new List<Pairing>(); //previous rounds
		private List<string> Paths = new List<string>();
		private List<int> Registry = new List<int>();
		private int[] lookUpTable;
        private string path = "";
        private int totalPairs = 0; 
		#endregion

		public DrawMachine2 ( List<Player> ply, List<Pairing> _History, int _Rnd,
			int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			plys.AddRange(ply); //Careful with Byes
            History = _History;
			lookUpTable = new int[ply.Count];
			Pairs = new List<Pairing>();
			Pairing.setStatics(_MaxHandi, _AdjHandi, _HandiAboveBar);
			plys.Sort (); //just in case
            foreach (Player pd in plys)
                if (pd.getParticipation(_Rnd-1))
                    totalPairs++;
			List<Player> takingABye = new List<Player> ();
			foreach (Player pd in plys)
				if (pd.getParticipation(_Rnd-1) == false) //0 based
					takingABye.Add(pd); 
			foreach (Player tab in takingABye)
				plys.Remove (tab);
            totalPairs = totalPairs / 2;
			//i want to search for Seed and see player 
            for (int i = 0; i < plys.Count; i++)
            {
				lookUpTable[plys[i].Seed - 1] = i;
            }
            //end DO WE NEED
			Fold = new List<FoldLayer>();
			//populate Fold layers which use Seed and not Deed
			Fold.Add (new FoldLayer (plys [0].getMMS (), plys [0].Seed));
			for(int i=1; i<plys.Count; i++)
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

		public void DRAW(int start=0)
		{
			Player top = plys [start];
			Console.WriteLine ("Calling FoldDraw() start:" + start);
			bool found = false;
			for (int i = start+1; i <= plys.Count -1; i++) { //foreachPlayer
				if (Registry.Contains(plys [i].Seed) == true) {
					found = true; //should be unreachable
				}else
					found =false;
				while (found == false) {
					foreach (FoldLayer mcl in Fold) { //for each Layer
                        if (found == false) { 
								// NEW LOGIC
								// request suggestions and browse for valid
								// if find valid, Eject it
								// 
								string test;
								lSuggestions = mcl.Offer(top.Seed,top.GetOpposition()); //not self, not history
                                foreach (int ls in lSuggestions) {
									test = path + " " + top.Seed + "," + ls; 
								//if not a blocked path AND not a registered suggestion
								if (Paths.Contains (test) == false && Registry.Contains(ls)==false) { 
									found = true;
									Pairs.Add(new Pairing(top,plys[lookUpTable[ls-1]]));
									path += " " + top.Seed + "," + plys[lookUpTable[ls-1]].Seed;
									mcl.Eject (ls);
									if (Pairs.Count == totalPairs)
										return; //best way to exit
									//Set to true the registered state of top and choice
									Registry.Add(top.Seed);
									Registry.Add (plys [lookUpTable [ls-1]].Seed);
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

                    if (found == false)
                    {
						if (Pairs.Count == totalPairs) //should be unreachable
							return;
                     //   Console.WriteLine("No valid pairing was found. Retry.");
                        //add to block
						string sp = path;
						Paths.Add(sp);
						CleanBlocked(sp);
						int penultimateSpace = path.LastIndexOf(" ");
                        if (penultimateSpace > 1)
                            path = path.Remove(penultimateSpace);
                        else
							Console.WriteLine("path cannot be removed as too small");//should be unreachable

						//update lookups
						Registry.Remove (Pairs [Pairs.Count - 1].black.Seed);
						Registry.Remove (Pairs[Pairs.Count - 1].white.Seed);
                        Pairs.RemoveAt(Pairs.Count - 1);

						for (int rp = 0; rp < plys.Count; rp++)
							if (Registry.Contains (plys[rp].Seed) == false) {
								DRAW(rp);
							}
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


