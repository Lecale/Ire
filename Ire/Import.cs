using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Ionic.Zip;

namespace Ire
{
	class Import
	{
		/*
		 * Directory path flexibility not here yet
		 */

		public Import ()
		{
		}

	/*
	 * Download Master file
	 * List of all World players who ever participated in any included tournament
	 * Should really download the zipped version to be nice
	 */ 
	public void DownloadMasterEGF ()
	{
		string uri = "http://www.europeangodatabase.eu/EGD/EGD_2_0/downloads/allworld_lp.html";
		
		WebClient client = new WebClient ();
		string egfCopy = "/Users/iandavis/egfrawdata.txt";

		Stream data = client.OpenRead (uri);
		StreamReader reader = new StreamReader (data);
		string s = reader.ReadToEnd ();
		data.Close ();
		reader.Close ();


		using (StreamWriter riter = new StreamWriter (egfCopy))
			riter.Write (s);

	}



		public void DownloadMasterZipEGF ()
		{

			string uri = "http://www.europeangodatabase.eu/EGD/EGD_2_0/downloads/allworld_lp.zip";
			string egfCopy = "/Users/iandavis/egzipdata.zip";
			string egfCopyUnZipped = "/Users/iandavis/egzipdata.txt";

//			WebClient client = new WebClient ();
//			client.DownloadFile (uri, egfCopy);

			Console.WriteLine ("DownloadMasterZipEGF file downloaded");

			FileInfo fi = new FileInfo (egfCopy);

			string unzipdir = "/Users/iandavis/";
			using (ZipFile zip = ZipFile.Read(fi.FullName))
			{
				zip.ExtractAll (unzipdir);
			}
		}


		/*	Pin tName tRating tClub tCountry
		 *
		 *What if the player already exists, do we have to worry about this?
		*/
		public void ReadPlayers (List<Player> _p)
		{
			string tLn = "";
			string fin = "/Users/iandavis/players.tsv";
			using(StreamReader reader = new StreamReader (fin))
			{
				tLn = reader.ReadLine();
				String [] split = tLn.Split ('\t');
				int rnd = split.Length - 4;
				try{
					int pine = int.Parse(split[0]);
					int rats = int.Parse(split[2]);
					bool [] bull = new bool[rnd];
					for(int i=5; i<split.Length; i++)
					{
						if((split[i].Trim()).ToUpper().Equals( "X"))
							bull[i-5] = false;
						else
							bull[i-5] = true;
					}
					_p.Add( new Player(pine,split[1],rats,split[3],split[4],bull) );
				}
				catch(Exception e) {
					Console.WriteLine ("Entry format error");
					Console.WriteLine (tLn);
				}
			}
		}

		public void GenerateRoundResults(int i, List<Pairing> ps)
		{
			string fOut = "/Users/iandavis/Round" + i + "Results.tsv";

			using (StreamWriter riter = new StreamWriter (fOut)) {
				riter.WriteLine ("Results of Round " + i);
				riter.WriteLine ("white  :  black");
				foreach (Pairing p in ps)
					riter.WriteLine (p.BasicOutput());
			}				

		}

		public void GenerateTemplate()
		{
			string fOut = "/Users/iandavis/players.tsv";
			Console.WriteLine ("Setting Tournament Information");
			Console.WriteLine ("Please enter the Tournament Name:");
			string name  = Console.ReadLine ();
			Console.WriteLine ("Please enter number of Rounds:");
			string round  = Console.ReadLine ();
			/*
			Console.WriteLine ("Top Group (yes / no )");
			string top  = Console.ReadLine ();
			Console.WriteLine ("Lower Group(yes / no )");
			string bot  = Console.ReadLine ();
*/
			using (StreamWriter riter = new StreamWriter (fOut)) {
				riter.WriteLine ("Tournament Name: " + name);
				riter.WriteLine ("Copy Paste the Player information into the sheet");
				riter.WriteLine ("PIN , Name, Rating, Club, Country");
				riter.WriteLine ("type X into the round column to mark a Bye");
				riter.WriteLine ("\t");
				string hdr = "Pin\tName\tRating\tClub\tCountry";
				for (int i = 0; i < int.Parse (round); i++) {
					hdr = hdr + "\tR" + (i + 1);
				}
				riter.WriteLine (hdr);
			}				

		}

		/*
		 *  Read raw egf data and write to excel type file
		 * There are a lot of players from RO Drob who are fake 
		 */

		public void FormatMasterEGF()
		{
			string tTest = "PIN          Name                              Club     Grade P&D";
			bool faci = false;	
			string egfCopy = "/Users/iandavis/egf.tsv";
			if(File.Exists(egfCopy));
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
			
	}
}

