using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.NET_Core_RestfulAPI_TestServer.DBContexts
{
    public partial class TestLogin
    {
        public uint AccountUniqueId { get; set; }
        public string AccountId { get; set; }
        public string AccountPassword { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}
