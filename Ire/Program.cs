using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    class Program
    {
        static void Main(string[] args)
        {

            TournamentBoss tb = new TournamentBoss(true);
            Console.WriteLine("Do you need to download the EGF Rating List? (yes/no)");
            string s = Console.ReadLine();
            if (s.ToUpper().StartsWith("Y"))
            {
                tb.DownloadEGFMasterZip();
            }
			tb.GenerateTemplateInputFile ();
			tb.GeneratePlayers (); // Cut to become auto
			tb.ReadPlayers();
			tb.SortField(true);
			tb.ShowField ();
			Console.WriteLine ("Press any key to read in the Settings File");
			s = Console.ReadLine();//wait	
			tb.ReadSettings ();
			tb.previewTopBar (true);
			tb.previewFloor (true);
            int rounds = tb.nRounds;
			//now we can start the tournament
            for (int i = 1; i < rounds+1; i++)
            {
                tb.MakeDraw(i);
                tb.ReadResults(i);
          //      tb.ShowField("Round" + i + "AfterReadResults"); //just a debug method
                tb.ProcessResults(i);
			//	tb.ShowField("Round" + i + "AfterProcess"); //just a debug method
                tb.UpdateTiebreaks(i);
                tb.SortField();
           //     tb.ShowField("Round" + i + "AfterSortField"); //just a debug method
                tb.GenerateStandingsfile(i);
            }
            tb.GenerateEGFExport();
            tb.ConvertStandingsToHTML(rounds);

            Console.WriteLine("The tournament has ended.");
            Console.WriteLine("We hope you enjoyed using Ire to make the pairings.");
            Console.WriteLine(RandomAdvice());
        }

        static string RandomAdvice()
        {
            Random d = new Random();
            List<string> FortuneCookie = new List<string>();
            FortuneCookie.Add("Go home and rest.");
            FortuneCookie.Add("Go out and party.");
            FortuneCookie.Add("Go forth and conquer.");
            FortuneCookie.Add("Now it is time to eat.");
            FortuneCookie.Add("Consider the Lily.");
            FortuneCookie.Add("Have you ever thought about changing your life?");
            FortuneCookie.Add("The way back to Earth has been wiped from my memory.");
            FortuneCookie.Add("Leave no prisoners behind.");
            FortuneCookie.Add("Tomorrow is a new day.");
            FortuneCookie.Add("Dispatch war rocket Ajax to collect his body.");
            
            return "";
        }
    }
}
