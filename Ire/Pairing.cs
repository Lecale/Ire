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

        public Pairing( Player a, Player b, int s, int r)
        {
            white = a;
            black = b;
            setting = s; //handicap
            result = r;
        }

        public Pairing(Player a, Player b, int HandiPolicy, bool HandiAboveBar)
        {
            float rawDiff = a.getMMS() - b.getMMS();
            rawDiff = (rawDiff * rawDiff) / rawDiff;
            white = a;
            black = b;
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
            return "" + white.getPin() + black.getPin();
        }

        string[] res = {"None","WWin","BWin","Draw","WAdj","BAdj","DAdj","LAdj"};
        
    }
}
