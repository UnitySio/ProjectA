using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts.Models
{
    public partial class UserInfo
    {
        public uint UserUniqueId { get; set; }
        public uint AccountUniqueId { get; set; }
        public uint? UserLv { get; set; }
        public uint? UserExp { get; set; }
        public uint? UserStamina { get; set; }
        public string UserNickname { get; set; }
        public DateTime? TimestampCreated { get; set; }
        public DateTime? TimestampLastSignin { get; set; }

        public virtual AccountInfo AccountUnique { get; set; }
    }
}
