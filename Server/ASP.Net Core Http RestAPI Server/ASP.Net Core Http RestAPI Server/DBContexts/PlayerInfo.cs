using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class PlayerInfo
    {
        public uint PlayerUniqueId { get; set; }
        public uint AccountUniqueId { get; set; }
        public uint PlayerLv { get; set; }
        public uint PlayerExp { get; set; }
        public uint PlayerStamina { get; set; }
        public uint PlayerIngamePoint { get; set; }
        public uint PlayerCashPoint { get; set; }
        public string PlayerNickname { get; set; }
        public string PlayerProfileImageUrl { get; set; }
        public string PlayerGender { get; set; }
        public DateTime? PlayerBirthdate { get; set; }
        public DateTime? TimestampCreated { get; set; }
        public DateTime? TimestampLastSignin { get; set; }

        public virtual AccountInfo AccountUnique { get; set; }
    }
}
