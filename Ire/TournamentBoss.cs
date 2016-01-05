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
         * Pin tName tRating tClub tCountry
         * This method has a GIGO ideology
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
#endregion
        #region Export Functions
        #endregion
    }
}
