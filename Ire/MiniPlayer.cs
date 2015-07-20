using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ire
{
    //a superclass for Player containing much less info
    class MiniPlayer //: IComparable
    {

        protected int pin; //internal id
        public MiniPlayer(int tPin)
        {
            pin = tPin;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            try
            {
                MiniPlayer p = (MiniPlayer)obj;
                if (pin == p.pin)
                    return true;
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }
    }
}
