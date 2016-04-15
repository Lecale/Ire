﻿using System;
using System.Collections.Generic;

namespace Ire
{
	public class DrawMachine2 : AbstractDrawMachina//Aka fold pairing
	{
		private List<Player> plys;
		private int MaxHandi;
		private int AdjHandi;
		private bool HandiAboveBar;
		private List<Pairing> Pairs;
		private List<FoldLayer> Fold;
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
            //DO WE NEED THIS LOOK UP TABLE AND DEED
			int d=0;
			foreach (Player pp in plys) {
				LookUpBull[d] = false;
				pp.Deed = d++;
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
            //DO WE STILL NEED THIS
			for (int j = 0; j < LookUpTable.Length; j++)
				LookUpTable [plys [j].Deed] = j; //This is not safe actually!
            
			DRAW ();
		}

		//find top player
		//take layer
		//take player from end
		//if valid pair else retry
		//   Next layer
		//      Nothing? block and retry


		public void DRAW(int start=0)
		{
			Player top = plys [start];
			Pairing tmp;
			Console.WriteLine ("Calling Draw() start:" + start);
			bool found = false;
			for (int i = start+1; i <= plys.Count -1; i++) { //foreachPlayer
				if (LookUpBull [i] == true) {
					found = true;
				}else
					found =false;
				while (found == false) {
					foreach (FoldLayer mcl in Fold) { //foreachLayer
                        if(found==false)
							for (int j = 0; j < mcl.Length; j++) {
                    	            break; //out of j
                        	    
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


