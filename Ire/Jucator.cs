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
        int Seed = -1;
        int rating = -1;
        string name = "unset";
        string club = "unset";
        string country = "unset";
        float iMMS = -1;
        float score = -1;
        int[] position;
        bool[] participation;
        int pin;
        int SOS; //normal tiebreaker
        int MOS; //Most Opponents Scores remove Top and Bottom

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

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
