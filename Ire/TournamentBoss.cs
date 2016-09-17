using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ire
{ 
    public class TournamentBoss
    {
		#region variables
        public int nRounds = 0;
        public int currentRound = 0;
        string exeDirectory;
		string TournamentName;
        private string workDirectory;
        bool Macintosh = false;
		bool TopBar=false;
		bool RatingFloor=false;
        bool HandiAboveBar = false;
        bool Verbose = false;
		int HandiAdjust=1;
		int nMaxHandicap = 9;
		int nTopBar = 5000;
		int nRatingFloor = 100;
		int nGradeWidth = 100; //to take from Settings
		string PairingStrategy = "Simple";
        List<Player> AllPlayers = new List<Player>();
		List<Player> RoundPlayers;
		List<Pairing> AllPairings = new List<Pairing> ();
		List<Pairing> RoundPairings = new List<Pairing> ();
        List<string> Tiebreakers = new List<string>(); //to take from Settings
		#endregion

		public TournamentBoss(bool Mac=false)
        {
			Macintosh = Mac;
            exeDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(exeDirectory);
        }

		#region NormalOperations
		public void SortField(bool init=false)
		{
			if (init)
				Player.SortByRating = true;
			AllPlayers.Sort ();
			int i = 1;
			if (init) { //late players get a different Seed because they were late and that is how it goes
				//top group but for bye will then be seeded as if in top group
				//this should not have any negative effect on the pairings though
				Player.SortByRating = false;
				foreach (Player p in AllPlayers) 
					p.Seed = i++;
			}
		}

		public void MakeDraw(int currentRound = 1)
		{
            Console.Clear();
			Console.WriteLine ("We are ready to make the draw for Round "+currentRound);
			if (currentRound > 1) {
				Console.WriteLine ("Do you want to add a new player to the players list (yes / no)");
				string s = Console.ReadLine ();
				if (s.ToUpper ().Trim ().StartsWith ("Y")) {
					HandleLatePlayers (currentRound);
				}
				Console.WriteLine ("Do you want to update player participation (byes) in the players list (yes / no)");
			 s = Console.ReadLine ();
				if (s.ToUpper ().Trim ().StartsWith ("Y")) {
                    Console.WriteLine("After updating the players file press return to continue");
                    string anykey = Console.ReadLine();
					ReadByesFromFile (currentRound);
				}
			}
			int i = -1;
			//Handle this later
			UpdateParticipation (currentRound);
			if (currentRound == 1)
				InitMMS ();
			else
				SortField ();
			if (RoundPlayers.Count % 2 == 1) {
				i = AssignBye (currentRound);
				while (i == -1) {
					int retry = 2;
					Console.WriteLine ("No player was found who already had less than " + (retry - 1) + "bye");
					if (currentRound <= retry)
						i = AssignBye (currentRound, retry++);
					else {
						i = 100;
						Console.WriteLine ("Fatal error encountered. Tournament cannot proceed");
					}
				}
			}
            List<Pairing> RndPairings = new List<Pairing>();
			Console.WriteLine ("The draw is being made ...");
            if (PairingStrategy.ToUpper().Equals("SIMPLE"))
            {
				DrawMachine1 dm1 = new DrawMachine1(RoundPlayers, AllPairings, nMaxHandicap, HandiAdjust, HandiAboveBar, Verbose);
                RndPairings = dm1.GetCurrentPairings();
            }
            if (PairingStrategy.ToUpper().Equals("FOLD"))
            {
				DrawMachine2 dm2 = new DrawMachine2(AllPlayers, AllPairings, currentRound, nMaxHandicap, HandiAdjust, HandiAboveBar, Verbose);
                RndPairings = dm2.GetCurrentPairings();
            }
            if (PairingStrategy.ToUpper().Equals("SPLIT"))
            {
                DrawMachine3 dm3 = new DrawMachine3(AllPlayers, AllPairings, currentRound, nMaxHandicap, HandiAdjust, HandiAboveBar, Verbose);
                RndPairings = dm3.GetCurrentPairings();
            }
            if (PairingStrategy.ToUpper().Equals("ADJACENT"))
            {
                DrawMachine4 dm4 = new DrawMachine4(AllPlayers, AllPairings, currentRound, nMaxHandicap, HandiAdjust, HandiAboveBar, Verbose);
                RndPairings = dm4.GetCurrentPairings();
            }
			if(Verbose)
				foreach (Pairing rp in RndPairings)
                	Console.WriteLine(rp.BasicOutput());
            GenerateRoundResultsFile(currentRound, RndPairings);
            Console.WriteLine();
			Console.WriteLine ("The draw is available at Round"+currentRound+"Results.txt");
			Console.WriteLine("Remember that the draw can be overwritten in the input file");
            Console.WriteLine("When you are ready to read in the completed results, press return");
            string anyKey = Console.ReadLine();
            if (anyKey.ToUpper().Equals("AUTO"))
            {
                Console.WriteLine("Result autogeneration was selected");
                GenerateResultsForRound(currentRound);
            }
        }

		public void InitMMS() 
		{
			SortField (true);
			foreach (Player ap in AllPlayers) {
				int gap = nTopBar - ap.getERating ();
				gap = gap / nGradeWidth;
				if (gap >= 0 && ap.topBar == false)
					gap++;
                ap.setMMS(100 - gap); 
                ap.setInitMMS(100 - gap);
			}

		}

		public void UpdateParticipation(int _rnd)
		{
			RoundPlayers = new List<Player> ();
			foreach (Player p in AllPlayers) {
				if (p.getParticipation (_rnd - 1))
					RoundPlayers.Add (p);
				else
					p.AssignBye (_rnd);
			}
			Console.WriteLine ("The number of players competing in round " + _rnd + " is " + RoundPlayers.Count);
		}

		public int AssignBye(int _rnd, int ByeLevel=1)
		{ 
			for (int i = RoundPlayers.Count - 1; i >-1; i--) {
				//Console.WriteLine (i);
				if (RoundPlayers [i].nBye() < ByeLevel) {
					Console.WriteLine ("A bye will be assigned to ...");
					Console.WriteLine (RoundPlayers [i].ToString ());
					RoundPlayers [i].AssignBye (_rnd);
					RoundPlayers.RemoveAt (i);
					return i;
				}
			}
			//emergency
			Console.WriteLine("Strangely, no Candidate for the Bye was found...");
			return -1;
		}

		//It should be possible to reverse Byes using this method
		public void ProcessResults(int rnd)
        {
            if (rnd == 1)
                GenerateStore();
			Console.WriteLine ("Processing Results: rnd " + rnd + " pairings " + RoundPairings.Count);
			//lookuptable
			foreach (Pairing p in RoundPairings) {
				if(p.white.getParticipation(rnd-1)==false) 
					Console.WriteLine("The following was originally assigned a bye:" + p.white.getName());
				if(p.black.getParticipation(rnd-1)==false) 
					Console.WriteLine("The following was originally assigned a bye:" + p.black.getName());
				p.white.setResult (rnd, p.black.Seed, p.WhiteScore (), p.GetHandi (), 1);
				p.black.setResult (rnd, p.white.Seed, p.BlackScore (), p.GetHandi (), 0);
				if(Verbose)	Console.WriteLine("W" + p.white.Seed + ": B " + p.black.Seed);
			}
			//now check for maladjusted bye
			foreach (Player ap in AllPlayers) {
                if (ap.getOpponent(rnd - 1) == 0)
                {
                    if (ap.getResult(rnd - 1) != 0.5)  
                    {
                        Console.WriteLine("Assigning a bye to " + ap.getName());
                        ap.AssignBye(rnd);//AssignBye adjusts the rnd number
                    }
                }
			}
		}

        //Should have some if SOS if MOS if SODOS logic
        public void UpdateTiebreaks(int rnd)
        {
            int[] lookUp = new int[AllPlayers.Count]; //yuck
            for (int i = 0; i < AllPlayers.Count; i++)
                lookUp[AllPlayers[i].Seed - 1] = i;
            foreach (Player ap in AllPlayers)
            {
                float _SOS = 0;
                float _MOS = 0; //do not calculate if rnd <3
                float _SODOS = 0;
                float maxSOS = -999;
                float minSOS = 999;
                //find actual games played from participation
                int nGame = 0;
				int dbg = -1;
                for (int i = 0; i < rnd; i++)
                {
					dbg = i;
                    try
                    {
                        if (ap.getParticipation(i) == true)
                        {
                            nGame++;
                            int op = ap.getOpponent(i) -1; 
							float f = AllPlayers[lookUp[op]].getMMS() ;
							f = f + ap.getAdjHandi(i);
                            _SOS += f;
                            _SODOS += (f * ap.getResult(i)); 
                            if (f > maxSOS)
                                maxSOS = f;
                            if (f < minSOS)
                                minSOS = f;
                        }
                        else
                        {
							if(Verbose) Console.WriteLine("a bye detected for " +ap.Seed + " in round "+i);
                        }
                    }
                    catch (Exception e) 
                    {
						Console.WriteLine("rnd:" + dbg + " " + e.Message);
                        Console.WriteLine("Seed was " + ap.Seed);
                        for (int ie = 0; ie < rnd; ie++ )
                            Console.WriteLine("Opponent " + ie + " was " + ap.getOpponent(ie));
                    }
                }
				float _score = ap.getScore (rnd);

				if (nGame == 0)
					_SOS = ap.getMMS() * rnd; //used to be initMMS();
                if (nGame < rnd)
                {
					if (nGame != 0) {
						_SOS = _SOS * rnd / nGame;
						if (_score != 0.5f*(float)(rnd - nGame) ) 
							_SODOS = _SODOS * (_score / (_score - (0.5f * (float)(rnd - nGame))));
						else
							_SODOS = 0.5f * _SOS / (rnd); // assign half mean SOS
					} else {
						_SODOS = 0.5f * _SOS / (rnd);
					}
                }

                
				if (rnd > 2)
					_MOS = _SOS - maxSOS - minSOS;
				else
					_MOS = _SOS; //maybe this is normal ??
                ap.SOS = _SOS;
                ap.MOS = _MOS;
                ap.SODOS = _SODOS;
				if (_score == 0 || _SODOS == 0)
                    ap.MDOS = 0;
                else{
                    ap.MDOS = _SODOS / _score;
                }
                    
            }
			FirstRatingDerivative(rnd);
			SecondRatingDerivative(rnd);
        }
        #region OPERA
        public void FirstRatingDerivative(int rnd)
        {
            //In this method we will use LINQ to make the query, just to practice using LINQ
            Rater Europa = new Rater();
            float[] oppRatings = new float[rnd];
            float[] theResults = new float[rnd];
            int[] oppSeeds = new int[rnd];
            foreach (Player p in AllPlayers)
            {
                for (int i = 0; i < rnd; i++)
                {
                    oppSeeds[i] = p.getOpponent(i);
                    theResults[i] = p.getResult(i);
                    //we need to adjust for handi here! if we are white!
                    //The rating needs to be increased by the number of handi we give.
                    // if we are black (da da da)
                    var linkquery = from matchPlayer in AllPlayers
                                    where matchPlayer.Seed == oppSeeds[i]
//                                    select new { linkRating = matchPlayer.Rating };
                                    select new { linkRating = matchPlayer.getERating() }; //Maybe fairer in top group?
                    foreach (var lq in linkquery)
                    {
                        oppRatings[i] = lq.linkRating;
                    }
                    if (p.getAdjHandi(i) != 0)
                        oppRatings[i] += 100 * p.getAdjHandi(i);
                }
                p.firstRating = Europa.ObtainNewRating(oppRatings, p.getERating(), theResults);
            }
        }
        public void SecondRatingDerivative(int rnd)
		{
            Rater Europa = new Rater();
            float[] oppRatings = new float[rnd];
            float[] theResults = new float[rnd];
            int[] oppSeeds = new int[rnd];
            foreach (Player p in AllPlayers)
            {
                for (int i = 0; i < rnd; i++)
                {
                    oppSeeds[i] = p.getOpponent(i);
                    theResults[i] = p.getResult(i);
                    var linkquery = from matchPlayer in AllPlayers
                                    where matchPlayer.Seed == oppSeeds[i]
                                    select new { linkRating = matchPlayer.firstRating };
                    foreach (var lq in linkquery)
                        oppRatings[i] = lq.linkRating;
                    if (p.getAdjHandi(i) != 0)
                        oppRatings[i] += 100 * p.getAdjHandi(i);
                }
				p.OPERA = (int) Europa.ObtainNewRating(oppRatings, p.firstRating, theResults);

            }
        }

        #endregion

        public void HandleLatePlayers(int rnd)
		{
			Console.WriteLine ("Late entrants should be added to the file players.txt");
			Console.WriteLine ("When ready, press return to proceed");
			string s = Console.ReadLine ();
			int before = AllPlayers.Count;
			ReadPlayers(false); //later true
			int after = AllPlayers.Count;
			//init mms
			//give byes
            for (int i = after; i > before; i--)
            {
                AllPlayers[i - 1].SetSeed(i);
				AllPlayers [i - 1].topBar = false; //should already be false?
                if(TopBar)
                    if (AllPlayers[i - 1].getERating() > nTopBar)
                    {
                        AllPlayers[i - 1].setERating(nTopBar+1);
                    }
                if(RatingFloor)
    				if (AllPlayers [i - 1].getERating () < nRatingFloor)
	    				AllPlayers [i - 1].setERating (nRatingFloor);
                //set initial mms
                int gap = nTopBar - AllPlayers[i-1].getERating();
                gap = gap / nGradeWidth;
                if (gap >= 0 && AllPlayers[i - 1].topBar == false)
                    gap++;
                AllPlayers[i - 1].setInitMMS(100 - gap);

                //assign bye
				for (int j = 1; j < rnd; j++) 
					AllPlayers [i - 1].AssignBye (j);
				string hlpDebug = "";
				for (int k = 0; k < nRounds; k++)
					if (AllPlayers [i - 1].getParticipation (k))
						hlpDebug += (k + 1) + " ";
				Console.WriteLine (AllPlayers [i - 1].ToString () + " plays in "+ hlpDebug);
            }
			GenerateStore (); //Else seeding is not recorded and a bug appears
			SortField ();
		}
		#endregion

		#region TestPreviewFunctions
		public void GeneratePlayers(int nPlayers=17, int midpoint=1500, int spread=500 )
		{
            Console.WriteLine("Generate dummy players? ( yes / no)");
            string s = Console.ReadLine();
            if (s.ToUpper().StartsWith("Y"))
            {
                Console.WriteLine("Enter 3 comma separated params (nPlayers,Midpoint,Spread)");
                string auto = Console.ReadLine();
                string[] split = auto.Split(new char[] {','});
                if (split.Length == 3)
                {
                    nPlayers = int.Parse(split[0]);
                   // rnd = int.Parse(split[1]);
                    midpoint = int.Parse(split[1]);
                    spread = int.Parse(split[2]);
                    Utility u = new Utility();
                    string end = "";
                    string fn = workDirectory + "players.txt";
                    int i = 999100; //fake EGD pin
                    using (StreamWriter sw = new StreamWriter(fn, true))
                    {
                        for (int np = 0; np < nPlayers; np++)
                            sw.WriteLine(i++ + "," + u.ProvideName() + "," + u.RatingByBox(midpoint, spread) + ",BLF,IE,1k" + end);
                    }
                }
                else
                    Console.WriteLine("Data entry error, players were not autogenerated");
            }
		}
		public void ShowField(string info="")
		{
			using(StreamWriter sw = new StreamWriter(workDirectory +  "dbg.txt",true))
				{
                    sw.WriteLine(info);
				foreach(Player p in AllPlayers )
					sw.WriteLine(p.ToDebug());			
				}
		}
		public void previewFloor(bool SetFloor =  false)
		{
            if (RatingFloor)
            {
                int tCount = 0;
                foreach (Player p in AllPlayers)
                {
                    if (p.getRating() < nRatingFloor)
                    {
                        Console.WriteLine(p.getName() + " " + p.getRating());
                        tCount++;
                    }
                }
                Console.WriteLine("Provisionally " + tCount + " in bottom group");
                if (SetFloor)
                {
                    Console.WriteLine("Apply this setting (yes / no )");
                    if (Console.ReadLine().ToUpper().StartsWith("Y"))
                    {
                        foreach (Player pete in AllPlayers)
                        {
                            if (nRatingFloor > pete.getRating())
                            {
                                pete.setERating(nRatingFloor);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Enter a new value for the Rating Floor");
                        nRatingFloor = int.Parse(Console.ReadLine().Trim());
                        previewFloor(true);
                    }
                }
            }
		}
		public void previewTopBar(bool SetBar = false)
		{
            if (TopBar)
            {
                int tCount = 0;
                foreach (Player peter in AllPlayers)
                {
                    if (peter.getRating() > nTopBar && peter.nBye() == 0)
                    {
                        Console.WriteLine(peter.getName() + " " + peter.getRating());
                        tCount++;
                    }
                }
                Console.WriteLine("Provisionally " + tCount + " in top group");
                if (SetBar)
                {
                    Console.WriteLine("Apply this setting (yes / no )");
                    if (Console.ReadLine().ToUpper().StartsWith("Y"))
                    {
                        foreach (Player pete in AllPlayers)
                        {
                            if (nTopBar < pete.getRating() && pete.nBye() == 0)
                            {
                                pete.topBar = true;
                                pete.setERating(nTopBar+1);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Enter a new value for the Rating of the Top Bar");
                        nTopBar = int.Parse(Console.ReadLine().Trim());
                        previewTopBar(true);
                    }
                }
            }
		}
        public void GenerateResultsForRound(int rnd)
        { 
            Utility u = new Utility();
            u.EnterResults(workDirectory, rnd);
        }
		#endregion

		#region ImportFunctions
        public void DownloadEGFMasterZip()
		{
			Console.WriteLine ("Downloading data file allworld_lp.zip ...");
			string uri = "http://www.europeangodatabase.eu/EGD/EGD_2_0/downloads/allworld_lp.zip";
			if( File.Exists(workDirectory + "egzipdata.zip"))
				File.Delete(workDirectory + "egzipdata.zip");
			if( File.Exists(workDirectory + "allworld_lp.html"))
				File.Delete(workDirectory + "allworld_lp.html");
			FileInfo fi;
            WebClient client = new WebClient ();
			client.DownloadFile (uri, workDirectory + "egzipdata.zip");
				Console.WriteLine ("File downloaded");
		    fi = new FileInfo (workDirectory + "egzipdata.zip");
            using (ZipFile zip = ZipFile.Read(fi.FullName))
            {
                try { zip.ExtractAll(workDirectory); }
                catch (Exception e) { Console.WriteLine(e.Message);  }
            }
            FormatMasterEGF();
        }

        public void RefreshPlayers()
        {
            Console.WriteLine("There is a 5 second delay between each call to the EGD");
            string tLn = "";
            string fin = workDirectory + "players.txt";
            WebClient wc = new WebClient();
            string[] tmp;
            char[] ctsv = { ',', '\t' };
            string pin;
            int newPin =-1;
            string baseURL = "http://www.europeangodatabase.eu/EGD/GetPlayerDataByPIN.php?pin=";
            List<string> saveHDR = new List<string>();
            List<string> storeP = new List<string>();
            using (StreamReader reader = new StreamReader(fin))
            {
                for (int i = 0; i < 6; i++)                {
                    tLn = reader.ReadLine(); //trip through headers
                    saveHDR.Add(tLn);
                }
                while ((tLn = reader.ReadLine()) != null)                {
                    tmp = tLn.Split(ctsv);
                    pin = tmp[0];
                    if (pin.Length > 1)
                    {
                        if (pin.Length < 8)
                        {
                            storeP.Add(tLn); //fake EGD pin
                        }
                        else
                        {
                            try
                            {
                                newPin = regor(wc.DownloadString(baseURL + pin));
                                Thread.Sleep(5000); //be nice to the EGD
                            }
                            catch (Exception e) { newPin = -1; }
                            if (newPin == -1)
                                storeP.Add(tLn);
                            else
                            {
                                if(Verbose)
                                    Console.WriteLine("Rating updated from: " + tmp[2] + " to " + newPin);
                                if (tLn.Contains("\t" + tmp[2] + "\t"))
                                    tLn = tLn.Replace("\t" + tmp[2] + "\t", "\t" + newPin + "\t");
                                if (tLn.Contains("," + tmp[2] + ","))
                                    tLn = tLn.Replace("," + tmp[2] + ",", "," + newPin + ",");
                                storeP.Add(tLn);
                            }
                        }
                    }
                    //else no pin invalid player, redact
                }
            }
            using (StreamWriter sw = new StreamWriter(fin))
            {
                foreach (string hdr in saveHDR)
                    sw.WriteLine(hdr);
                foreach (string sP in storeP) 
                    sw.WriteLine(sP);
            }
 
        }

        private int regor(string json)
        {
            json = json.Replace("\"","");
            string[] split = json.Split(new char[] {':',','});
            try {
                if (split[1].ToUpper().Equals("OK"))
                {
                    for (int i = 2; i < split.Length; i++)
                        if (split[i].ToUpper().Equals("GOR"))
                            return int.Parse(split[i + 1]);
                    return -1;
                }
                else
                    return -1;
            }
            catch (Exception e) { return -1; }
        }

		public void ReadByesFromFile(int nextRound){
			//read players file
			//if player already registered
			//check if his participation changed

			string tLn = "";
			string fin = workDirectory + "players.txt";
			using (StreamReader reader = new StreamReader (fin)) {
				for (int i = 0; i < 6; i++)
					tLn = reader.ReadLine(); //trip through headers
				while ((tLn = reader.ReadLine ()) != null) {
					String[] split = tLn.Split(new char[] {',','\t'});
					try {
						int pine = int.Parse (split [0]);
						int rats = int.Parse (split [2]);
						bool[] bull = new bool[nRounds]; //not set via input file
						for(int k=0; k<bull.Length; k++)
							bull[k]=true;
                        if(split.Length > 6)
						    for (int i = 6; i < split.Length; i++) {
							    if (split [i].Equals ("") == false) {
								    try{
									    int byeRound = int.Parse(split[i].Trim());
									    if(byeRound > nRounds){
										    Console.WriteLine("A bye cannot be allocated for a round which does not exist");
										    Console.WriteLine(tLn);
									    }
									    else
										    bull[byeRound-1]=false; //0 based
								    }
								    catch(Exception e){Console.WriteLine(e.Message);}
							    }
						    }
						Player j = new Player (pine, split [1], rats, split [3], split [4],  bull, split[5]);
						if (AllPlayers.Contains (j) == true) {
							for(int ap = 0; ap<AllPlayers.Count; ap++)
								if(j.Equals(AllPlayers[ap]))
									for(int i2=nextRound-1; i2<nRounds; i2++) //0 based
										AllPlayers[ap].SetParticipation(i2, bull[i2]);
						}
					} catch (Exception e) {
						Console.WriteLine ("An exception was encountered in ReadByesFromFile" + e.Message);
						Console.WriteLine (tLn);
					}
				}
			}
		}

        //	
        //  Pin tName tRating tClub tCountry is the expected input order THEN GRADE
		//
		public void ReadPlayers(bool Supression=false, bool Initial = false)
        {
			if (Initial == true) {
				Console.WriteLine ("Please press return when you have finished editing players.txt");
				Console.ReadLine ();
			}
            string tLn = "";
			string fin = workDirectory + "players.txt";
            using (StreamReader reader = new StreamReader(fin))
            {
                for (int i = 0; i < 6; i++)
                    tLn = reader.ReadLine(); //trip through headers

                while ((tLn = reader.ReadLine()) != null)
                {
					String[] split = tLn.Split(new char[] {',','\t'});
                    try
                    {
                        int pine = int.Parse(split[0]);
                        int rats = int.Parse(split[2]);
						bool[] bull = new bool[nRounds]; //not set via input file
						for(int k=0; k<bull.Length; k++)
							bull[k]=true;
                        if (split.Length > 6) // or no need to handle bye setting
                        {
                            for (int i = 6; i < split.Length; i++)
                            {
                                if (split[i].Equals("") == false)
                                {
                                    try
                                    {
                                        int byeRound = int.Parse(split[i].Trim());
                                        if (byeRound > nRounds)
                                        {
                                            Console.WriteLine("Bye cannot be allocated for round which does not exist");
                                            Console.WriteLine(tLn);
                                        }
                                        else
                                            bull[byeRound - 1] = false; //0 based
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("ReadPlayers() ByePart" + split.Length);
                                        Console.WriteLine(e.Message);
                                    }
                                }
                            }
                        }
                        Player j = new Player(pine, split[1], rats, split[3], split[4], bull, split[5]);
                        if (AllPlayers.Contains(j) == false)
                            AllPlayers.Add(j);
                        else
                            if (Verbose)
                                Console.WriteLine("Duplicate Entry detected");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Entry format error" + e.Message);
                        Console.WriteLine(tLn);
                    }
                }
				Console.WriteLine ("Number of players now registered is " + AllPlayers.Count);
            }
        }

        public bool GenerateTemplateInputFile()
        {
            try
            {
                Console.WriteLine("Please enter the name of the working folder for the tournament");
                if(Macintosh)
                    workDirectory = exeDirectory + "/"+ Console.ReadLine().Trim();
                else
                    workDirectory = exeDirectory + "\\"+ Console.ReadLine().Trim();
            }
            catch (Exception e) { GenerateTemplateInputFile(); }
			if (Directory.Exists (workDirectory) == false)
				Directory.CreateDirectory (workDirectory);
			if(Macintosh)
				workDirectory += "/";
			else
				workDirectory += "\\";
			Console.WriteLine("Working directory is: "+workDirectory);
			//does saved data exist
            if (File.Exists(workDirectory + "settings.txt") &&
                File.Exists(workDirectory + "players.txt") &&
                File.Exists(workDirectory + "Init.txt")
            )
            {
                Console.WriteLine("Would you like to restore saved tournament data? (yes / no) ");
                string _answer = Console.ReadLine();
                if (_answer.ToUpper().StartsWith("Y"))
                    return false; // Enter load stored tournament mode from Program.cs
            }
            string fOut = workDirectory + "players.txt";
			Console.WriteLine("Template file created at: "+fOut);
            Console.WriteLine("Setting Tournament Information...");
            Console.WriteLine("Please enter the Tournament Name:");
            string name = Console.ReadLine();
           
			// This information goes into a separate file  
			Console.WriteLine("Please enter number of Rounds:");
			string round = Console.ReadLine();
            try {
                nRounds = int.Parse(round);
            }
            catch (Exception e) {
                Console.WriteLine("Number of rounds set to 3 as it could not be read: " + e.Message);
                nRounds = 3; 
            }
            Console.WriteLine ("Top Group ? (yes / no )");
			string top  = Console.ReadLine ();
			if (top.ToUpper ().StartsWith ("Y")) {
				TopBar = true;
				Console.WriteLine ("Noted. Top Group Rating can be entered in Settings");
			}
            Console.WriteLine ("Rating Floor ? (yes / no )");
            string bot  = Console.ReadLine ();
			if (bot.ToUpper ().StartsWith ("Y")) {
				RatingFloor = true;
				Console.WriteLine ("Noted. Rating Floor can be entered in Settings");
			}
			Console.WriteLine ("Handicap Adjustment ? (0 for none)");
			string adj = Console.ReadLine();
            try{
                HandiAdjust = int.Parse(adj);
            }
            catch (Exception e){
                Console.WriteLine("Handicap Adjustment set to 1 as it could not be read: " + e.Message);
                HandiAdjust = 1;
            }
            Console.WriteLine("Maximum Handicap Allowed ?");
			string maxhan = Console.ReadLine();
            try {
                nMaxHandicap = int.Parse(maxhan);
            }
            catch (Exception e){
                Console.WriteLine("Maximum Handicap set to 9 as it could not be read: " + e.Message);
                nMaxHandicap = 9;
            }
			Console.WriteLine ("Grade Width (default 100)");
			string GradeWidth = Console.ReadLine();
			try{
				nGradeWidth = int.Parse(GradeWidth); 
			}
			catch(Exception e){
				Console.WriteLine("Grade width set to 100 as it could not be read " + e.Message);
			}
			Console.WriteLine ("Pairing Strategy (Fold / Simple / Split / Adjacent)");
			string pst = Console.ReadLine().ToUpper().Trim();
			if(pst.Equals("FOLD"))
				PairingStrategy = "Fold";
            if (pst.Equals("SPLIT"))
                PairingStrategy = "Split";
            if (pst.Equals("ADJACENT"))
                PairingStrategy = "Adjacent";
            bool overwritePlayers = true;
            if (File.Exists(fOut))
            {
                Console.WriteLine("Players file already exists - Overwrite it? (yes / no)");
                string s = Console.ReadLine().ToUpper();
				if (s.StartsWith ("N"))
                    overwritePlayers = false;
                else
                    File.Delete(fOut);
			}

            if(overwritePlayers)
                using (StreamWriter riter = new StreamWriter(fOut))
                {
                    riter.WriteLine("Tournament Name:\t" + name + ",");
                    riter.WriteLine("Copy Paste the Player information into the sheet,");
                    riter.WriteLine("PIN , Name, Rating, Club, Country,");
                    riter.WriteLine("type X into the round column to mark a Bye,");
                    riter.WriteLine(",");
                    string hdr = "Pin,Name,Rating,Club,Country";
                    string bdy = ",,,,";
                    for (int i = 0; i < nRounds; i++)
                    {
                        hdr = hdr + ",R" + (i + 1);
                        bdy = bdy + ",";
                    }
                    riter.WriteLine(hdr);
                    riter.WriteLine(bdy);
                }
			//write settings
            bool overwriteSettings = true;
			string fSettings = workDirectory + "settings.txt";
			if (File.Exists(fSettings))
			{
				Console.WriteLine("Settings file already exists - Overwrite it? (yes / no)");
				string s = Console.ReadLine().ToUpper();
				if (s.StartsWith("N"))
					overwriteSettings = false;
				else
					File.Delete(fSettings);
			}
            if(overwriteSettings)
			    using (StreamWriter riter = new StreamWriter (fSettings)) {
                    riter.WriteLine("Tournament Name:\t" + name);
                    riter.WriteLine("Rounds:\t" + nRounds);
                    riter.WriteLine("Pairing Strategy:\t" + PairingStrategy);
				    if (TopBar) {
					    riter.WriteLine ("Top Bar Rating:\t");
					    riter.WriteLine ("Permit handicap above bar:\tNo");
				    }
				    if(RatingFloor)
					    riter.WriteLine ("Rating Floor:\t");
				    riter.WriteLine ("Handicap Policy:\t"+HandiAdjust);
				    riter.WriteLine ("Max Handicap:\t"+nMaxHandicap);		
				    riter.WriteLine ("Grade Width:\t"+nGradeWidth);		
				    riter.WriteLine ("Tiebreak 1:\tSOS");		
				    riter.WriteLine ("Tiebreak 2:\t");		
				    riter.WriteLine ("Tiebreak 3:\t");		
			    }

			return true;
        }

		public void ReadSettings() //Use a switch statement !!!!!1
		{
			char[] ch = { '\t'};
			string[] s;
			string dbg="";
			try{
			using (StreamReader sr = new StreamReader (workDirectory + "settings.txt")) {
					while(sr.EndOfStream == false)	{
						dbg = sr.ReadLine ();
						if(dbg!=null)
						if(dbg.Length > 2){
							s =dbg.Split(ch);
							if(s[0].Contains("Tournament Name")){
								TournamentName = s[1];
                            }
                            if (s[0].Contains("Rounds"))
                            {
                                nRounds = int.Parse(s[1].Trim());
                            }
                            if (s[0].Contains("Pairing Strategy"))
                            {
                                PairingStrategy = s[1].Trim();
                            }
							if(s[0].Contains("Top Bar Rating")){
								nTopBar = int.Parse(s[1].Trim());
								TopBar = true;
							}
							if(s[0].Contains("Rating Floor")){
								nRatingFloor = int.Parse(s[1].Trim());
								RatingFloor = true;
							}
							if(s[0].Contains("Handicap Policy")){
								HandiAdjust = int.Parse(s[1].Trim());
							}
							if(s[0].Contains("Max Handicap")){
								nMaxHandicap = int.Parse(s[1].Trim());
							}
							if(s[0].Contains("Grade Width")){
								nGradeWidth = int.Parse(s[1].Trim());
							}
							if(s[0].Contains("Permit handicap above bar")){
	                            if (s[1].ToUpper().StartsWith("Y"))
	                                HandiAboveBar = true;
                            }
                            if (s[0].Contains("Tiebreak ") && s.Length > 1)
                            {
                                if (s[1].Trim() != "")
                                    Tiebreakers.Add(s[1].ToUpper());
                            }
                            if (s[0].Contains("Debug") && s.Length > 1)
                            {
                                Verbose = true;
                            }
						}
					}
				}
			}
			catch(Exception e) { Console.WriteLine ("exception from: {0} {1}",dbg,e.InnerException);}
			Player.SetTiebreakers(Tiebreakers);
		}

        // the file name this is pointing to is not well defined
        public void FormatMasterEGF()
		{
			string tTest = "PIN          Name                              Club     Grade P&D";
			bool faci = false;
            string tzipfile = workDirectory +"allworld_lp.html"; 
			string egfCopy = workDirectory + "egf.txt";
			if(File.Exists(egfCopy))
			try{
				File.Delete(egfCopy);
			}
			catch(Exception e){ Console.WriteLine (e.ToString ());
			}
			string line;
			string T = ",";
			StringBuilder li = new StringBuilder("");

			using (StreamReader reader = new StreamReader (tzipfile)) {
				using (StreamWriter riter = new StreamWriter (egfCopy)) {
					riter.WriteLine ("pin"+T+"name"+T+"rating"+T+"country"+T+"club"+T+"grade");
					while ((line = reader.ReadLine ()) != null) {
						if (faci) {
							line = line.Trim (); 
							if (line.Length < 89) {
								//Console.WriteLine (line); //Drobeta
                                ;
							} 
							else {
								/* pin - 8 digit  0 7
								 * name - 40 digit 8 47
								 * club country - 8 digit 48 55
								 * blank 56 58
								 * grade 59 61
								 * blank 62 69
								 * gor 70 73
								 * rest not interesting
								*/ 
								li.Append(line.Substring(0,8)); // Pin
									li.Append(T);
									li.Append(line.Substring(8,40).Trim()); //Name
									li.Append(T);
									li.Append(line.Substring(70,4).Trim()); // GoR
									li.Append(T);
									li.Append(line.Substring(48,3).Trim()); //Country
									li.Append(T);
									li.Append(line.Substring(51,5).Trim()); //Club
									li.Append(T);
									li.Append(line.Substring(59,3).Trim()); //Grade
									li.Append(T);
								riter.WriteLine (li);
								li.Clear ();
							}
						} 
						else {
							line = line.Trim (); 
							if (line.StartsWith (tTest)) {
								faci = true;
								line = reader.ReadLine ();
							}
						}
					
					}
					riter.Flush ();
				}
			}
			Console.WriteLine ("Player data extracted to the file egf.txt");
		}

		/*
Bd	White	Result	Black	Handicap  
1	Davis.A(1)	?:?	Jones.B(2)	0  
		 */ 
        public void ReadResults(int rnd)
        {
			List<Pairing> actualPairs = new List<Pairing> ();
			char[] c = { '\t','\v'};
			char[] c1 = { '(',')','?'};
			int[] LUT = new int[AllPlayers.Count];
			int[] CNT = new int[AllPlayers.Count];
            for (int i = 0; i < AllPlayers.Count; i++){
                if(Verbose) 
					Console.WriteLine("ReadResults()i:" + i + ":S:" + AllPlayers[i].Seed);
                LUT[AllPlayers[i].Seed - 1] = i;
            }
            using (StreamReader sr = new StreamReader(workDirectory + "Round" + rnd + "Results.txt"))
            {
				sr.ReadLine ();
				sr.ReadLine ();
				string s;
				while (sr.EndOfStream == false) {
					try{
						s = sr.ReadLine (); s.Trim (); }
					catch(Exception e){ s = "";
					}
					if (s.Length >2 ) {	
						string[] split = s.Split (c);
						string[] split2 = split [1].Split (c1);
						int white = int.Parse (split2 [1]) -1;
						split2 = split [3].Split (c1);
						int black = int.Parse (split2 [1]) -1;
						int handicap = int.Parse (split [4].Replace ("h", ""));
						int result = 0;
						if (split [2].Equals ("1:0"))
							result = 1;
						if (split [2].Equals ("0:1"))
							result = 2;
						if (split [2].Equals ("0.5:0.5"))
							result = 3;
						if (split [2].Equals ("0:0"))
								result = 7;
						if(Verbose)
							Console.WriteLine("Read Pairing: " + AllPlayers[LUT[white]].Seed + ":" + AllPlayers[LUT[black]].Seed);
						Pairing p = new Pairing (AllPlayers [LUT [white]], AllPlayers [LUT [black]], handicap, result);
						CNT [LUT [white]]++;
						CNT [LUT [black]]++;
						if (actualPairs.Contains (p) == false)
							if (CNT [LUT [white]] == 1 && CNT [LUT [black]] == 1)
								actualPairs.Add (p);
							else
								throw new Exception ("A player played more than one game in this round.");
						else
							Console.WriteLine ("A duplicate pairing result was detected " + p.ToFile ());
					}
				}
				//and if we did not hit an exception
				Console.WriteLine("Finished reading in results for Round " + rnd);
				RoundPairings = actualPairs;
                AllPairings.AddRange(actualPairs);
            }
        }


        public int RestoreTournament()
        {
            Console.WriteLine("Ire will attempt to restore the tournament to the end of the last round played.");
            Console.WriteLine("After doing so we will make a new draw. This means the old draw will be overwritten");
            Console.WriteLine("Enter the number of the last completed round.");
            int rndRestore = int.Parse(Console.ReadLine());
            for(int i=1; i<rndRestore+1; i++)
                if (File.Exists(workDirectory + "Round" + i + "Results.txt") == false )
                {
                    Console.WriteLine("Missing results for round " + i);
                    Console.WriteLine("This is a fatal error. Ire will exit.");
                    throw new Exception();
                }
            Console.WriteLine("Loading Settings ...");
            ReadSettings(); 
            Console.WriteLine("Loading Players ...");
            ReadPlayers();
            //read store file which should be in the same order as the players file(?)
            // look up table based on EGD pin
            // 
            List<int> EGD = new List<int>();
            List<int> Seed = new List<int>();
            List<float> iMMS = new List<float>();
            string[] tmp;
            using (StreamReader sr = new StreamReader(workDirectory + "Init.txt"))
            {
                try
                {
                    while (sr.EndOfStream == false)
                    {
                        tmp = sr.ReadLine().Split('\t');
                        if (tmp.Length > 3)
                        {
                            EGD.Add(int.Parse(tmp[0]));
                            Seed.Add(int.Parse(tmp[1]));
                            iMMS.Add(float.Parse(tmp[2]));
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine("Exception in Restore:"+ e.Message);  }
            }
            foreach (Player ap in AllPlayers)
            { //very slow method
                for (int i = 0; i < EGD.Count; i++)
                {
                    if (ap.EGDPin == EGD[i])
                    {
                        ap.Seed = Seed[i];
                        ap.initMMS = iMMS[i];
                        i = EGD.Count + 42;
                    }
                }
            }
            //Read in results from all rounds
            Console.WriteLine("Loading Result History ...");
            for (int i = 1; i < rndRestore + 1; i++)
            {
                ReadResults(i);
                ProcessResults(i);
            }
            Console.WriteLine("Updating Tiebreakers ...");
			UpdateTiebreaks (rndRestore); 
            return rndRestore + 1;
        }
#endregion

        #region Export Functions

		public void GenerateStore()
		{	string fOut = workDirectory + "Init.txt";
			using (StreamWriter riter = new StreamWriter(fOut))
				foreach (Player p in AllPlayers)
					riter.WriteLine(p.ToStore());
		}

        public void GenerateRoundResultsFile(int rnd, List<Pairing> ps)
        {
			string fOut = workDirectory + "Round" + rnd + "Results.txt";
            using (StreamWriter riter = new StreamWriter(fOut))
            {
                riter.WriteLine("Results of Round " + rnd);
                riter.WriteLine("Bd\twhite\t:\tblack");
				int count = 0;
                foreach (Pairing p in ps)
                    riter.WriteLine(++count + "\t" + p.ToFile());
            }

        }

        public void GenerateEGFExport()
        {   
            string fn = workDirectory + TournamentName.Trim().Replace(" ","") + ".h9";
            int[] lookup = new int[AllPlayers.Count + 1];
            string ln="";
            for (int ap = 0; ap< AllPlayers.Count; ap++)
            {
                lookup[AllPlayers[ap].Seed]=ap+1;
            }
            lookup[0] = 0;
            using (StreamWriter sw = new StreamWriter(fn))
            {
                sw.WriteLine("; EV[" + TournamentName.Trim().Replace(" ", "") + "]");
                sw.WriteLine("; Generated by Ire");
                sw.WriteLine("; ");
                foreach (Player ap in AllPlayers)
                {
                    ln += lookup[ap.Seed]; //finishing position
                    ln += " ";
                    ln += ap.ToEGF(); 
                    for (int r = 0; r < nRounds; r++)
                    {
                        ln += " " + lookup[ap.getOpponent(r)];
                        if (ap.getOpponent(r) != 0)
                        {
                            float result = ap.getResult(r);
                            if (result == 0f) ln += "-" + ap.EGFColour(r) + ap.getAdjHandi(r);
                            if (result == 0.5f) ln += "=" + ap.EGFColour(r) + ap.getAdjHandi(r);
                            if (result == 1f) ln += "+" + ap.EGFColour(r) + ap.getAdjHandi(r);
                        }
                        else
                        {
                            float result = ap.getResult(r);
                            if (result == 0f) ln += "-";
                            if (result == 0.5f) ln += "=";
                            if (result == 1f) ln += "+";

                        }
                    }
                        sw.WriteLine(ln);
                        ln = "";
                }
            }
        }

		public void GenerateStandingsfile(int rnd)
		{
            if (File.Exists(workDirectory + "Round" + rnd + "Standings.txt"))
            {
                Console.WriteLine("Warning - Overwriting Round" + rnd + "Standings.txt");
                File.Delete(workDirectory + "Round" + rnd + "Standings.txt");
            }
            string hdr = "Pl\tName\tRank\tRating\tMMS\tWins\t";
            for (int i = 0; i < rnd; i++)
                hdr = hdr + (i+1) + "\t";
            foreach(string tb in Tiebreakers)
                hdr = hdr + tb + "\t";

            using (StreamWriter sw = new StreamWriter(workDirectory + "Round" + rnd + "Standings.txt"))
            {
                sw.WriteLine("Tournament: " + TournamentName + " Round: " + rnd);
                sw.WriteLine("");
                sw.WriteLine(hdr);
                int cnt = 1; 
                string t = "\t";
                foreach (Player ap in AllPlayers)
                {
					sw.WriteLine(cnt++ + t + ap.ToStanding(rnd) + TiebreakerOut(ap)); 
                }
            }
			VerboseStandings (rnd); //To be made optional ?
		}

		public void VerboseStandings(int rnd)
		{
			string hdr = "Pl\tName\tRank\tRating\tMMS\tWins\t";
			for (int i = 0; i < rnd; i++)
				hdr = hdr + (i+1) + "\t";
			foreach(string tb in Tiebreakers)
				hdr = hdr + tb + "\t";
			if (rnd == 0)
			if (File.Exists (workDirectory + "RoundStandings.txt"))
				File.Delete (workDirectory + "RoundStandings.txt");
			using (StreamWriter sw = new StreamWriter (workDirectory + "RoundStandings.txt", true)) {
				sw.WriteLine ("Standings for Round " + rnd);
				sw.WriteLine("");
				sw.WriteLine(hdr);
				int cnt = 1;
				foreach (Player ap in AllPlayers)
				{
					sw.WriteLine(cnt++ + "\t" + ap.ToStandingVerbose(rnd) + TiebreakerOut(ap)); 
				}
				sw.WriteLine("");
			}

		}

		private string TiebreakerOut(Player p)
		{
			string s = "";
			foreach (string tb in Tiebreakers) {
				if (tb.Equals ("SOS"))
					s += (p.SOS + "\t");
				if (tb.Equals ("SODOS"))
                    s += (p.SODOS + "\t");
                if (tb.Equals("MOS"))
                    s += (p.MOS + "\t");
                if (tb.Equals("MDOS"))
                    s += (p.MDOS + "\t"); 
                if (tb.Equals("OPERA"))
                    s += (p.OPERA + "\t");
			}
			return s;
		}


		public void ConvertStandingsToHTML(int rnd)
		{
            string fName = workDirectory + "Round" + rnd + "Standings.txt";
			List <string> AllLines = new List<string>();
			using (StreamReader sr = new StreamReader (fName)) {
				sr.ReadLine ();sr.ReadLine ();
				while (sr.EndOfStream == false) {
					AllLines.Add (sr.ReadLine ());
				}
			}
			string fOut = fName.Replace (".txt", ".html");
			Console.WriteLine (fOut);
			using (StreamWriter sw = new StreamWriter (fOut)) {
				sw.WriteLine ("<html><table border=1>");
				foreach (string al in AllLines) {
					sw.WriteLine("<tr><td>" + al.Replace("\t","</td><td>") + "</td></tr>");
				}
				sw.WriteLine ("</table>");
				if(TopBar)
					sw.WriteLine ("<p>Rating Bar: " + nTopBar);
				if(RatingFloor)
					sw.WriteLine ("<p>Rating Floor: " + nRatingFloor);
				sw.WriteLine ("<p>Handicap Adjustment: " + HandiAdjust + " Max Handicap: " + nMaxHandicap);
				sw.WriteLine ("</html>");
			}
		}
        #endregion
    }
}
