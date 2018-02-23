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
		private string[] end = {"better","lower","water","later","son","morn","mane","ston","crake","flute","pie","cake"};

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
            double d2 = r.NextDouble();
            double d = d1 * d2;
            double plusMinus = r.NextDouble();
            if (plusMinus > 0.5)    return (int)((width * d) + mid);
            else   return (int)( mid - (width * d));
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

        //Utility::Search by EGD Pin or Country
        public void egfTextSearchResults(List<string> tokens)
        {
            Console.WriteLine("Instead of running a tournament");
            Console.WriteLine("we are going to search the 'egf.txt' file");
            Console.WriteLine("please enter the relative directory path for the file");
            int pin = -1;
            string fN = "";
            string lN = "";
            string tmp = "";
            
            string dirpath = Console.ReadLine();
            if(dirpath.StartsWith("\\"))
                dirpath = Directory.GetCurrentDirectory() + dirpath ;
            else
                dirpath = Directory.GetCurrentDirectory() + "\\" + dirpath;
            try
            {
                pin = int.Parse(tokens[0].Trim());
                Console.WriteLine("searching for egd pin {0}", pin);
            }
            catch
            {
                fN = tokens[0].ToLower().Trim(); lN = tokens[1].ToLower().Trim();
                Console.WriteLine("searching for string {0} , {1}", fN, lN);
            }
            Console.WriteLine(pin);
            int Matches = 0; int Tries = 0;
            using (StreamReader sr = new StreamReader(dirpath + "\\egf.txt"))
            {
                using (StreamWriter sw = new StreamWriter(dirpath + "\\searchResults.txt"))
                {
                    if (pin != -1)
                    {
                        while (sr.EndOfStream == false)
                        {
                            Tries++;
                            tmp = sr.ReadLine();
                            if (tmp.Contains(pin.ToString()))
                            {
                                sw.WriteLine(tmp);
                                Matches++;
                            }
                        }
                        sw.Flush();
                    }
                    else
                    {
                        while (sr.EndOfStream == false)
                        {
                            Tries++;
                            tmp = sr.ReadLine().ToLower();
                            if (tmp.Contains(fN) && tmp.Contains(lN))
                            {
                                sw.WriteLine(tmp);
                                Matches++;
                            }
                        }
                        sw.Flush();
                    }
                }
            }
            Console.WriteLine("Checked {0} Matches {1}",Tries,Matches);
            Environment.Exit(0);
        }
	}
}

