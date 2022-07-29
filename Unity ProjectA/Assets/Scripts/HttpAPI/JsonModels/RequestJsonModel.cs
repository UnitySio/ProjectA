using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server.JsonDataModels
{
    public abstract class RequestJsonModel
    {
    }
    
    public class RequestSignIn : RequestJsonModel
    {
        public string authType { get; set; }
        public string oauthToken { get; set; }
        public string jwtRefresh { get; set; }
        public string userIP { get; set; }
    }

    public class RequestUserData : RequestJsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨. 요청한 사용자 구분용도. 
        public string jwtAccess { get; set; }
    }

    public class RequestUpdateUserNickname : RequestJsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨. 요청한 사용자 구분용도. 
        public string jwtAccess { get; set; }

        //변경할 유저 닉네임 값.
        //추후 서버에서 해당 계정이 닉네임을 변경할 권리 (닉네임 변경권 소유여부 등) 가 있는지 DB에서 조회후 판정하게끔 예정
        public string userNickname { get; set; }
    }

    public class RequestAddCharacter : RequestJsonModel
    {
        public string jwtAccess { get; set; }
        public int characterUniqueID { get; set; }
        public int characterLv { get; set; }
    }

    public class RequestGetCharacter : RequestJsonModel
    {
        public string jwtAccess { get; set; }
    }
}