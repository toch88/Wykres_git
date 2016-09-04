using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wykres
{
    class Package: object
    {
        public float charNb, last;
        public float sin, cos, deg;
        
        public Package(float _charNb, float _sin, float _cos, float _deg, float _last)
        {
            charNb = _charNb;
            sin = _sin;
            cos = _cos;
            deg = _deg;
            last = _last;
        }
    }       
}
