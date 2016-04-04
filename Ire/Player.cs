using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    public class Player : Person, IComparable //Inheritance probably useless
    {
		#region variable
		protected float MMS = -1;  
        protected float[] score;
		protected bool[] participation;
		protected int[] opponent;
		protected int[] handi;
		protected int[] BlackWhite; //Black0White1
        public bool topBar = false; //potentially no handicap above bar
        public float SOS = -1;
		public float MOS = -1; //Middle portion of SOS
		protected int eRating; // effective rating, used for lower bar
		private int EGDPin;
		public int Deed = -1; //Deed is the draw seeding for a particular round

		private static bool BreakBySOS = true;
		private static bool BreakByMOS = true;
		public static bool SortByRating = false;
		#endregion
		//this is here just to allow compilation TODELETE
		public Player(int _seed, string _nom="null", int _rat=-1, string _club="null", string _ctry="null")
			: base (_nom="null", _rat=-1, _club="null", _ctry="null" )
		{Seed = _seed;}

		public Player(int _seed, string _nom , int _rat, string _ctry, string _club, bool[] par) 
			: base (_nom, _rat, _club, _ctry )
        {
			Seed = -1; //When we first read Players in they are not sorted
			EGDPin = _seed;
			eRating = _rat;
			//participation does not exist
			participation = new bool[par.Length];
			score = new float[par.Length];
			BlackWhite = new int[par.Length];
			handi = new int[par.Length];
			opponent = new int[par.Length];
		//	Console.WriteLine ("Participation length "+par.Length);
			for(int i=0; i<par.Length; i++)
				participation[i]=par[i];
		}

        public void setPlayer(string nom, string cc, string g, int _rat, int tr, int p)
        {
            Name = nom;
            Country = cc;
            Rank = g;
            Rating = _rat;
            score = new float[tr];
            participation = new bool[tr];
            opponent = new int[tr]; //positive=player //negative=bye //0=?
			Seed = p;
			eRating = _rat;

            for (int ii = 0; ii < tr; ii++)
            {
				participation[ii]=true;
				opponent[ii] = 0;
				handi[ii] = 0;
				BlackWhite[ii] = 0;
                score[ii] = -1;
            }
        }
        //Allows Override
		public void setResult(int rnd, int op, float sc)
		{
			score[rnd] = sc; opponent[rnd] = op;
		}

		//This is the correct method
		public void setResult(int rnd, int op, float _score, int _handicap=0, int BW =1)
		{
            rnd--; //0 based arrary as always
			score[rnd] = _score;
			opponent[rnd] = op;
			handi[rnd] = _handicap;
			BlackWhite[rnd] = BW;
			setMMS(getMMS ());
		}

		public void AssignBye(int _rnd)
		{
			Console.WriteLine ("AssignBye rnd " + _rnd + " par.L" + participation.Length);
			participation[_rnd] = false;
		}

        public float getMMS()
        {
            float f = 0;
			if(opponent!=null)
            for (int i = 0; i < opponent.Length; i++)
                f += score[i];
            f += MMS;
                return f;
        }

        public float getScore(int rnd)
        {
            float f = 0;
            for (int i = 0; i < rnd; i++)
                f += score[i];
            return f;
        }
        public float getMMS(int rnd)
        {
            float f = getScore(rnd);
            f += MMS;
            return f;
        }
		#region SOS and MOS Calculation
		public int getOpponent(int i)
		{
			return opponent[i];
		}
		public int getAdjHandi(int i)
		{
			//if black substract handicap , if White add handicap to SOS
			return handi[i] * BlackWhite[i];
		}
		//SOS is public?
		#endregion

        public void setOpponent(int i, int rnd)
        {
            opponent[rnd] = i;
        }
        public bool getParticipation(int i)
        {
		//	Console.WriteLine (participation.Length);
            return participation[i];
        }
        public int nBye()
        {
            int n =0;
            for (int i = 0; i < participation.Length; i++)
                if (!participation[i])
                    n++;
            return n;
        }
        public string getName()
        {
            return Name;
        }
        public int getRating()
        {
            return Rating;
        }
		public int getERating()
		{
			return eRating;
		}
		public void setERating(int _rating)
		{
			eRating = _rating;
		}
        public void setMMS(float s)
        {
            MMS = s;
        }
        public int getSeed()
        { return Seed; }
        public void setTop()
        { topBar = true; }

		#region Override Methods
		public override int GetHashCode ()
		{ //Evil grin
			return 11; 
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
				return false;
			try{
				Player p = (Player) obj;
				if(Seed>0)
				{
					if(Seed==p.Seed)
						return true;
				}else
				{
					if(EGDPin==p.EGDPin)
					return true;
				}
			}
			catch(Exception e) {
				return false;
			}
			return false;
		}
		#endregion

        public int CompareTo(Object o)
        {
            Player p = (Player)o; // how gross is this?
			if (SortByRating) {
				if (p.eRating > eRating)
					return 1;
				if (p.eRating == eRating)
					return 0;
				return -1;
			}
            if (p.MMS > MMS)
                return 1;
            if (p.MMS == MMS)
            {   
				if (BreakBySOS == true) {
					if (p.SOS > SOS)
						return 1;
					if (p.SOS == SOS) {
						if (BreakByMOS) {
							if (p.MOS > MOS)
								return 1;
							if (p.MOS == MOS)
								return 0;
							return -1;
						} else
							return 0;
					}

					//neither
					return -1;
				} else {	
					if (BreakByMOS) {
						if (p.MOS > MOS)
							return 1;
						if (p.MOS == MOS)
							return 0;
						return -1;
					} else //no tiebreaker
						return 0;
				}
            }

            return -1;
        }
		#region output
        public override string ToString()
        {
			return Name + " " + eRating + "("+MMS+")";
        }

		public string ToFile()
		{
			char[] c = { ' '};
			string[] split = Name.Split (c);
			if(split.Length ==2)
				return split[0]+"."+split[1].Substring(0,1).ToUpper() + "(" + Seed + ")";
			else
				return split[0] + "(" + Seed + ")";
		}

        public string ToStanding(int rnd)
        {// sw.WriteLine(cnt + t + ap.ToFile() + t + ap.Rank + t + ap.Rating + t + );
            string s = ToFile() ;
            s = s + "\t(" + Rank + ")\t(" + Rating + ")\t";
            s = s + getMMS(rnd) + "\t" + getScore(rnd) + "\t";
            for(int i=0; i<rnd; i++)
            {
                s = s + opponent[i];
                if (score[i] == 0) s = s + "-";
                if (score[i] == 1) s = s + "+";
                if (score[i] == 0.5) s = s + "=";
            }
            return s;
        }
		#endregion
    }


}
