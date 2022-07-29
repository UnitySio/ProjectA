using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server.JsonDataModels
{
    public class ResponseJsonModel
    {
        //요청 관련 결과를 나타냄. ok 라면 정상. 그외의 문자열이라면 해당 예외상황에 대한 문자열 반환. (오류코드 등)
        public string result { get; set; }
    }
    
    public class ResponseSignIn : ResponseJsonModel
    {
        public string jwtAccess { get; set; }
        public string jwtRefresh { get; set; }
    }

    public class ResponseUserData : ResponseJsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨. 요청한 사용자 구분용도. 
        public UserData userData { get; set; }
    }

    public class ResponseUpdateUserNickname : ResponseJsonModel
    {
    }

    public class ResponseAddCharacter : ResponseJsonModel
    {
    }

    public class ResponseGetCharacter : ResponseJsonModel
    {
        public List<CharacterData> characterData { get; set; }
    }
}