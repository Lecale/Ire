using System;
using System.Collections.Generic;

namespace Ire
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].Equals("SRCH"))
                {
                    List<string> etsr = new List<string>();
                    for (int i = 1; i < args.Length; i++)
                        etsr.Add(args[i]);
                    Utility u = new Utility();
                    u.egfTextSearchResults(etsr);
                }                
            }

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
                tb.ReadPlayers(true,true);
                tb.SortField(true);
				Console.Clear();
				Console.WriteLine("Now please complete your data in the Settings File.");
                awaitText("When you have finished type 'done'", true, "done");
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
                tb.GenerateFinalStandingsFile(rounds);  //Experimental !
            tb.GenerateEGFExport();
            tb.ConvertStandingsToHTML(rounds);

			Console.Clear();
			Console.WriteLine("The tournament has ended.");
            Console.WriteLine("We hope you enjoyed using Ire to make the pairings.");
        }

        public static void awaitText(string instruction, bool clearConsole = true, string keyPhrase = "done")
        {
            bool awaiting = true;
            while (awaiting)
            {
                Console.WriteLine(instruction);
                if (keyPhrase.ToLower().Equals(Console.ReadLine()))
                    awaiting = false;
            }
            if (clearConsole) Console.Clear();
        }
    }
}
