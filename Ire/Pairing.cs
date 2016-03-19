using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    class Pairing
    {
        protected Player white;
        protected Player black;
        protected int setting = 0;
        protected int result = 0;
		static int maxHandi;
		static int defaultPolicy;

		public void setStatics (int _maxHandi = 9, int _defPolicy = -1)
		{
			maxHandi = _maxHandi;
			defaultPolicy = _defPolicy;
		}

		public Pairing( Player a, Player b)
		{
			white = a;
			black = b;
		}

        public Pairing( Player a, Player b, int _handicap, int _result)
        {
            white = a;
            black = b;
			setting = _handicap;
            result = _result;
        }

        public Pairing(Player a, Player b, int HandiPolicy, bool HandiAboveBar)
        {
            Random r = new Random();
            int coin = r.Next(0, 2) - 1;
            float rawDiff = a.getMMS() - b.getMMS();
            rawDiff = (rawDiff * rawDiff) / rawDiff;
            white = a; //save time with default assignment
            black = b;
            if (HandiPolicy == -1) //no handicap
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
                if(rawDiff>HandiPolicy)
                {
                    if (HandiAboveBar)
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
                            setting = (int)(rawDiff - HandiPolicy);
                        }
                    }
                    else //handicap allowed above bar
                    {
                        if (a.getMMS() < b.getMMS())
                        {
                            white = b;
                            black = a;
                        }
                        setting = (int)(rawDiff - HandiPolicy);
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
           
        }

        public void ChangeSetting(int s)
        { setting = s; }

        public void SetResult(int r)
        { result = r; }

        public string BasicOutput()
        {
            string s;
            switch (result){
                case 0:
                default:
                    s = "    ?:?   "; break;
                case 1:
                case 4:
                     s = "   1:0   ";  break;
                case 2:
                case 5:
                     s = "   0:1   ";  break;
                case 3:
                case 6:
                    s = " 0.5:0.5 "; break;
                case 7:
                    s = "   0:0   "; break;
            }
            return white.ToString() + s + black.ToString() + " h"+setting;
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
			if (p.white == white)
			if (p.black == black)
				return true;
			if (p.white == black)
			if (p.black == white)
				return true;
			return false;
		}
    }
}
