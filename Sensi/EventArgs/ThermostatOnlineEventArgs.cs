using Sensi.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.EventArgs
{
    public class ThermostatOnlineEventArgs : OnlineResponseBase
    {
        public string Icd { get; set; }
        
        public static explicit operator ThermostatOnlineEventArgs(OnlineResponse o)
        {
            ThermostatOnlineEventArgs t = new ThermostatOnlineEventArgs();

            t.Capabilities = o.Capabilities;
            t.EnvironmentControls = o.EnvironmentControls;
            t.OperationalStatus = o.OperationalStatus;
            t.Product = o.Product;
            t.Schedule = o.Schedule;
            t.Settings = o.Settings;
            return t;

        }
        
    }
}
