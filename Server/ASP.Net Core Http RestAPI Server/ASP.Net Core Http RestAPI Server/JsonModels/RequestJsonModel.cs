using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server.JsonDataModels
{
    public abstract class RequestJsonModel
    {
    }

    // URL/Auth/Login
    public class RequestSignIn : RequestJsonModel
    {
        //authType을 나타냄. account, oauth, jwt 의 3가지. 
        //계정email, pw 입력시 account & google/apple oauth사용시 oauth, JWT AccessToken 갱신시 jwt
        public string authType { get; set; }

        //authType이 oauth일때 송신.
        public string oauthToken { get; set; }

        //authType이 account일때 송신. 계정 email 주소
        public string accountEmail { get; set; }

        //authType이 account일때 송신. hash함수로 추출된 계정 password
        public string accountPassword { get; set; }

        //authType이 jwt일때 송신. refreshToken을 확인후 새로 JWT AccessToken 발급하여 반환함.
        public string jwtRefresh { get; set; }
        
        public string userIP { get; set; }
    }

    public class RequestSignUpAuthNumber : RequestJsonModel
    {
        public string accountEmail { get; set; }
    }

    public class RequestSignUpAuthNumberVerify : RequestJsonModel
    {
        public string signUpToken { get; set; }

        public string authNumber { get; set; }
    }

    public class RequestSignUp : RequestJsonModel
    {
        //authType을 나타냄. account, oauth 의 2가지.    추후 연동과정 추가시 새로운 타입이 추가될 수 있음.
        //계정email, pw 입력시 account & google/apple oauth사용시 oauth
        public string authType { get; set; }

        //authType이 oauth일때 송신.
        public string oauthToken { get; set; }

        //authType이 account일때 송신. 계정 email 주소
        public string accountEmail { get; set; }

        //authType이 account일때 송신. hash함수로 추출된 계정 password
        public string accountPassword { get; set; }

        public string signUpToken { get; set; }
        
        public string userIP { get; set; }
        /*
        siogames 이메일 계정

        구글계정간 연동.


        siogames 계정을 먼저 생성후, 아직 연동안된 구글계정을 연동하거나  연동되었다가 연동해제된 구글계정을 연동하는건 ok.

        단, 구글계정으로 먼저 생성후 siogames계정을 연동시에는, 기존에 생성된 siogames계정으로는 연동이 불가능.
        구글계정으로 먼저 생성후 siogames계정을 연동시에는, 그때 새로 siogames을 생성하는 과정을 거쳐서 연동해야함.

        siogames 계정으로 먼저 생성 -> 구글 연동 or 연동 해제 ok
        구글 계정으로 먼저 생성 -> siogames 계정 연동시 그자리에서 바로 가입후 연동해야함.

        */
    }

    public class RequestFindPasswordAuthNumber : RequestJsonModel
    {
        //비밀번호를 찾을 email 주소값.
        public string accountEmail { get; set; }
    }

    public class RequestFindPasswordAuthNumberVerify : RequestJsonModel
    {
        //이전단계 확인용 고유 token. 이 값은 반드시 필요함. (추후 db에서 해당 토큰을 가지고 진행함. 유효기간 체크도 진행) 
        public string findPasswordToken { get; set; }

        //인증메일로 발송된 고유 문자열번호. 반드시 포함해야함. 
        public string authNumber { get; set; }
    }

    public class RequestResetPassword : RequestJsonModel
    {
        //이전단계 확인용 고유 token. 이 값은 반드시 필요함. (추후 db에서 해당 토큰을 가지고 진행함. 유효기간 체크도 진행) 
        public string findPasswordToken { get; set; }

        //변경할 계정 비밀번호 값.
        public string accountPassword { get; set; }
    }

    public class RequestUserData : RequestJsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨. 요청한 사용자 구분용도. 
        public string jwtAccess { get; set; }
    }

    public class RequestUserNicknameUpdate : RequestJsonModel
    {
        //JWT Access 토큰. 1시간 내외의 유효시간을 갖는 토큰. 로그인 과정 성공시 반환됨. 요청한 사용자 구분용도. 
        public string jwtAccess { get; set; }

        //변경할 유저 닉네임 값.
        //추후 서버에서 해당 계정이 닉네임을 변경할 권리 (닉네임 변경권 소유여부 등) 가 있는지 DB에서 조회후 판정하게끔 예정
        public string userNickname { get; set; }
    }

    public class RequestUserNicknameCheck : RequestJsonModel
    {
        public string jwtAccess { get; set; }
    }
}