using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{ //combine Import and Turn 
    class TournamentBoss
    {
        int nRounds = 0;
        string exeDirectory;
        string workDirectory;
        bool Macintosh = false;
        List<Jucator> AllPlayers = new List<Jucator>();


        public TournamentBoss()
        {
            exeDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(exeDirectory);
        }

#region Import Functions
        public void DownloadEGFMasterZip()
        {
            string uri = "http://www.europeangodatabase.eu/EGD/EGD_2_0/downloads/allworld_lp.zip";
         //   string egfCopyUnZipped = "/Users/iandavis/egzipdata.txt";
            WebClient client = new WebClient ();
            client.DownloadFile (uri, workDirectory + "egzipdata.zip");
            Console.WriteLine("DownloadMasterZipEGF file downloaded");
            FileInfo fi = new FileInfo(workDirectory + "egzipdata.zip");
            using (ZipFile zip = ZipFile.Read(fi.FullName))
            {
                zip.ExtractAll(workDirectory);
            }
        }

        /*	
         * Pin tName tRating tClub tCountry is the expected input order
         * GIGO method, checks for already entered player
		*/
        public void ReadJucatori()
        {
           
            string tLn = "";
            string fin = workDirectory + "players.csv";
            using (StreamReader reader = new StreamReader(fin))
            {
                for (int i = 0; i < 6; i++)
                    tLn = reader.ReadLine(); //trip through headers

                while ((tLn = reader.ReadLine()) != null)
                {
                    //tLn = reader.ReadLine();
                    String[] split = tLn.Split(',');
                    Console.WriteLine(split.Length);
                    int rnd = split.Length - 5;
                    Console.WriteLine("rnd" + rnd);

                    try
                    {
                        int pine = int.Parse(split[0]);
                        Console.WriteLine("pin" + pine);
                        int rats = int.Parse(split[2]);
                        Console.WriteLine("rat" + rats);
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
                        Jucator j = new Jucator(pine, split[1], rats, split[3], split[4], bull);
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
            string fOut = workDirectory + "players.csv";
            Console.WriteLine("Setting Tournament Information...");
            Console.WriteLine("Please enter the Tournament Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Please enter number of Rounds:");
            string round = Console.ReadLine();
            nRounds = int.Parse(round);
            /*
            Console.WriteLine ("Top Group (yes / no )");
            string top  = Console.ReadLine ();
            Console.WriteLine ("Lower Group(yes / no )");
            string bot  = Console.ReadLine ();
*/
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
        #region Export Functions

        public void GenerateRoundResults(int i, List<Pairing> ps)
        {
            string fOut = workDirectory + "Round" + i + "Results.tsv";
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
