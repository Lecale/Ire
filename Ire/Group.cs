using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * This class is here to make things easier for the draw
 * it must be a bit wasteful
 * Order players by MMS 
 * Perhaps a sub group by SOS can be useful? 
 */

namespace Ire
{
    class Group
    {
        List<int> pins;
        public float MMS = -1;
        public Group(float f)
        {
            MMS = f;
            pins = new List<int>();
        }

        public bool Has(int i) 
        {
            if (pins.Contains(i))
                return true;
            return false;
        }

        public void add(int i)
        {
            pins.Add(i);
        }

        public int count()
        {
            return pins.Count;
        }

    }
}
