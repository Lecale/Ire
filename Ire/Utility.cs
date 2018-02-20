using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Ire
{
	public class Utility
	{
		private Random r;
		private string[] fn = {"Al","Bob","Cal","Dar","Eoin","Fra","Ger","Hal","Io","Jo","Ken","Lor","Meg","Nim","Olli","Peg","Rach","Sue","Tony","Unt"};
		private string[] pre = {"O'","Mc","Ker","Ze","Van","Herr","Mac"};
		private string[] mid = {"Trah","Row","Land","Sea","Flack","Black","Whit","Red","Ash","Round","Tri","Green"};
		private string[] end = {"better","lower","water","later","son","morn","mane","ston","crake","flute,","pie","cake"};

		public Utility ()
		{
			r = new Random();
		}

		public string ProvideName()
		{
			string first = fn [r.Next (fn.Length)];
			string surname = pre [r.Next (pre.Length)] + mid [r.Next (mid.Length)] + end [r.Next (end.Length)];
			return surname + " " + first; 
		}

		public int RatingByBox(int mid, int width)
		{
			double d1 = r.NextDouble ();
			double d2 = r.NextDouble ();

			double d = (d1 * d2);
			if (r.NextDouble () > 0.5)
				d = d * -1;
			return (int)(d * width + mid);
		}

        public void EnterResults(string dir, int rnd)
        {
            List<string> allLines = new List<string>();
            using (StreamReader sr = new StreamReader(dir + "Round" + rnd + "Results.txt"))
            {
                while (sr.EndOfStream == false)
                    allLines.Add(sr.ReadLine());
            }
            using (StreamWriter sw = new StreamWriter(dir + "Round" + rnd + "Results.txt",false))
            {
                int counter = 0;
                foreach (string al in allLines)
                {
                    if (counter < 2)
                        sw.WriteLine(al);
                    else
                    {
                        if (r.NextDouble() < 0.45)
                        {
                            sw.WriteLine(al.Replace("?:?", "1:0"));
                        }
                        else
                        {
                            if (r.NextDouble() < 0.9)
                            {
                                sw.WriteLine(al.Replace("?:?", "0:1"));
                            }
                            else 
                            {
                                if (r.NextDouble() < 0.98)
                                    sw.WriteLine(al.Replace("?:?", "0.5:0.5"));
                                else
                                    sw.WriteLine(al.Replace("?:?", "0:0")); 
                            }
                        } 
                    }
                        counter++;
                }
            }

        }

        /*
         * Lets us initialise with the list from another tournament
         * 
         * {"retcode":"Ok","Pin_Player":"12913769","AGAID":"17693",
         * "Last_Name":"Davis","Name":"Ian","Country_Code":"IE",
         * "Club":"Belf","Grade":"1d","Grade_n":"30","EGF_Placement":"832",
         * "Gor":"2116","DGor":"0","Proposed_Grade":"","Tot_Tournaments":"73",
         * "Last_Appearance":"T170304F","Elab_Date":"2009-04-03","Hidden_History":
         * "0","Real_Last_Name":"Davis","Real_Name":"Ian"}
         * 
         */
        public void ImportTournamentFromEGDTextFile()
        {
            WebClient wc = new WebClient();
            string t;
            string pin="";
            string[] tmp;
            char[] c = { ',', ':' };
            string baseURL = "http://www.europeangodatabase.eu/EGD/GetPlayerDataByPIN.php?pin=";
           
            Console.WriteLine("Please enter the fully qualified filename");
            string egd = Console.ReadLine();
//          sw.WriteLine(i++ + "," + u.ProvideName() + "," + u.RatingByBox(midpoint, spread) + ",BLF,IE,1k" + end);
            int cntr = 1;
            using (StreamReader sr = new StreamReader(egd))
            {
                while (sr.EndOfStream == false)
                {
                    t = sr.ReadLine().Trim();
                    if ( t.StartsWith(";")== false) //check we are not having a comment
                    { 
                        //get pin
                        tmp = t.Split();    pin = tmp[tmp.Length-1]; 
                        //http request
                        t = wc.DownloadString(baseURL + pin).Replace("\"","");  Thread.Sleep(5000);
                        //dictionary
                        tmp = t.Split(c);
                        Dictionary<string, string> d = new Dictionary<string, string>();
                        for (int i = 0; i < tmp.Length; i += 2)
                            d.Add(tmp[i],tmp[i+1]);
                        //text file
                        t = "" + cntr + d["Last_Name"] + " " + d["Name"] + " ";
                        t += d["Gor"] + " " + d["Club"] + " " + d["Country"] + " " + d["Grade"];
                    }
                }
            }
        }

	}
}

