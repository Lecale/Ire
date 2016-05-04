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
	//New Class to combine Import and Turn 
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
                    Console.WriteLine("After updating the Players file press any key to continue");
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
			Console.WriteLine ("The draw will now be made ...");
            if (PairingStrategy.ToUpper().Equals("SIMPLE"))
            {
                DrawMachine1 dm1 = new DrawMachine1(RoundPlayers, AllPairings, nMaxHandicap, HandiAdjust, HandiAboveBar);
                RndPairings = dm1.GetCurrentPairings();
            }
            if (PairingStrategy.ToUpper().Equals("FOLD"))
            {
                DrawMachine2 dm2 = new DrawMachine2(AllPlayers, AllPairings, currentRound, nMaxHandicap, HandiAdjust, HandiAboveBar);
                RndPairings = dm2.GetCurrentPairings();
            }
            if (PairingStrategy.ToUpper().Equals("SPLIT"))
            {
                DrawMachine3 dm3 = new DrawMachine3(AllPlayers, AllPairings, currentRound, nMaxHandicap, HandiAdjust, HandiAboveBar);
                RndPairings = dm3.GetCurrentPairings();
            }
            foreach (Pairing rp in RndPairings)
                Console.WriteLine(rp.BasicOutput());
            GenerateRoundResultsFile(currentRound, RndPairings);
            Console.WriteLine();
            Console.WriteLine("When you are ready to read in the results, press any key");
            Console.WriteLine("Remember that the draw can be overwritten in the input file");
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
			Console.WriteLine ("for this round we have " + RoundPlayers.Count);
		}

		public int AssignBye(int _rnd, int ByeLevel=1)
		{ 
			for (int i = RoundPlayers.Count - 1; i >-1; i--) {
				Console.WriteLine (i);
				if (RoundPlayers [i].nBye() < ByeLevel) {
					Console.WriteLine ("A bye will be assigned to ...");
					Console.WriteLine (RoundPlayers [i].ToString ());
					RoundPlayers [i].AssignBye (_rnd);
					RoundPlayers.RemoveAt (i);
					return i;
				}
			}
			//emergency
			Console.WriteLine("Wow no Bye Candidate found...");
			return -1;
		}

		//It should be possible to reverse Byes using this method
		public void ProcessResults(int rnd)
        {
            if (rnd == 1)
                GenerateStore();
			Console.WriteLine ("ProcessResults: rnd " + rnd + " pairings " + RoundPairings.Count);
			//lookuptable
			foreach (Pairing p in RoundPairings) {
				if(p.white.getParticipation(rnd-1)==false) 
					Console.WriteLine("The following was originally assigned a bye:" + p.white.getName());
				if(p.black.getParticipation(rnd-1)==false) 
					Console.WriteLine("The following was originally assigned a bye:" + p.black.getName());
				p.white.setResult (rnd, p.black.Seed, p.WhiteScore (), p.GetHandi (), 1);
				p.black.setResult (rnd, p.white.Seed, p.BlackScore (), p.GetHandi (), 0);
                Console.WriteLine("W" + p.white.Seed + ": B " + p.black.Seed);
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
            {
                lookUp[AllPlayers[i].Seed - 1] = i;
            }
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
                            Console.WriteLine("a bye detected for " +ap.Seed + " in round "+i);
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
                if (nGame < rnd && nGame != 0)
                {
                    _SOS = _SOS * rnd / nGame;
                    _SODOS = _SODOS * rnd / nGame;
                }
                if (nGame == 0)
                    _SOS = ap.initMMS;
                if (rnd > 2)
                    _MOS = _SOS - maxSOS - minSOS;
                ap.SOS = _SOS;
                ap.MOS = _MOS;
                ap.SODOS = _SODOS;
                if (ap.getScore(rnd - 1) == 0)
                    ap.MDOS = _SODOS / ap.getScore(rnd - 1);
                else
                    ap.MDOS = 0;
            }
        }

		public void HandleLatePlayers(int rnd)
		{
			Console.WriteLine ("Late entrants should be added to the file players.txt");
			Console.WriteLine ("Press any key to proceed");
			string s = Console.ReadLine ();
			int before = AllPlayers.Count;
			ReadPlayers(false); //later true
			int after = AllPlayers.Count;
			//init mms
			//give byes
            for (int i = after; i > before; i--)
            {
//				Console.WriteLine ("Late Loop player:" + i);
                AllPlayers[i - 1].SetSeed(i);
				AllPlayers [i - 1].topBar = false; //should already be false?
                if(TopBar)
    				if (AllPlayers [i - 1].getERating () > nTopBar)
	    				AllPlayers [i - 1].setERating (nTopBar);
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
				for (int j = 1; j < rnd; j++) {
//					Console.WriteLine ("Late Loop assign bye round:" + j);
					AllPlayers [i - 1].AssignBye (j);
				}
				for (int k = 0; k < nRounds; k++)
					if (AllPlayers [i - 1].getParticipation (k))
						Console.WriteLine (AllPlayers [i - 1].ToString () + " plays in "+ (k + 1));
            }
//			Console.WriteLine ("sort field again");
			SortField ();
		}
		#endregion

		#region TestPreviewFunctions
		public void GeneratePlayers(int nPlayers=17, int midpoint=1500, int spread=500 )
		{
            Console.WriteLine("Generate Dummy Players? (Yes/No)");
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
                                pete.setERating(nTopBar);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Enter a new value for the Rating Top Bar");
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
			string uri = "http://www.europeangodatabase.eu/EGD/EGD_2_0/downloads/allworld_lp.zip";
			if( File.Exists(workDirectory + "egzipdata.zip"))
				File.Delete(workDirectory + "egzipdata.zip");
			if( File.Exists(workDirectory + "allworld_lp.html"))
				File.Delete(workDirectory + "allworld_lp.html");
			FileInfo fi;
            WebClient client = new WebClient ();
			client.DownloadFile (uri, workDirectory + "egzipdata.zip");
				Console.WriteLine ("DownloadMasterZipEGF file downloaded");
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
            Console.WriteLine("RefreshPlayers() waits 5 seconds between each call to the EGD");
            string tLn = "";
            string fin = workDirectory + "players.txt";
            WebClient wc = new WebClient();
            string[] tmp;
            char[] ctsv = { ',', '\t' };
            string newGor;
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
//                    Console.WriteLine(tLn);
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
										    Console.WriteLine("Bye cannot be allocated for round which does not exist");
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
        public void ReadPlayers(bool Supression=false)
        {
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
                            if (Supression == false)
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
                Console.WriteLine("Would you like to restore saved tournament data? (yes:no) ");
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
			Console.WriteLine ("Pairing Strategy (Fold/Simple/Split)");
			string pst = Console.ReadLine().ToUpper().Trim();
			if(pst.Equals("FOLD"))
				PairingStrategy = "Fold";
			if(pst.Equals("SPLIT"))
				PairingStrategy = "Split";
            bool overwritePlayers = true;
            if (File.Exists(fOut))
            {
                Console.WriteLine("Players file already exists - Overwrite it? y/n");
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
				Console.WriteLine("Settings file already exists - Overwrite it? y/n");
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

		public void ReadSettings()
		{
			char[] ch = { '\t'};
			string[] s;
			string dbg="";
			try{
			using (StreamReader sr = new StreamReader (workDirectory + "settings.txt")) {
				while(true)	{
						dbg = sr.ReadLine ();
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
							}
							if(s[0].Contains("Rating Floor")){
								nRatingFloor = int.Parse(s[1].Trim());
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
							if(s[0].Contains("Tiebreak ") && s.Length > 1){
								if(s[1].Trim()!="")
									Tiebreakers.Add(s[1].ToUpper());
							}
						}
					}
				}
			}
			catch(Exception e) { Console.WriteLine (e.Message); Console.WriteLine ("exception from: "+dbg);}
			Player.SetTiebreakers(Tiebreakers);
		}

        // the file name this is pointing to is not well defined
        public void FormatMasterEGF()
		{
            //Console.WriteLine(DateTime.Now);
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
//							Console.WriteLine (line);
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
//            Console.WriteLine(DateTime.Now);
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
            //    Console.WriteLine("ReadResults()i:" + i + ":S:" + AllPlayers[i].Seed);
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
					//	Console.WriteLine ("Reading: " + s);
						//s.Replace (space, tab);
						string[] split = s.Split (c);
						string[] split2 = split [1].Split (c1);
						int white = int.Parse (split2 [1]) -1;
						split2 = split [3].Split (c1);
						int black = int.Parse (split2 [1]) -1;
						int handicap = int.Parse (split [4].Replace ("h", ""));
						//split2 = split [2].Split (c1);
						int result = 0;
						if (split [2].Equals ("1:0"))
							result = 1;
						if (split [2].Equals ("0:1"))
							result = 2;
						if (split [2].Equals ("0.5:0.5"))
							result = 3;
						if (split [2].Equals ("0:0"))
							result = 7;
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
//                Console.WriteLine("Total pairings count now " + AllPairings.Count);
            }
        }


        public int RestoreTournament()
        {
            Console.WriteLine("Ire will attempt to restore the tournament to the last round played.");
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
            Console.WriteLine("Loading Settings..");
            ReadSettings();
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
            Console.WriteLine("Loading Result History..");
            for (int i = 1; i < rndRestore + 1; i++)
            {
                ReadResults(i);
                ProcessResults(i);
            }
            Console.WriteLine("Updating Tiebreakers..");
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

		/*
; TC[T160130A]
; CL[C]
; EV[BelfastHandicap] YES
; PC[IE, Belfast]
; DT[2016-01-30,2016-01-30]	
; HA[h9]   
; KM[6.5]	
; TM[52]	
; CM[Standard byo-yomi (submitted by jameshut)]
; Generated by OpenGotha 3.35
;
; Pl Name                            Rk Co Club  CAT  NBW  EXT  EXR
		1 Gociu Tiberiu      5k IE  Belf  1    3    7    7    2+/b7  4+/w6  5+/w1  0-      |12850431
		*/
        public void GenerateEGFExport()
        {   //This method is lazy and only gives the ESSENTIAL components of the file
            string fn = workDirectory + TournamentName.Trim().Replace(" ","") + ".h9";
            if (File.Exists(fn))
            {
                File.Delete(fn);
            }
            using (StreamWriter sw = new StreamWriter(fn))
            {
                sw.WriteLine("; EV[" + TournamentName.Trim().Replace(" ", "") + "]");
                sw.WriteLine("; Generated by Ire");
                sw.WriteLine("; ");
                int i = 1;
                foreach (Player ap in AllPlayers)
                {
                    sw.WriteLine(ap.ToEGF());
                }
            }
        }

		public void GenerateStandingsfile(int rnd)
		{
			//if file exists , delete it
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
                // add tied method to Players
                foreach (Player ap in AllPlayers)
                {
					sw.WriteLine(cnt++ + t + ap.ToStanding(rnd) + TiebreakerOut(ap)); // + tiebreak
                }
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
				if (tb.Equals ("MOS"))
					s += (p.MOS + "\t");
			}
			return s;
		}


		public void ConvertStandingsToHTML(int rnd)
		{//todo
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
