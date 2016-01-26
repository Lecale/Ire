using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Joc is intended to replace Pairing
//lightweight convenience class
namespace Ire
{
    class Joc 
    {
        private string PINS;
        public Joc(string white, string black)
        {
            PINS = white + "_" + black;
        }
        public bool ContainsJucator(string _pin)
        {
            if (PINS.Contains(_pin))
                return true;
            return false;
        }
    }
}
