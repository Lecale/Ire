﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    class Player : Person, IComparable
    {
		protected float MMS = -1;  //static
        protected float[] score;
		protected bool[] participation;
		protected int[] opponent;
		protected int[] handi;
		protected int[] BlackWhite;
        protected int pin; //internal id
        protected bool topBar = false; //no handicap?
        protected float SOS = -1;
		protected int eRating;


		public Player(int _seed, string _nom , int _rat, string _ctry, string _club, bool[] par) 
			: base (_nom, _rat, _club, _ctry )
        {
			//participation does not exist
			participation = new bool[par.Length];
			for(int i=0; i<par.Length; i++)
				participation[i]=par[i];
		}

        public void setPlayer(string nom, string cc, string g, int rt, int tr, int p)
        {
            Name = nom;
            Country = cc;
            Rank = g;
            Rating = rt;
            score = new float[tr];
            participation = new bool[tr];
            opponent = new int[tr]; //positive=player //negative=bye //0=?
            pin = p;

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
			score[rnd] = sc;
			opponent[rnd] = op;
		}
		//This is the correct method
		public void setResult(int rnd, int op, float sc, int hnd=0, int BW =1)
		{
			score[rnd] = sc;
			opponent[rnd] = op;
			handi[rnd] = hnd;
			BlackWhite[rnd] = BW;
		}

        public float getMMS()
        {
            float f = 0;
            for (int i = 0; i < opponent.Length; i++)
                f += score[i];
            f += MMS;
                return f;
        }

        public float getMMS(int rnd)
        {
            float f = 0;
            for (int i = 0; i < rnd; i++)
                f += score[i];
            f += MMS;
            return f;
        }

		//Use for SOS calculation
		public int getOpponent(int i)
		{
			return opponent[i];
		}//Use for SOS calculation
		public int getAdjHandi(int i)
		{
			//if black substract handicap , if White add handicap to SOS
			return handi[i] * BlackWhite[i];
		}
        public void setOpponent(int i, int r)
        {
            opponent[r] = i;
        }
        public bool getParticipation(int i)
        {
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
        public float getSOS()
        {
            return SOS;
        }
        public int getRating()
        {
            return Rating;
        }
        public void setSOS(float s)
        {
            SOS = s;
        }
        public void setMMS(float s)
        {
            MMS = s;
        }
        public int getPin()//why is this a string?
        { return pin; }
        public void setTop()
        { topBar = true; }
        public bool isTop()
        { return topBar; }

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
				if(pin==p.pin)
					return true;
			}
			catch(Exception e) {
				return false;
			}
			return false;
		}

        public int CompareTo(Object o)
        {
            Player p = (Player)o; // yuck
            if (p.MMS > MMS)
                return -1;
            if (p.MMS == MMS)
            {
                if (p.SOS > SOS)
                    return -1;
                if (p.SOS == SOS)
                    return 0;
                return 1;
            }

            return 1;
        }

        public override string ToString()
        {
            return Name + " " + Rating;
        }
    }


}
