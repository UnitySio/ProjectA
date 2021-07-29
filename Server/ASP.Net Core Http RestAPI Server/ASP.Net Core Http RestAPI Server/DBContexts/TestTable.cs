using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.NET_Core_RestfulAPI_TestServer.DBContexts
{
    public partial class TestTable
    {
        public long UniqueId { get; set; }
        public string TestString { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public long Level { get; set; }
        public long Exp { get; set; }
    }
}
