using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sensi
{
    public static class ExtensionMethods
    {
        public static char GetTemperatureCode(this TemperatureUnits value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            TemperatureCode attribute
                    = Attribute.GetCustomAttribute(field, typeof(TemperatureCode))
                        as TemperatureCode;

            return attribute == null ? 'C' : attribute.Code;
        }
    }
}
