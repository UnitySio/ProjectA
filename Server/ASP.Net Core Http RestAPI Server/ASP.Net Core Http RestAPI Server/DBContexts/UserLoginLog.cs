using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class UserLoginLog
    {
        public uint LogUniqueId { get; set; }
        public uint AccountUniqueId { get; set; }
        public string UserNickname { get; set; }
        public string UserIp { get; set; }
        public DateTime? TimestampLastLogin { get; set; }

        public virtual AccountInfo AccountUnique { get; set; }
    }
}
