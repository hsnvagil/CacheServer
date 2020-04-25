using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheClient {
    public class Post {
        public string Operation { get; set; }
        public KeyValuePair<string, string> Data;
    }
}
