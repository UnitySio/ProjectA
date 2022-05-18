using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class UserCharacterInfo
    {
        public uint InfoUniqueId { get; set; }
        public uint AccountUniqueId { get; set; }
        public uint CharacterUniqueId { get; set; }
        public uint? CharacterLv { get; set; }
    }
}
