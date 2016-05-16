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
			foreach (string a in args)
				Console.WriteLine (a);
            TournamentBoss tb = new TournamentBoss(true);
			bool newT = tb.GenerateTemplateInputFile (); //set working directory
            int startRound = 1;
            if (newT)
            {
                Console.WriteLine("Do you need to download the EGF Rating List? (yes / no)");
                string s = Console.ReadLine();
                if (s.ToUpper().StartsWith("Y"))
                {
                    tb.DownloadEGFMasterZip();
                }
                tb.GeneratePlayers(); 
                tb.ReadPlayers(true);
                tb.SortField(true);
				Console.WriteLine("Now please complete your data in Settings File.");
				Console.WriteLine("When you have finished press return.");
                s = Console.ReadLine();//wait	
                tb.ReadSettings();
                tb.previewTopBar(true);
                tb.previewFloor(true);
            }
            else 
            {
                startRound = tb.RestoreTournament();
            }
            int rounds = tb.nRounds;
			//now we can start the tournament
            for (int i = startRound; i < rounds + 1; i++)
            {
                tb.MakeDraw(i);
                tb.ReadResults(i);
                tb.ProcessResults(i);
                tb.UpdateTiebreaks(i);
                tb.SortField();
                tb.GenerateStandingsfile(i);
            }
            tb.GenerateEGFExport();
            tb.ConvertStandingsToHTML(rounds);

            Console.WriteLine("The tournament has ended.");
            Console.WriteLine("We hope you enjoyed using Ire to make the pairings.");
        }

    }
}
