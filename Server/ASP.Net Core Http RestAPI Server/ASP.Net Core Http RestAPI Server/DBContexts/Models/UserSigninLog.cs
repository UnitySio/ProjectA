using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts.Models
{
    public partial class UserSigninLog
    {
        public uint LogUniqueId { get; set; }
        public uint AccountUniqueId { get; set; }
        public string UserIp { get; set; }
        public string UserNickname { get; set; }
        public DateTime? TimestampLastSignin { get; set; }
    }
}
