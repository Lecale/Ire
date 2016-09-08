using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TB
{
    public class Rater
    {

        public float _A;
        public float _Con;

        public Rater()
        { }


        public float ObtainNewRating(float[] oppRating, float hisRating, float[] Results, bool Debug = false)
        {
            _A = ObtainAForRating(hisRating);
            _Con = ObtainConForRating(hisRating);
            float runningRating = hisRating;
            for (int i = 0; i < oppRating.Length; i++)
            {
                runningRating += ChangeInRating(oppRating[i], hisRating, Results[i]);
                if (Debug) Console.WriteLine(ChangeInRating(oppRating[i], hisRating, Results[i]));
            }
            return runningRating;
        }


        /*
                 * The expectation of winning is completely out of sync with reality
                 * whilst the EGD continually increases it
                 * Once we approach 2000 we see that it has reached plateau of around 40%
                 */
        public float ObtainAForRating(float rat)
        {
            float ourA = 200 + ((1900 - rat) * 0.1625f);
            if (ourA > 200) return 200f;
            if (ourA < 70) return 70f;
            return ourA;

        }

        /*
         * Deliberately induce a higher stability for the lower ratings with this simplistic function
         * instead of stretching from 120 to 10 we stretch from 60 to 10
         */
        public float ObtainConForRating(float rat)
        {
            float inverse = (2700 - rat) / 100;
            float ourCon = 10 + inverse + (float)(Math.Sqrt(Math.Pow(inverse, 2) - inverse));
            if (ourCon > 10f) return ourCon;
            return 10f;
        }

        /*
         * SE(A) = 1 / [eD/a + 1] - ε/2 
         * SE(A) + SE(B) = 1 - ε 
         * Rnew - Rold = con * [ SA - SE(D)] 
         * but since we do not need to adjust for inflation we can remove the ε
         */
        public float ChangeInRating(float opponent, float player, float result)
        {
            float expo = (float)(1 / (Math.Exp((opponent - player) / _A) + 1));
            return _Con * (result - expo);
        }

    }
}
