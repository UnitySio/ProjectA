using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class AccountInfo
    {
        public uint AccountUniqueId { get; set; }
        public byte AccountAuthLv { get; set; }
        public string AccountGuestToken { get; set; }
        public string AccountOauthTokenGoogle { get; set; }
        public string AccountOauthTokenApple { get; set; }
        public DateTime? AccountBanExpire { get; set; }
        public byte AccountBanReason { get; set; }
        public byte AccountBanned { get; set; }

        public virtual UserInfo UserInfo { get; set; }
    }
}
