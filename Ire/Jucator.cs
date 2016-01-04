using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    class Jucator : IComparable
    {
        public GameCollection gc;
        public int Seed = -1;
        int rating = -1;
        string name = "unset";
        string club = "unset";
        string country = "unset";
        float iMMS = -1;
        float score = -1;
        public int[] position;
        bool[] participation;
        int pin;
        float SOS =-1; //normal tiebreaker
        float MOS =-1; //Most Opponents Scores remove Top and Bottom
        bool iAboveBar = false;

        public Jucator()
        { }

        public Jucator(int _pin, string _nom , int _rat, string _ctry, string _club, bool[] _par) 
        {
			pin = _pin;
			name = _nom;
			rating = _rat;
			country = _ctry;
			club = _club;
			participation = new bool[_par.Length];
			for(int i=0; i<_par.Length; i++)
				participation[i]=_par[i];
            gc = new GameCollection(_par.Length);
		}

        public void SetiMMS(float _iMMS)
        {
            iMMS = _iMMS;
        }
        public float GetMMS()
        {
            return iMMS + gc.getScore();
        }

        public float GetMMS(int _rnd)
        {
            return iMMS + gc.getScoreAt(_rnd);
        }

        public bool IsInRound(int _rnd)
        {
            return participation[_rnd];
        }

        public void RecordPairing(int _rnd, int _hnd, bool _isW, int _res = 0, int _rem = 0)
        {
            gc.setCondition(_rnd, _hnd, _isW, _res, _rem);
        }

        public void RecordResult(int _rnd, int _res, int _rem = 1)
        { 
            gc.setResult(_rnd, _res, _rem);
        }

        public bool HasPlayed(int _pin)
        {
            return gc.hasPlayed(_pin);
        }

        public int GetOpponentAt(int _rnd)
        { 
            return gc.pin[_rnd];
        }
        public int[] GetOpponents()
        {
            return gc.pin;
        }
        public void SetSOS(float _SOS)
        { SOS = _SOS; }
        public void SetMOS(float _MOS)
        { MOS = _MOS; }
        //Use for SOS calculation , be careful about sense
        //jucator is white, his SOS contribution is inflated and must be reduced
        public int getAdjHandi(int _rnd)
        {
            return gc.HdcpAdjustment(_rnd);
        }

        public int CompareTo(object obj)
        {
            Jucator p = (Jucator)obj; // yuck

            if (p.iMMS + p.gc.getScore() > iMMS + gc.getScore() )
                return -1;
            if (p.SOS > SOS)
                    return -1;
            if (p.MOS > MOS)
                    return -1;
            return 0;
        }

        public override string ToString()
        {
            return name + "(" + Seed + ")" + rating;
        }

        public string DebugInformation()
        {
            return "";
        }

        public void SetTopGroup()
        { iAboveBar = true; }

        public bool StartedAboveBar()
        { return iAboveBar; }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            try
            {
                Jucator p = (Jucator)obj;
                if (pin == p.pin)
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return false;
        }
    }
}
