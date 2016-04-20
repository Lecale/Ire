using System;
using System.Collections.Generic;

namespace Ire
{
	public class DrawMachine2 //Aka fold pairing
	{
		private List<Player> plys; //this will be all players for convenience of LookUp
		private int MaxHandi;
		private int AdjHandi;
		private bool HandiAboveBar;
		private List<Pairing> Pairs;
		private List<FoldLayer> Fold;
		private List<Pairing> History = new List<Pairing>(); //previous rounds
        private List<Pairing> Blocked = new List<Pairing>();
        private List<string> Paths = new List<string>();
		private Pairing lastPair = null;
		private int[] lookUpTable;
		private bool[] lookUpBull;
        private string path = "";
        int totalPairs;

		public DrawMachine2 ( List<Player> ply, List<Pairing> _History, int _Rnd,
			int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false)
		{
			plys = ply;
            History = _History;
			lookUpTable = new int[ply.Count];
		    lookUpBull = new bool[ply.Count];
			Pairs = new List<Pairing>();
			MaxHandi = _MaxHandi;
			AdjHandi = _AdjHandi;
			HandiAboveBar = _HandiAboveBar;
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
                                int suggestion = -1;
                                //request a number 
								if (BLOCK) { //if we were blocked
									int[] hiraki = top.GetOpposition ();
									Array.Resize (ref hiraki, hiraki.Length + 2); //what is this line really doing
									hiraki [hiraki.Length - 1] = lastPair.black.Seed;
									hiraki [hiraki.Length - 2] = lastPair.white.Seed;
									suggestion = mcl.Pop (top.Seed, hiraki);
								}else
                                	suggestion = mcl.Pop(top.Seed, top.GetOpposition()); 
                                if (suggestion != -1)
                                {
                                    Pairs.Add(new Pairing(top,plys[lookUpTable[suggestion]]));
                                    path += " " + top.Deed + "," + plys[lookUpTable[suggestion]].Deed; 
									lastPair = Pairs[Pairs.Count-1];
									BLOCK = false;
                                    found = true; //exit while
                                    if (Pairs.Count == totalPairs)
                                        return; //best way to exit
                                }
							}
					}//foreachLayer

                    //This might be more complex
                    // remove a blocked pair, but when we restart it will be given back as a suggestion automatically?
                    if (found == false)
                    {
                        Console.WriteLine("No valid pairing was found");
                        //add to block
                        Blocked.Add(Pairs[Pairs.Count - 1]);
						//update lookups
						lookUpBull [Pairs [Pairs.Count - 1].black.Seed]=false;
						lookUpBull[Pairs [Pairs.Count - 1].white.Seed]=false;
                        //rm last pairing added
                        Pairs.RemoveAt(Pairs.Count - 1);
						DRAW(i-2,true); //not correct 
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

        public bool ValidPath(string vp)
        {
            foreach (string p in Paths)
            {
                if (p.StartsWith(vp))
                    return false;
            }
            return true;
        }
	}
}


