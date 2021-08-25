using System;
using System.Collections.Generic;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class CharacterInfo
    {
        public uint CharacterUniqueId { get; set; }
        public uint AccountUniqueId { get; set; }
        public string CharacterName { get; set; }
        public uint CharacterLv { get; set; }
        public uint CharacterExp { get; set; }
        public uint CharacterHp { get; set; }
        public uint CharacterAtk { get; set; }
        public uint CharacterDef { get; set; }
        public string CharacterSpecStat { get; set; }

        public virtual AccountInfo AccountUnique { get; set; }
    }
}
