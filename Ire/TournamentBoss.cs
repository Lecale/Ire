using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{ 
	//New Class to combine Import and Turn 
    class TournamentBoss
    {
        int nRounds = 0;
        string exeDirectory;
		string TournamentName;
        private string workDirectory;
        bool Macintosh = false;
		bool TopBar=false;
		bool RatingFloor=false;
		int HandiAdjust=1;
		int nMaxHandicap = 9;
		int nTopBar = 5000;
		int nRatingFloor = 100;
		int nGradeWidth = 100;
        List<Player> AllPlayers = new List<Player>();
		List<Player> RoundPlayers;


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
				Player.SortByRating = false;
				//top group but for bye will then be seeded as if in top group
				//this should not have any negative effect on the pairings though
				foreach (Player p in AllPlayers) 
					p.Seed = i++;
			}
		}

		public void MakeDraw(int currentRound = 1)
		{
			Console.WriteLine ("We are ready to make the draw for Round "+currentRound);
			Console.WriteLine ("Do you want to make an update to the players list (yes / no)");
			string s = Console.ReadLine (); 
			int i = -1;
			//Handle this later
			UpdateParticipation (currentRound);
			InitMMS ();
			if(RoundPlayers.Count % 2 == 1 )
				i = AssignBye (currentRound);
            while (i == -1)
            {
                int retry = 2;
                Console.WriteLine("No player was found who already had less than " + (retry - 1) + "bye");
                if (currentRound <= retry)
                    i = AssignBye(currentRound, retry++);
                else
                {
                    i = 100;
                    Console.WriteLine("Fatal error encountered. Tournament cannot proceed");
                }
            }
			Console.WriteLine ("MakeDraw prep completed ...");
			DrawMachine1 dm1 = new DrawMachine1 (RoundPlayers, 9, 1, true);

			List<Pairing> RndPairings = dm1.GetCurrentPairings ();
			foreach (Pairing rp in RndPairings)
				Console.WriteLine (rp.BasicOutput());
		}

		public void InitMMS() 
		{
			SortField (true);
            Console.WriteLine("InitMMS()");
            Console.WriteLine("eRating - MMS - Rating - Seed");
			foreach (Player ap in AllPlayers) {
				int gap = nTopBar - ap.getERating ();
				gap = gap / 100;
				if (gap >= 0 && ap.topBar == false)
					gap++;
				ap.setMMS (100 - gap);
				Console.WriteLine (ap.getERating() + " - " + ap.getMMS() + " - " + ap.getRating() + " - " + ap.getSeed());
			}

		}

		public void UpdateParticipation(int _rnd)
		{
			RoundPlayers = new List<Player> ();
			foreach (Player p in AllPlayers) {
				if (p.getParticipation (_rnd-1))
					RoundPlayers.Add (p);
			}
			Console.WriteLine ("for this round we have " + RoundPlayers.Count);
		}

		public int AssignBye(int _rnd, int ByeLevel=1)
		{ 
		//	Console.WriteLine("First player bye count is " + RoundPlayers [0].nBye () + "byeLevel is " + ByeLevel);
		//	Console.WriteLine("Player count is " + RoundPlayers.Count);
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
		#endregion

		#region TestPreviewFunctions
		public void GeneratePlayers(int nPlayers,int rnd)
		{
			Utility u = new Utility ();
			string end = "";
			for(int r=0; r<rnd; r++)
				end += "," + (r + 1);
			string fn = workDirectory + "players.txt";

			// 			riter.WriteLine("PIN , Name, Rating, Club, Country,")
			int i = 100;
			using (StreamWriter sw = new StreamWriter (fn,true)) {
				for(int np = 0; np<nPlayers; np++)
					sw.WriteLine (i++ + "," + u.ProvideName() + "," + u.RatingByBox(1500,500) + ",BLF,IE" + end);
			}
		}
		public void ShowField()
		{
			using(StreamWriter sw = new StreamWriter(workDirectory +  "dbg.txt"))
				{
				foreach(Player p in AllPlayers )
					sw.WriteLine(p.ToString());			
				}
		}
		public void previewFloor(bool SetFloor =  false)
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
			if (SetFloor) {
				Console.WriteLine ("Apply this setting (yes / no )");
				if (Console.ReadLine ().ToUpper ().StartsWith ("Y")) {
					foreach (Player pete in AllPlayers) {        
						if (nRatingFloor > pete.getRating () ) {
							pete.setERating (nRatingFloor);
						}
					}
				}
				else
				{
					Console.WriteLine ("Enter a new value for the Rating Floor");
					nRatingFloor = int.Parse(Console.ReadLine ().Trim ());
					previewFloor(true);
				}
			}
		}
		public void previewTopBar(bool SetBar = false)
		{ 
			int tCount = 0;
			foreach (Player peter in AllPlayers) {   
				if (peter.getRating () > nTopBar && peter.nBye()==0) {
					Console.WriteLine (peter.getName () + " " + peter.getRating ());
					tCount++;
				}
			}
			Console.WriteLine ("Provisionally " + tCount + " in top group");
			if (SetBar) {
				Console.WriteLine ("Apply this setting (yes / no )");
				if (Console.ReadLine ().ToUpper ().StartsWith ("Y")) {
					foreach (Player pete in AllPlayers) {        
						if (nTopBar < pete.getRating () && pete.nBye()==0) {
							pete.topBar = true;
							pete.setERating (nTopBar);
						}
					}
				}
				else
				{
					Console.WriteLine ("Enter a new value for the Rating Top Bar");
					nTopBar = int.Parse(Console.ReadLine ().Trim ());
					previewTopBar(true);
				}
			}
		}

		#endregion

		#region ImportFunctions
        public void DownloadEGFMasterZip()
        {
            string uri = "http://www.europeangodatabase.eu/EGD/EGD_2_0/downloads/allworld_lp.zip";
			FileInfo fi;
         //   string egfCopyUnZipped = "/Users/iandavis/egzipdata.txt";
            WebClient client = new WebClient ();
			if (Macintosh) {
				client.DownloadFile (uri, workDirectory + "/egzipdata.zip");
				Console.WriteLine ("DownloadMasterZipEGF file downloaded");
				fi = new FileInfo (workDirectory + "/egzipdata.zip");
			} else {
				client.DownloadFile (uri, workDirectory + "\\egzipdata.zip");
				Console.WriteLine ("DownloadMasterZipEGF file downloaded");
				fi = new FileInfo (workDirectory + "\\egzipdata.zip");
			}
            using (ZipFile zip = ZipFile.Read(fi.FullName))
            {
                zip.ExtractAll(workDirectory);
            }
        }

        /*	
         * Pin tName tRating tClub tCountry is the expected input order
         * GIGO method, checks for already entered player
		*/
        public void ReadPlayers()
        {
           
            string tLn = "";
			string fin = workDirectory + "players.txt";
            using (StreamReader reader = new StreamReader(fin))
            {
                for (int i = 0; i < 6; i++)
                    tLn = reader.ReadLine(); //trip through headers

                while ((tLn = reader.ReadLine()) != null)
                {
                    //tLn = reader.ReadLine();
                    String[] split = tLn.Split(',');
                    //Console.WriteLine(split.Length);
                    int rnd = split.Length - 5;
                    //Console.WriteLine("rnd" + rnd);

                    try
                    {
                        int pine = int.Parse(split[0]);
                  //      Console.WriteLine("pin" + pine);
                        int rats = int.Parse(split[2]);
                    //    Console.WriteLine("rat" + rats);
                        bool[] bull = new bool[rnd];
                        for (int i = 5; i < rnd + 5; i++)
                        {
                            if (split[i].Equals("") == false)
                            {
                                if ((split[i].Trim()).ToUpper().Equals("X"))
                                    bull[i - 5] = false;
                                else
                                    bull[i - 5] = true;
                            }
                        }
                        Player j = new Player(pine, split[1], rats, split[3], split[4], bull);
                        if (AllPlayers.Contains(j) == false)
                            AllPlayers.Add(j);
                        else
                            Console.WriteLine("Duplicate Entry detected");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Entry format error" + e.Message);
                        Console.WriteLine(tLn);
                    }
                }

				Console.WriteLine ("Number of players registered is " + AllPlayers.Count);
            }
        }

        public void GenerateTemplateInputFile()
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
            string fOut = workDirectory + "players.txt";
			Console.WriteLine("Template at: "+fOut);
            Console.WriteLine("Setting Tournament Information...");
            Console.WriteLine("Please enter the Tournament Name:");
            string name = Console.ReadLine();
           
			// This information goes into a separate file  
			Console.WriteLine("Please enter number of Rounds:");
			string round = Console.ReadLine();
			nRounds = int.Parse(round); 
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
			HandiAdjust = int.Parse(adj); 
			Console.WriteLine ("Maximum Handicap Allowed ?");
			string maxhan = Console.ReadLine();
			nMaxHandicap = int.Parse(maxhan); 
			Console.WriteLine ("Grade Width (default 100)");
			string GradeWidth = Console.ReadLine();
			//nGradeWidth = int.Parse(GradeWidth); //Implement later - not essential

            if (File.Exists(fOut))
            {
                Console.WriteLine("File already exists - Overwrite it? y/n");
                string s = Console.ReadLine().ToUpper();
                if (s.StartsWith("N"))
                    return;
                else
                    File.Delete(fOut);
			}

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
			string fSettings = workDirectory + "settings.txt";
			if (File.Exists(fSettings))
			{
				Console.WriteLine("File already exists - Overwrite it? y/n");
				string s = Console.ReadLine().ToUpper();
				if (s.StartsWith("N"))
					return;
				else
					File.Delete(fSettings);
			}
			using (StreamWriter riter = new StreamWriter (fSettings)) {
				riter.WriteLine ("Tournament Name:\t" + name);
				riter.WriteLine ("Rounds:\t" + nRounds);
				if (TopBar) {
					riter.WriteLine ("Top Bar Rating:\t");
					riter.WriteLine ("Permit handicap above bar:\t");
				}
				if(RatingFloor)
					riter.WriteLine ("Rating Floor:\t");
				riter.WriteLine ("Handicap Policy:\t"+HandiAdjust);
				riter.WriteLine ("Max Handicap:\t"+nMaxHandicap);		
			}
        }

		public void ReadSettings()
		{
			char[] ch = { '\t'};
			string[] s;
			try{
			using (StreamReader sr = new StreamReader (workDirectory + "settings.txt")) {
				while(true)	{
						s = sr.ReadLine ().Split(ch);
						if(s[0].Contains("Tournament Name")){
							TournamentName = s[1];
						}
						if(s[0].Contains("Rounds")){
							nRounds = int.Parse(s[1].Trim());
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
						if(s[0].Contains("MPermit handicap above bar")){
							;//not handled yet
						}
					}
				}
			}
			catch(Exception e) { Console.WriteLine (e.Message);}
		}

        // the file name this is pointing to is not well defined
        public void FormatMasterEGF()
		{
			string tTest = "PIN          Name                              Club     Grade P&D";
			bool faci = false;	
			string egfCopy = workDirectory + "egf.tsv";
			if(File.Exists(egfCopy))
			try{
				File.Delete(egfCopy);
			}
			catch(Exception e){ Console.WriteLine (e.ToString ());
			}
			string line;
			string T = "\t";
			StringBuilder li = new StringBuilder("");

			using (StreamReader reader = new StreamReader ("/Users/iandavis/egfcopi.txt")) {
			//using (StreamReader reader = new StreamReader ("/Users/iandavis/egfrawdata.txt")) {

				using (StreamWriter riter = new StreamWriter ("/Users/iandavis/egf.tsv")) {
					riter.WriteLine ("pin"+T+"name"+T+"rating"+T+"country"+T+"club"+T+"grade");
					while ((line = reader.ReadLine ()) != null) {
						if (faci) {
							line = line.Trim (); 
							if (line.Length < 89) {
								Console.WriteLine (line); //Drobeta
							} 
							else {
								/* Want to pretty up the data
								 * 
								 * pin - 8 digit  0 7
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
							Console.WriteLine (line);
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


		}
			
#endregion

		//In fact we may no longer wish to use Pairing class
        #region Export Functions

        public void GenerateRoundResults(int i, List<Pairing> ps)
        {
			string fOut = workDirectory;
			if(Macintosh)
				workDirectory += "/Round" + i + "Results.txt";
			else
				workDirectory += "\\Round" + i + "Results.txt";
            using (StreamWriter riter = new StreamWriter(fOut))
            {
                riter.WriteLine("Results of Round " + i);
                riter.WriteLine("white  :  black");
                foreach (Pairing p in ps)
                    riter.WriteLine(p.BasicOutput());
            }

        }

        #endregion
    }
}
