using System;
using System.Collections.Generic;

namespace Ire
{
    public class DrawMachine4 //Aka monrad pairing
    {
        #region variables
        private List<Player> plys = new List<Player>(); //this will be all players for convenience of LookUp
        private List<Pairing> Pairs;
        private List<int> lSuggestions = new List<int>();
        private List<MonradLayer> _Monrad;
        private List<Pairing> History = new List<Pairing>(); //previous rounds
        private List<string> Paths = new List<string>();
        private List<int> Registry = new List<int>();
        private int[] lookUpTable;
        private string path = "";
        private int totalPairs = 0;
        private bool Verbose = false;
        #endregion

        public DrawMachine4(List<Player> ply, List<Pairing> _History, int _Rnd,
            int _MaxHandi = 9, int _AdjHandi = 1, bool _HandiAboveBar = false, bool _Verbose = false)
        {
            Verbose = _Verbose;
            plys.AddRange(ply); //Careful with Byes
            History = _History;
            lookUpTable = new int[ply.Count];
            Pairs = new List<Pairing>();
            Pairing.setStatics(_MaxHandi, _AdjHandi, _HandiAboveBar);
            foreach (Player pd in plys)
                if (pd.getParticipation(_Rnd - 1))
                    totalPairs++;
            List<Player> takingABye = new List<Player>();
            foreach (Player pd in plys)
                if (pd.getParticipation(_Rnd - 1) == false) //0 based
                    takingABye.Add(pd);
            foreach (Player tab in takingABye)
                plys.Remove(tab);
            totalPairs = totalPairs / 2;
            //i want to search for Seed and see player 
            for (int i = 0; i < plys.Count; i++)
            {
                lookUpTable[plys[i].Seed - 1] = i;
            }
            //end DO WE NEED
            _Monrad = new List<MonradLayer>();
            //populate Split layers which use Seed and not Deed
            _Monrad.Add(new MonradLayer(plys[0].getMMS(), plys[0].Seed));
            for (int i = 1; i < plys.Count; i++)
            {
                if (plys[i].getMMS() == _Monrad[_Monrad.Count - 1].MMSKey)
                {
                    _Monrad[_Monrad.Count - 1].Add(plys[i].Seed);
                }
                else
                {
                    _Monrad.Add(new MonradLayer(plys[i].getMMS(), plys[i].Seed));
                }
            }
            if (Verbose) DebugMonrad();
            DRAW();
        }

        public void DRAW(int start = 0)
        {
            Player top = plys[start];
            if (Verbose) Console.WriteLine("Calling AdjacentDraw() start: {0}", start);
            if (Verbose) Console.WriteLine("Memory in use: {0}", GC.GetTotalMemory(true));
            bool found = false;
            for (int i = start + 1; i <= plys.Count - 1; i++)
            { //foreachPlayer
                if (Registry.Contains(plys[i].Seed) == true)
                {
                    found = true; //to check - why this is necessary in current code
                }
                else
                    found = false;
                while (found == false)
                {
                    foreach (MonradLayer mcl in _Monrad)
                    { //for each Layer
                        if (found == false)
                        {
                            // NEW LOGIC
                            // request suggestions and browse for valid
                            // if find valid, Eject it
                            // 
                            string test;
                            lSuggestions = mcl.Offer(top.Seed, top.GetOpposition()); //not self, not history
                            foreach (int ls in lSuggestions)
                            {
                                if (Verbose) Console.WriteLine("Suggesting {0}" , ls);
                                test = path + " " + top.Seed + "," + ls;
                                //if not a blocked path AND not a registered suggestion
                                if (Paths.Contains(test) == false && Registry.Contains(ls) == false)
                                {
                                    found = true;
                                    Pairs.Add(new Pairing(top, plys[lookUpTable[ls - 1]]));
                                    path += " " + top.Seed + "," + plys[lookUpTable[ls - 1]].Seed;
                                    mcl.Eject(ls);
                                    for (int ie = 0; ie < _Monrad.Count; ie++) //we had forgotten to eject Top
                                        if (_Monrad[ie].Contains(top.Seed) == true)
                                        {
                                            _Monrad[ie].Eject(top.Seed);
                                            i = _Monrad.Count + 11;
                                        }
                                    if (Pairs.Count == totalPairs)
                                        return; //best way to exit
                                    //Set to true the registered state of top and choice
                                    Registry.Add(top.Seed);
                                    Registry.Add(plys[lookUpTable[ls - 1]].Seed);
                                    //find next top
                                    for (int rp = 0; rp < plys.Count; rp++)
                                        if (Registry.Contains(plys[rp].Seed) == false)
                                        {
                                            top = plys[rp];
                                            i = start; // overkill
                                            rp = plys.Count + 4;
                                        }
                                    if (Verbose) Console.WriteLine("Blocked Path: {0}", path);
                                    if (Verbose) DebugMonrad();
                                    break;
                                }
                            }//end foreach suggestion
                        }
                    }//foreachLayer

                    if (found == false)
                    {
                        if (Pairs.Count == totalPairs) //should be unreachable
                            return;
                        // Console.WriteLine("No valid pairing was found. Retry.");
                        //add to block
                        Console.WriteLine(path);
                        string sp = path;
                        Paths.Add(sp);
                        CleanBlocked(sp);
                        int penultimateSpace = path.LastIndexOf(" ");
                        if (penultimateSpace > 1)
                            path = path.Remove(penultimateSpace);
                        else
                            Console.WriteLine("path cannot be removed as too small");//should be unreachable

                        //reInject (Push) Seeds
                        reInjection(Pairs[Pairs.Count - 1].black.Seed);
                        reInjection(Pairs[Pairs.Count - 1].white.Seed);
                        //update lookups
                        Registry.Remove(Pairs[Pairs.Count - 1].black.Seed);
                        Registry.Remove(Pairs[Pairs.Count - 1].white.Seed);
                        //Purge from Pairs
                        Pairs.RemoveAt(Pairs.Count - 1);

                        for (int rp = 0; rp < plys.Count; rp++)
                            if (Registry.Contains(plys[rp].Seed) == false)
                                DRAW(rp);

                    }
                }

            }//end foreachplayer
        }


        public List<Pairing> GetCurrentPairings()
        {
            return Pairs;
        }

        public void AddPairing(List<Pairing> completedRnd)
        {
            History.AddRange(completedRnd);
        }

        public void reInjection(int blockedSeed)
        {
            foreach (MonradLayer ml in _Monrad)
                if (ml.Contained(blockedSeed))
                {
                    ml.Push(blockedSeed);
                    return;
                }
        }

        public void CleanBlocked(string END)
        {
            List<int> iHold = new List<int>();
            for (int i = 0; i < Paths.Count - 1; i++)
                if (Paths[i].StartsWith(END))
                    iHold.Add(i);
            for (int j = iHold.Count - 1; j > -1; j--)
                Paths.RemoveAt(iHold[j]);
        }

        public void DebugMonrad()
        {
            foreach (MonradLayer ml in _Monrad)
                Console.WriteLine(ml);
            Console.ReadLine();
        }
    }
}


