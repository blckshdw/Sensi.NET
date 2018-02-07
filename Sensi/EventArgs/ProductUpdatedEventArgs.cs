using Sensi.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.EventArgs
{
    public class ProductUpdatedEventArgs
    {
        public string Icd { get; set; }
        public Product Product { get; set; }
    }
}
