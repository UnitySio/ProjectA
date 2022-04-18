using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server.JsonDataModels
{
    public class Response_JsonModel
    {
        //요청 관련 결과를 나타냄. ok 라면 정상. 그외의 문자열이라면 해당 예외상황에 대한 문자열 반환. (오류코드 등)
        public string result { get; set; }
    }

    public class Response_Auth_Login : Response_JsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨.
        public string jwt_access { get; set; }
        //JWT Refresh 토큰. 2주정도의 유효시간을 갖는 토큰이며, Access 토큰만료시
        //번거롭게 다시 로그인과정을 거치치 않고 Access 토큰을 재발급 받는데 사용됨.Refresh 토큰 만료시, 재로그인 필요
        public string jwt_refresh { get; set; }
    }

    public class Response_Auth_Join_SendRequest : Response_JsonModel
    {
        public string join_token { get; set; }
    }

    public class Response_Auth_Join_SendAuthNumber : Response_JsonModel
    {
        
    }


    public class Response_Auth_Join : Response_JsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨.
        public string jwt_access { get; set; }
        //JWT Refresh 토큰. 2주정도의 유효시간을 갖는 토큰이며, Access 토큰만료시
        //번거롭게 다시 로그인과정을 거치치 않고 Access 토큰을 재발급 받는데 사용됨.Refresh 토큰 만료시, 재로그인 필요
        public string jwt_refresh { get; set; }
    }

    public class Response_Auth_FindPassword_SendRequest : Response_JsonModel
    {
        //다음 단계를 진행하기 위한 고유 토큰.  1개의 email에 대해서 고유함. 
        //(이후에 다른 요청이 오면 이전 요청의 findpassword_token은 취소됨.즉 만료처리 됨)
        public string findpassword_token { get; set; }
    }

    public class Response_Auth_FindPassword_SendAuthNumber : Response_JsonModel
    {

    }

    public class Response_Auth_FindPassword_UpdateAccountPassword : Response_JsonModel
    {

    }

    public class Response_User_Gamedata : Response_JsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨. 요청한 사용자 구분용도. 
        public UserData userDataInfo { get; set; }
    }

    public class Response_User_Gamedata_UpdateUserName : Response_JsonModel
    {

    }
    
    public class ResponseServer : Response_JsonModel
    {
        public int serverState { get; set; }
    }
}
