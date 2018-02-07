using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sensi.Response;

namespace Sensi.EventArgs
{
    public class CapabilitiesUpdatedEventArgs
    {
        public string Icd { get; set; }
        public Capabilities Capabilities { get; set; }
    }

}
