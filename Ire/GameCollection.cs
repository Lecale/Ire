using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    class GameCollection
    {
        int[] pin;
        bool[] isWhite;
        int[] handicap;
        int[] result;
        int[] remarks;
        // 0 unplayed 1 normal 2 adjudicated 3 forfeit

        public GameCollection(int _NumRounds)
        {
            pin = new int[_NumRounds];
            handicap = new int[_NumRounds];
            result = new int[_NumRounds];
            isWhite = new bool[_NumRounds];
            remarks = new int[_NumRounds];

            for (int i = 0; i < _NumRounds; i++)
            {
                pin[i] = -1;
                handicap[i] = 0;
                result[i] = 0;
                isWhite[i] = false;
                remarks[i] = 0;
            }
        }

        public int getScore()
        {
            int gt = 0;
            for (int i = 0; i < result.Length; i++)
                if (remarks[i] != 0)
                    gt += result[i];
            return gt;
        }

        public void setResult(int _rnd, int _res, int _rem = 1)
        {
            result[_rnd] = _res;
            remarks[_rnd] = _rem;//
        }

        public void setCondition(int _rnd, int _hnd, bool _isW, int _res = 0, int _rem = 0)
        {
            handicap[_rnd] = _hnd;
            isWhite[_rnd] = _isW;
            result[_rnd] = _res;//
            remarks[_rnd] = _rem;//
        }

        public void TestOutput()
        { }
    }
}
