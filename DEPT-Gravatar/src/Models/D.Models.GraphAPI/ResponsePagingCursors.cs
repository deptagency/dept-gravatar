using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.GraphAPI
{
    public class ResponsePagingCursors
    {
        public string Before { get; set; }
        public string After { get; set; }
    }
}
