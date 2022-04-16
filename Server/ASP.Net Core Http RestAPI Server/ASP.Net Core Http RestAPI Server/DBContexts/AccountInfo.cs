using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class AccountInfo
    {
        public AccountInfo()
        {
            CharacterInfos = new HashSet<CharacterInfo>();
        }

        public uint AccountUniqueId { get; set; }
        public byte AccountAuthLv { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPassword { get; set; }
        public string AccountGuestToken { get; set; }
        public string AccountOauthTokenGoogle { get; set; }
        public string AccountOauthTokenApple { get; set; }
        public DateTime AccountBanExpire { get; set; }
        public int AccountBanReason { get; set; }
        public int AccountBanned { get; set; }

        public virtual PlayerInfo PlayerInfo { get; set; }
        public virtual ICollection<CharacterInfo> CharacterInfos { get; set; }
    }
}
