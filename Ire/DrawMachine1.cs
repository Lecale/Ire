using System;
using System.Collections.Generic;
using System.IO;

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
		private bool[] LookUpBull;
        private List<string> Paths = new List<string>();
        private string path = "";
        private string tryPath = "";
        private int depth = 0;
        private int retry = 0;

		public DrawMachine1 ( List<Player> ply, List<Pairing> _History, 
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
            Pairing.setStatics(MaxHandi, AdjHandi, HandiAboveBar);
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
				LookUpTable [plys [j].Deed] = j; 
//			Console.WriteLine ("DrawMachine1 History.Count:"+History.Count);
			DRAW ();
		}

		public void DRAW(int start=0)
		{
			Player top = plys [start];
			Pairing tmp;
			Console.WriteLine ("Calling Draw() start:" + start + ":depth:" + depth + ":retry:" + retry);
			bool found = false;
			for (int i = start+1; i <= plys.Count -1; i++) { //foreachPlayer
				//Console.WriteLine ("looking at i :" + i);
				if (LookUpBull [i] == true) {
//					Console.WriteLine (i + " had been paired previously");
					found = true;
				}
                else
					found =false;
				while (found == false) {
					foreach (McLayer mcl in BigM) { //foreachLayer
                        if(found==false)
							for (int j = 0; j < mcl.Length; j++) {
								tmp = new Pairing (plys[mcl.GetAt (j)], top);
								//Is this path really accurate?
                                tryPath = path + " " + top.Deed + "," + plys[mcl.GetAt(j)].Deed;
                                if (plys[mcl.GetAt(j)].Deed != top.Deed)
                                if (LookUpBull[plys[mcl.GetAt(j)].Deed] == false)
                        //        if( Paths.Contains(tryPath) == false) //already tried this path
                                if(ValidPath(tryPath)==true)
                                if (History.Contains(tmp) == false) //cannot allow repeat pairing
                                    {
									LookUpBull[top.Deed ] = true;
									LookUpBull[plys[mcl.GetAt (j)].Deed] = true;
                                    path += " " + top.Deed + "," + plys[mcl.GetAt(j)].Deed;
            	                    Pairs.Add(tmp);
                	                found = true;
									top = plys[0]; 
									for (int k = 0; k < LookUpBull.Length; k++) //update top
										if ( LookUpBull[k] == false) {
											top = plys [k];
											k = LookUpBull.Length + 1;
										}
									if ((Pairs.Count + Pairs.Count) == plys.Count) {
//										Console.WriteLine ("All pairings made");
//                                        Console.ReadLine();
                                        found = true;
                                        return;
                                    }
                    	            break; //out of j
                        	    }//end if
							}//end j
					}//foreachLayer
                    if (found == false)
                    {
                        if (Pairs.Count == plys.Count - Pairs.Count)
                            return;
                        Console.WriteLine("No valid pairing was found. Depth" + Pairs.Count + ":Players:"+plys.Count);
						string sp = path;
                        Paths.Add(sp);
//                        Console.WriteLine("Blocked Path:"+path);
						CleanBlocked(sp);
                        int penultimateSpace = path.LastIndexOf(" ");
                        path = path.Remove(penultimateSpace);
                        //Console.WriteLine("newpath:" + path);
                        //Mandatory removal of last pair
						//update lookups
						LookUpBull [Pairs [Pairs.Count - 1].black.Deed]=false;
						LookUpBull [Pairs [Pairs.Count - 1].white.Deed]=false;
                        //rm last pairing added
                        Pairs.RemoveAt(Pairs.Count - 1);
                        //Now we check the path to see if we need to go deeper
//                        Console.WriteLine("Number of Blocked Paths is now " + Paths.Count);
//						foreach (string ppp in Paths)
//							Console.WriteLine (ppp);
//						Console.ReadLine ();
                        
                        //why don't we call at highest false
                        for (int re = 0; re < LookUpBull.Length; re++ )
                            if (LookUpBull[re] == false)
                            {
                                depth = Pairs.Count;
                                retry++;
                                DRAW(re);
                            }
                        //used to be DRAW(i-2);
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
			List<int> iHold = new List<int> ();
			for (int i = 0; i < Paths.Count - 1; i++)
				if(Paths[i].StartsWith(END))
					iHold.Add (i);
//			if(iHold.Count>0)
//				Console.WriteLine ("cleanBlocked() rm " + iHold.Count);
			for(int j=iHold.Count-1; j>-1; j--)
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


        //Do we need to make sure that we retain VALID matches for the last player?
        
	}
}


