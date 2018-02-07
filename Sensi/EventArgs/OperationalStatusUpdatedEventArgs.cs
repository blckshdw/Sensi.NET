using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.EventArgs
{
    public class OperationalStatusUpdatedEventArgs
    {
        public string Icd { get; set; }
        public Sensi.Response.OperationalStatus OperationalStatus { get; set; }

    }
}
