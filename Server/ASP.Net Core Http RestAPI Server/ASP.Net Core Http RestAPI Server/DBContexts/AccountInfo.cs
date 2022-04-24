﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class AccountInfo
    {
        public AccountInfo()
        {
            UserLoginLogs = new HashSet<UserLoginLog>();
        }

        public uint AccountUniqueId { get; set; }
        public byte AccountAuthLv { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPassword { get; set; }
        public string AccountGuestToken { get; set; }
        public string AccountOauthTokenGoogle { get; set; }
        public string AccountOauthTokenApple { get; set; }
<<<<<<< HEAD
<<<<<<< HEAD
=======
        public DateTime AccountBanExpire { get; set; }
        public int AccountBanReason { get; set; }
        public int AccountBanned { get; set; }
>>>>>>> f6db78a... 계정 정지 관련 기능 추가
=======
        public DateTime? AccountBanExpire { get; set; }
        public byte AccountBanReason { get; set; }
        public byte AccountBanned { get; set; }
>>>>>>> 029fd61... 리팩토링 1차 재작업

        public virtual UserInfo UserInfo { get; set; }
        public virtual ICollection<UserLoginLog> UserLoginLogs { get; set; }
    }
}
