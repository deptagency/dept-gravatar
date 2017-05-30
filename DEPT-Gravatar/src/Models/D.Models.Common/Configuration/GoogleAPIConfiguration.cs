using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.Common.Configuration
{
    public class GoogleAPIConfiguration
    {
        public string ApplicationName { get; set; }
        public string ServiceAccountClientEmail { get; set; }
        public string UserEmailImpersonate { get; set; }
        public string ServiceAccountPrivateKey { get; set; }
    }
}
