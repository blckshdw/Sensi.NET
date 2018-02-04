using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.Response
{

    public class Location
    {
        public string Zipcode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public class WeatherResponse
    {
        public string Condition { get; set; }
        public int ConditionId { get; set; }
        public Temperature CurrentTemp { get; set; }
        public Temperature HighTemp { get; set; }
        public Temperature LowTemp { get; set; }
        public Location Location { get; set; }
    }
}
