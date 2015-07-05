using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    class Turn
    {
        protected Round[] _rounds;
        protected List<Player> _players = new List<Player>();
        protected List<Group> _groups = new List<Group>(); //use for pairing
        protected List<Bye> _byes = new List<Bye>(); //dirty way to track
        protected string STATE = "NULL";
        protected int CUR_Round = -1;

        protected bool highBar = false;
        protected bool lowBar = false;
        protected int highRating = 0;
        protected int lowRating = 9000;
        protected int RatingBar = -1; //set via console
        protected int RatingFloor = -1;

        private List<string> PlayedPairings;
        private List<string> BlockedPairings;
        private List<string> TempPairings;

        public Turn(int n)
        {
            _rounds = new Round[n];
            PlayedPairings = new List<string>();
            BlockedPairings = new List<string>();
            TempPairings = new List<string>();
        }

        // of course his details are read in via file!
        public void AddLatePlayer(Player p)
        {
            _players.Add(p);
            int rat = p.getRating();
            int s = RatingBar - rat;
            s = s / 100;
            float SMMS = 0;
            if (rat >= RatingBar)
            {
                SMMS = 99 + ((float)0.5 * CUR_Round);
                AcceptDeclineSuggestedRating(SMMS, p);
            }
            if (rat <= RatingFloor)
            {
                s = RatingBar - RatingFloor;
                s = s / 100;
                SMMS = 99 - s;
                SMMS = SMMS + ((float)0.5 * CUR_Round);
                AcceptDeclineSuggestedRating(SMMS, p);
            }
            if (rat < RatingBar && rat > RatingFloor)
            {
                SMMS = 99 - s;
                SMMS = SMMS + ((float)0.5 * CUR_Round);
                AcceptDeclineSuggestedRating(SMMS, p);
            }
        }
        public void AcceptDeclineSuggestedRating(float i, Player p)
        {
            Console.WriteLine();
            Console.WriteLine("Late Player is " + p.getName() + " " + p.getRating());
            Console.WriteLine("Suggested starting MMS is " + i);
            Console.WriteLine("Accept - enter y");
            Console.WriteLine("Decline enter MMS eg 42");
            string s = Console.ReadLine().ToUpper();
            if (s.StartsWith("Y"))
                p.setMMS(i);
            else {
                try
                {
                    float f = float.Parse(s);
                    p.setMMS(f);
                }
                catch (Exception e)
                { 
                    Console.WriteLine("Error reading input: " + s);
                    AcceptDeclineSuggestedRating(i, p);
                }
            }
            Console.WriteLine();
        }

        //call after every round
        public void sortPlayers()
        {
            _players.Sort();
        }

        public void previewTopBar()
        { 
            int tCount = 0;
            foreach (Player peter in _players)
            {        
                if (highRating < peter.getRating())
                        highRating = peter.getRating();
                if (peter.getRating() > RatingBar)
                {
                    Console.WriteLine(peter.getName() + " " + peter.getRating());
                    tCount++;
                }
            }
            Console.WriteLine("Provisionally " +tCount +" in top group");
        }

        public void previewFloor()
        {
            int tCount = 0;
            foreach (Player peter in _players)
            {
                if (peter.getRating() < RatingFloor)
                {
                    Console.WriteLine(peter.getName() + " " + peter.getRating());
                    tCount++;
                }
            }
            Console.WriteLine("Provisionally " + tCount + " in bottom group");
        }

        public void InitMMS() 
        {
            if (highBar)
            {
                for (int i = 0; i < _players.Count; i++)
                    if (_players.ElementAt(i).getRating() >= RatingBar)
                    {   //does he have a bye -> manual
                        _players.ElementAt(i).setTop();
                        _players.ElementAt(i).setMMS(100);
                    }

            }
            else 
            {
                foreach (Player peter in _players)
                    if (highRating < peter.getRating())
                        highRating = peter.getRating();
                RatingBar = highRating - 100; //or STEP
            }
            //Set initial MMS for everyone else
            foreach (Player peter in _players)
            {
                if (!peter.isTop())
                {
                    int rat = peter.getRating();
                    if (rat >= RatingBar) //no top group
                        peter.setMMS(100);
                    else 
                    {
                        int step = RatingBar - rat;
                        step = step / 100;
                        step++;
                        peter.setMMS(100 - step);
                    }
                    if (rat <= lowRating) //do we need this
                        lowRating = rat;
                }
            }
            if (lowBar)
            {
           //     int step = RatingBar - lowRating;
                int step = RatingBar - RatingFloor;
                step = step / 100;
                step++;
                foreach (Player peter in _players)
                {
                    if (peter.getRating() <= RatingFloor)
                    {  
                        peter.setMMS(step);
                    }
                }
            }
           
        }

        public void MakePairing()
        {
            List<Player> byes = new List<Player>();
            List<Player> paired = new List<Player>();
            List<Player> unpaired = new List<Player>();
            //Find n_players participating
            foreach(Player p in _players)
            {
                if (!p.getParticipation(CUR_Round))
                {
                    byes.Add(p);
                }
            }
            //if odd, find Bye_assignee
            if (_players.Count - byes.Count / 2 == 1)
            {
                int i = assignBye();
                Bye b = new Bye(_players.ElementAt(i).getMMS(), CUR_Round, _byes.Count);
                _byes.Add(b);
                _players.ElementAt(i).setResult(CUR_Round, -1 , (float)0.5);
            }
            //go top down
            
        }

        public void MakeGroups(int rnd)
        {
            float gLog = -1;
            _groups.Clear();
            //create all the groups
            foreach (Player p in _players)
            {
                if (p.getParticipation(rnd - 1))
                {
                    if (p.getMMS() != gLog)
                    {
                        _groups.Add(new Group(p.getMMS()));
                        gLog = p.getMMS();
                    }
                }
            }
            int posn = 0; //add all the players
            foreach (Player p in _players)
            {
                if (p.getParticipation(rnd - 1))
                {
                    if (p.getMMS() == _groups.ElementAt(posn).MMS)
                    {
                        _groups.ElementAt(posn).add(p.getPin() );
                    }
                    else 
                    {
                        posn++;
                        _groups.ElementAt(posn).add(p.getPin());
                    }
                }
            }
           
        }

        // Give bye to lowest n.byes, then lowest ranked
        public int assignBye()
        {
            int found = -1;
            for (int level = 1; level < _rounds.Length; level ++ )
                for (int i = _players.Count; i > 0; i--)
                {
                    if (_players.ElementAt(i - 1).nBye() < level)
                    {
                        found = i - 1;
                        return found;
                    }
                }
            //emergency
            return _players.Count -1;
        }


    }
}
