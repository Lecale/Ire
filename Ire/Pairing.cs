using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    public class Pairing
    {
        public Player white;
        public Player black;
        protected int setting = 0;
        protected int result = 0;
		static int maxHandi;
		static int defaultPolicy;
		static bool handAboveBar = false;

		public static void setStatics (int _maxHandi = 9, int _defPolicy = -1, bool _handAboveBar=false)
		{
			maxHandi = _maxHandi;
			defaultPolicy = _defPolicy;
			handAboveBar = _handAboveBar;
		}

		//should contain some kind of sneaky bye allocation thing
        public Pairing( Player a, Player b, int _handicap, int _result)
        {
            white = a;
            black = b;
			setting = _handicap;
            result = _result;
        }

        public Pairing(Player a, Player b)
        {
            Random r = new Random();
            int coin = r.Next(0, 2) - 1;
            float rawDiff = a.getMMS() - b.getMMS();
            rawDiff = (rawDiff * rawDiff) / rawDiff;
            white = a; //save time with default assignment
            black = b;
            if (defaultPolicy == -1) //no handicap
            {//coin flip
                if (coin > 0)
                {
                    white = b;
                    black = a;
                }
            }
            else
            { /*handicap_n n=adjustment
               * n=0 handi from rawDiff=1
               * n=1 handi from rawDiff=2 etc
               * except if aboveBar stuff is in place
               * Remember 1.5 is treated as 1 according to tradition
               */
				if(rawDiff>defaultPolicy)
                {
					if (handAboveBar==false)
                    {
                        if (a.topBar || b.topBar)
                        { //somebody abovebar flipcoin
                            if (coin > 0)
                            {
                                white = b;
                                black = a;
                            }
                        }
                        else
                        {
                            if (a.getMMS() < b.getMMS())
                            {
                                white = b;
                                black = a;
                            }
							setting = (int)(rawDiff - defaultPolicy);
                        }
                    }
                    else //handicap allowed above bar
                    {
                        if (a.getMMS() < b.getMMS())
                        {
                            white = b;
                            black = a;
                        }
						setting = (int)(rawDiff - defaultPolicy);
                    }
                }
                else
                {//coin flip
                    if (coin > 0)
                    {
                        white = b;
                        black = a;
                    }
                }
            }
			if (maxHandi != -1)
			if (setting > maxHandi)
				setting = maxHandi;
        }

		public int GetHandi(){
			return setting;
		}
        public void ChangeHandi(int s)
        { setting = s; }

        public void SetResult(int r)
        { result = r; }

        public string BasicOutput()
        {
            string s;
            switch (result){
                case 0:
                default:
                    s = "?:?"; break;
                case 1:
                case 4:
                     s = "1:0";  break;
                case 2:
                case 5:
                     s = "0:1";  break;
                case 3:
                case 6:
                    s = "0.5:0.5"; break;
                case 7:
                    s = "0:0"; break;
            }
			return white.ToString() + "\t" + s + "\t" + black.ToString() + "\t" + " h"+setting;
        }

		public string ToFile()
		{
			string s;
			switch (result) {
			case 0:
			default:
				s = "?:?";
				break;
			case 1:
			case 4:
				s = "1:0";
				break;
			case 2:
			case 5:
				s = "0:1";
				break;
			case 3:
			case 6:
				s = "0.5:0.5";
				break;
			case 7:
				s = "0:0";
				break;
			}
			return white.ToFile() + "\t" + s + "\t" + black.ToFile() + "\t" + " h"+setting;
		}

		public float WhiteScore()
		{
			if (result == 3 || result == 6)
				return 0.5f;
			if (result == 1 || result == 4)
				return 1;
			if (result == 5 || result ==2||result ==7)
				return 0;
			Console.WriteLine("No result yet");
			return - 1;
			
		}

		public float BlackScore()
		{if (result == 3 || result == 6)
			return 0.5f;
			if (result == 1 || result == 4)
				return 0;
			if (result == 5 || result ==2||result ==7)
				return 1;
			Console.WriteLine("No result yet");
			return - 1;
		}

        //quick way to check
        public string Key()
        {
         //   return "" + white.getPin() + black.getPin();
			return "NotInUse";
        }

        string[] res = {"None","WWin","BWin","Draw","WAdj","BAdj","DAdj","LAdj"};
        
		public override bool Equals (object obj)
		{
			Pairing p = (Pairing)obj;
			if (p.white.getSeed() == white.getSeed())
			if (p.black.getSeed() == black.getSeed())
				return true;
			if (p.white.getSeed() == black.getSeed())
			if (p.black.getSeed() == white.getSeed())
				return true;

			if (p.white.EGDPin == white.EGDPin)
			if (p.black.EGDPin == black.EGDPin)
				return true;
			if (p.white.EGDPin == black.EGDPin)
			if (p.black.EGDPin == white.EGDPin)
				return true;
			return false;
		}
    }
}
