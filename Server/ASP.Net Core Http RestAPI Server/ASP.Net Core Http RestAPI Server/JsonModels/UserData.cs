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
        public string UserName { get; set; }
        public int UserLv { get; set; }
    }

    public enum AuthLv : int
    {
        User_Guest = 0,
        User_Account = 1,
        User_Event = 2,
        Admin_GM = 3,
        Admin_ChiefGM = 4,
        Admin_Master = 5
    }
}
