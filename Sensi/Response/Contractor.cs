using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.Response
{
    public class Contractor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string AddressLine1 { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string MainPhone { get; set; }
        public string LogoURL { get; set; }
    }
}
