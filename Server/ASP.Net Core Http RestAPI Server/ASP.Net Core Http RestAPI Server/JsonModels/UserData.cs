using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server.JsonDataModels
{
    public class UserData
    {
        public uint AccountUniqueId { get; set; }
        public int AuthLv { get; set; }
        public string AccountEmail { get; set; }
        public string UserNickname { get; set; }
        public int UserLv { get; set; }
    }

    public enum AuthLv
    {
        UserGuest,
        UserAccount,
        UserEvent,
        AdminGM,
        AdminChiefGM,
        AdminMaster
    }
}