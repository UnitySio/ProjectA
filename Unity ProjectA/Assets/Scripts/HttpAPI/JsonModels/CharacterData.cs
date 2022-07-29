using UnityEngine;

namespace ASP.Net_Core_Http_RestAPI_Server.JsonDataModels
{
    [System.Serializable]
    public class CharacterData
    {
        [field: SerializeField]
        public uint CharacterUniqueID { get; set; }
        [field: SerializeField]
        public int CharacterGrade { get; set; }
        [field: SerializeField]
        public int CharacterLv { get; set; }
    }
}