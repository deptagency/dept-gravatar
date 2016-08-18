using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.GraphAPI
{
    public class Response<T> where T : class
    {
        public List<T> Data { get; set; }
        public ResponsePaging Paging { get; set; }
    }
}
