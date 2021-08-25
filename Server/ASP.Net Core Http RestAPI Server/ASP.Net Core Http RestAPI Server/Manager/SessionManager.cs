using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class SessionManager
    {
        static ConcurrentDictionary<uint, string> sessionCheck;

        public static void Initialize()
        {
            sessionCheck = new ConcurrentDictionary<uint, string>();
        }


        //첫 접속시 로그인중인 유저 및 토큰 등록. (중복 로그인 방지용)
        public static bool RegisterToken(uint uniqueID, string token)
        {
            if (sessionCheck.ContainsKey(uniqueID))
            {
                sessionCheck[uniqueID] = token;
            }
            else
            {
                sessionCheck.TryAdd(uniqueID, token);
            }
            
            return sessionCheck.ContainsKey(uniqueID);
        }


        //다른 기기에서 로그인 중인지 체크.
        public static bool isDuplicate(uint uniqueID, string token)
        {
            if (sessionCheck.ContainsKey(uniqueID))
            {
                return sessionCheck[uniqueID].Equals(token);
            }
            return sessionCheck.ContainsKey(uniqueID);
        }
    }
}
