﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.Response
{
    public class ThermostatResponse
    {
        public string DeviceName { get; set; }
        public int ContractorId { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ICD { get; set; }
        public string TimeZone { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        public override string ToString()
        {
            return this.DeviceName;
        }
    }
}
