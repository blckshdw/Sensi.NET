using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.Response
{
    public class Temperature
    {
        public int F { get; set; }
        public int C { get; set; }

        public override string ToString()
        {
            return $"{C}°C {F}°F";
        }
    }
}
