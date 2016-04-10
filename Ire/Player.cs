using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
	public class Player : Person, IComparable<Player> //Inheritance probably useless
    {
		#region variable
        public float initMMS = -1;
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

		private static List<string> Tiebreaker = new List<string> ();
		public static bool SortByRating = false;
		#endregion

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
        #region ResultsByesMMS

		public void setResult(int rnd, int op, float _score, int _handicap=0, int BW =1)
		{
            rnd--; //0 based arrary as always
			score[rnd] = _score;
			opponent[rnd] = op;
			handi[rnd] = _handicap;
			BlackWhite[rnd] = BW;
			//setMMS(getMMS (rnd));
			setMMS(getMMS ());
			}

		public void AssignBye(int rnd)
		{
            rnd--; //0 based arrary as always
		//	Console.WriteLine ("AssignBye rnd " + rnd + " par.L" + participation.Length);
			participation[rnd] = false;
            score[rnd] = 0.5f;
            setMMS(getMMS(rnd));
		}

        public float getMMS()
        {
            float f = initMMS;
			if(opponent!=null)
            for (int i = 0; i < opponent.Length; i++)
                f += score[i];
           // f += MMS;
           return f;
        }

        public float getScore(int rnd)
        {
            float f = 0;
            for (int i = 0; i < rnd; i++)
                f += score[i] ;
            return f;
        }
        public float getMMS(int rnd)
        {
            return initMMS + getScore(rnd);
        }
        public void setMMS(float s)
        {
            MMS = s;
        }
        public void setInitMMS(float s)
        {
            initMMS = s;
        }
#endregion

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

        public int getSeed()
        { return Seed; }
        public void setTop()
        { topBar = true; }

		#region Override Methods
//		public override int GetHashCode ()
//		{ //Evil grin
//			return 11; 
//		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
				return false;
			try{
				Player p = (Player) obj;

			//	Console.WriteLine ("Equals()" + EGDPin + "," + p.EGDPin);
				if(Seed>0)
				{
					if(Seed==p.Seed)
						return true;
				}
				//else //EGDPin is also a unique key
				//{
					if(EGDPin==p.EGDPin)
					return true;
				//}
			}
			catch(Exception e) {
				return false;
			}
			return false;
		}
		#endregion

		public static void SetTiebreakers(List<string> _tie)
		{
			Tiebreaker = _tie;
		}
		public int CompareTo(Player p)
        {
//            Player p = (Player)o; // how gross is this?
			if (SortByRating) {
				if (p.eRating > eRating)
					return 1;
				if (p.eRating == eRating)
					return 0;
				return -1;
			}
		//	Console.WriteLine ("Sorting by McMahonites" + MMS + " p." + p.MMS);
			//Sort by MMS
            if (p.MMS > MMS)
                return 1;
		//	Console.WriteLine ("Sorting by McMahonites" + MMS + " p." + p.MMS + "did not return 1");
			if (p.MMS == MMS) {
				foreach(string tie in Tiebreaker)
				{
					if(tie.Equals("SOS"))
					{
						if(p.SOS > SOS)
							return 1;
						if(p.SOS < SOS)
							return -1;	
					}if(tie.Equals("MOS"))
					{
						if(p.MOS > MOS)
							return 1;
						if(p.MOS < MOS)
							return -1;	
					}if(tie.Equals("SODOS"))
					{
/*						
*/					}
				}
				return 0;
			}
            return -1;
        }

		#region output
        public override string ToString()
        {
            return Name + " " + eRating + "(" + MMS + ")";
        }

        public string ToDebug()
        {
			return ToString() + ".IM."+ initMMS + " S(" + Seed + ")";
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
        {
            string s = ToFile() ;
            s = s + "\t(" + Rank + ")\t(" + Rating + ")\t";
            s = s + getMMS(rnd) + "\t" + getScore(rnd) + "\t";
            for(int i=0; i<rnd; i++)
            {
                s = s + opponent[i];
                if (score[i] == 0) s = s + "-\t";
                if (score[i] == 1) s = s + "+\t";
                if (score[i] == 0.5) s = s + "=\t";
            }
            return s;
        }

        public string ToEGF()
        {// http://europeangodatabase.eu/EGD/EGF_rating_system.php#Submissions 
            string s = Name + " " + Rank + " " + Country + " " + Club + " "; //EGD identifiers
            for (int i = 0; i < opponent.Length; i++)
            {
                s = s + opponent[i];
                if (score[i] == 0) s = s + "-h" + getAdjHandi(i) + " ";
                if (score[i] == 1) s = s + "+h" + getAdjHandi(i) + " "; 
                if (score[i] == 0.5) s = s + "=h" + getAdjHandi(i) + " ";  
            }
            return s;
        }
		#endregion
    }


}
