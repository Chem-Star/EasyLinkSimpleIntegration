using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLinkSimpleIntegration.DataObjects
{
    class EasyLinkResponse
    {
        public string status        { get; set; }
        public string message       { get; set; }
        public string record_name   { get; set; }
        public EasyLinkOrder record { get; set; }
    }
}
