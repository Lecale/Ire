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
#endregion
        #region Export Functions
        #endregion
    }
}
