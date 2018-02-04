using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sensi
{
    public class TemperatureCode : Attribute
    {
        public char Code { get; set; }
        public TemperatureCode(char code)
        {
            this.Code = code;
        }
    }
    public enum TemperatureUnits
    {
        [TemperatureCode('C')]
        Celcuis,
        [TemperatureCode('F')]
        Farenheit
    }

}
