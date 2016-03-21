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
            // Ask if you want to update the Rating list
            // Ask for details about the tournament (final)
            // Start entering players (late arrivals can be added afterwards)
            // request confirmation on bar groups
            // ask for top bar : show preliminary assign
            // ask for low bar : show preliminary assign
            // ask if want to remove from top bar = *bye
            // make draw for first round
            // - any late players? read from file
            // - do you want to set a bye? read from file
            // make pairing
            // -output file
            //read results 
            //sort players
            //loop until tournament end

            TournamentBoss tb = new TournamentBoss(true);
			tb.GenerateTemplateInputFile ();
			tb.GeneratePlayers (21); // this works fine
			tb.ReadPlayers();
			tb.SortField(true);
			tb.ShowField ();
			Console.WriteLine ("Ready to read in Settings File?");
			string s = Console.ReadLine();//wait	
			tb.ReadSettings ();
			tb.previewTopBar ();
			tb.previewFloor ();
			tb.previewTopBar (true);
			tb.previewFloor (true);
			//now we can start the tournament
			tb.MakeDraw();

			/*
			i.DownloadMasterZipEGF ();
            i.GenerateTemplate();
			Console.ReadLine ();
            Turn t = new Turn(i._Round);
            Console.WriteLine("importing players 1...");
            List<Player> pImport = i.ReadPlayers();
            foreach (Player pp in pImport)
                Console.WriteLine(pp.ToString());
            Console.WriteLine("importing players 2...");
            t.AddPlayersFromImport(pImport);
			t.DebugPlayers ();
            t.SetBar();
            t.MakePairing();
*/
        }
    }
}
