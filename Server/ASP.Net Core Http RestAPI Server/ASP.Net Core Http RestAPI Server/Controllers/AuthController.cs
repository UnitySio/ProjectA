using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    /*

    AuthController

    Auth (인증) 관련 로직을 처리하는 컨트롤러 클래스.


    API 리스트

    - 로그인 (계정입력 & OAuth) 및 JWT AccessToken 재발급
    - 회원가입 (계정입력 & OAuth)
    - 비밀번호 찾기 (계정입력) - 요청                                                                
    - 비밀번호 찾기 (계정입력) - 인증번호 인증
    - 비밀번호 찾기 (계정입력) - 비밀번호 변경요청


    */



    [ApiController]
    public class AuthController : ControllerBase
    {
        private static DBContextPoolManager<siogames_mainContext> dbPoolManager;
        public static ILogger<AuthController> debugLogger;

        public AuthController(ILogger<AuthController> logger)
        {
            debugLogger = logger;
            if (dbPoolManager == null)
                dbPoolManager = new DBContextPoolManager<siogames_mainContext>();
        }

        //이메일 정규식 (aaa@gmail.com)
        Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");
        
        #region 로그인 (계정입력 & OAuth) 및 JWT AccessToken 재발급

        //요청 URI
        // http://serverAddress/auth/login
        [HttpPost("auth/login")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_Login> Post(Request_Auth_Login request)
        {
            Response_Auth_Login response = new Response_Auth_Login();
            var dbContext = dbPoolManager.Rent();

            if (request == null || string.IsNullOrEmpty(request.authType))
            {
                response.jwt_access = response.jwt_refresh = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            string authType = request.authType.ToLower();

            //계정정보로 로그인할 경우.
            if (authType.Equals("account"))
            {
                //입력된 값이 이메일 값이 아닌 경우.
                if (emailPattern.IsMatch(request.account_email) == false)
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid email address";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var result = dbContext.AccountInfos
                    .Where(table =>
                        //email주소가 일치하는 row를 검색.
                        table.AccountEmail.Equals(request.account_email)
                    )
                    .AsNoTracking();


                //해당 이메일의 계정이 존재할 경우.
                if (result.Count() == 1)
                {
                    var tableData = result.FirstOrDefault();

                    //비밀번호 정보가 틀린 경우 종료.
                    if (tableData.AccountPassword.Equals(request.account_password) == false)
                    {
                        response.jwt_access = response.jwt_refresh = null;
                        response.result = "invalid account info.";
                        dbPoolManager.Return(dbContext);
                        return response;
                    }
                    
                    // 계정이 정지되었을 경우
                    if (tableData.AccountBanned == 1)
                    {
                        var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)tableData.AccountBanExpire);
                        if (expire < 0) // 기간 만료 전
                        {
                            response.jwt_access = response.jwt_refresh = null;
                            response.result = $"banned, {tableData.AccountBanExpire}, {tableData.AccountBanReason}";
                            dbPoolManager.Return(dbContext);
                            return response;
                        }
                        if (expire > 0) // 기간 만료 후
                        {
                            tableData.AccountBanned = 0;
                            dbContext.Entry(tableData).State = EntityState.Modified;
                            var changedCount = await dbContext.SaveChangesAsync();
                        }
                    }

                    UserData userdata = new UserData()
                    {
                        AccountUniqueId = tableData.AccountUniqueId,
                        AccountEmail = tableData.AccountEmail,
                        AuthLv = tableData.AccountAuthLv,
                        //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                        //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                    };

                    try
                    {
                        userdata.UserLv = (int) tableData.PlayerInfo.PlayerLv;
                        userdata.UserName = tableData.PlayerInfo.PlayerNickname;
                    }
                    catch (Exception e)
                    {
                        userdata.UserLv = 0;
                        userdata.UserName = null;
                        debugLogger.LogWarning($"login exception : {e}");
                    }

                    //새로운 jwt토큰 발행후 반환.
                    response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                    response.jwt_refresh = JWTManager.createNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                    if (SessionManager.RegisterToken(tableData.AccountUniqueId, response.jwt_access))
                    {
                        //로그인에 성공한 유저의 로그인 일시를 갱신.
                        var playerData = dbContext.PlayerInfos
                            .Where(table =>
                                //email주소가 일치하는 row를 검색.
                                table.AccountUniqueId.Equals(userdata.AccountUniqueId)
                            )
                            .AsNoTracking();

                        if (playerData.Count() == 1)
                        {
                            var player = playerData.FirstOrDefault();

                            //최종 로그인 일시를 UTC시간 기준으로 갱신.
                            player.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(player).State = EntityState.Modified;
                            var changedCount = await dbContext.SaveChangesAsync();
                        }
                    }
                }
                //가입된 이메일이 아닌 경우.
                else if (result.Count() == 0)
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "need join account";
                }
                //잘못된 정보
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid account info.";
                }
            }
            //google, apple등의 oauth정보로 로그인할 경우.
            else if (authType.Contains("oauth"))
            {
                //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                IQueryable<AccountInfo> result;

                if (authType.Contains("google"))
                {
                    //oauth 토큰이 일치하는 row를 검색.
                    result = dbContext.AccountInfos
                        .Where(table => table.AccountOauthTokenGoogle.Equals(request.oauth_token))
                        .AsNoTracking();
                }
                else if (authType.Contains("apple"))
                {
                    //oauth 토큰이 일치하는 row를 검색.
                    result = dbContext.AccountInfos
                        .Where(table => table.AccountOauthTokenApple.Equals(request.oauth_token))
                        .AsNoTracking();
                }
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid authType";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //로그인 성공
                if (result.Count() == 1)
                {
                    var tableData = result.FirstOrDefault();

                    UserData userdata = new UserData()
                    {
                        AccountUniqueId = tableData.AccountUniqueId,
                        AccountEmail = tableData.AccountEmail,
                        AuthLv = tableData.AccountAuthLv,
                        //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                        //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                    };

                    try
                    {
                        userdata.UserLv = (int) tableData.PlayerInfo.PlayerLv;
                        userdata.UserName = tableData.PlayerInfo.PlayerNickname;
                    }
                    catch (Exception)
                    {
                        userdata.UserLv = 0;
                        userdata.UserName = null;
                    }

                    //새로운 jwt토큰 발행후 반환.
                    response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                    response.jwt_refresh = JWTManager.createNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                    if (SessionManager.RegisterToken(tableData.AccountUniqueId, response.jwt_access))
                    {
                        //로그인에 성공한 유저의 로그인 일시를 갱신.
                        var playerData = dbContext.PlayerInfos
                            .Where(table =>
                                //email주소가 일치하는 row를 검색.
                                table.AccountUniqueId.Equals(userdata.AccountUniqueId)
                            )
                            .AsNoTracking();

                        if (playerData.Count() == 1)
                        {
                            var player = playerData.FirstOrDefault();

                            //최종 로그인 일시를 UTC시간 기준으로 갱신.
                            player.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(player).State = EntityState.Modified;
                            var changedCount = await dbContext.SaveChangesAsync();
                        }
                    }
                }
                else if (result.Count() == 0)
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "need join account";
                }
                //잘못된 정보
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid oauthtoken info.";
                }
            }
            //jwt access token 갱신요청의 경우.
            else if (authType.Equals("jwt"))
            {
                //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
                SecurityToken tokenInfo = new JwtSecurityToken();

                if (JWTManager.checkValidationJWT(request.jwt_refresh, out tokenInfo))
                {
                    //유효성 검증이 완료된 토큰 정보.
                    JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                    object AccountUniqueId, AuthLv;
                    AccountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");
                    //AuthLv = jwt.Payload.GetValueOrDefault("AuthLv");

                    //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                    var result = dbContext.AccountInfos
                        .Where(table =>
                            //jwt 토큰에서 지정된 AccountUniqueId기준으로 테이블 검색.
                            table.AccountUniqueId.Equals(uint.Parse(AccountUniqueId.ToString()))
                        )
                        .AsNoTracking();

                    if (result.Count() == 1)
                    {
                        var tableData = result.FirstOrDefault();

                        UserData userdata = new UserData()
                        {
                            AccountUniqueId = tableData.AccountUniqueId,
                            AccountEmail = tableData.AccountEmail,
                            AuthLv = tableData.AccountAuthLv,
                            //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                            //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                        };

                        try
                        {
                            userdata.UserLv = (int) tableData.PlayerInfo.PlayerLv;
                            userdata.UserName = tableData.PlayerInfo.PlayerNickname;
                        }
                        catch (Exception)
                        {
                            userdata.UserLv = 0;
                            userdata.UserName = null;
                        }

                        response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                        //기존 jwt_refresh 그대로 적용.
                        response.jwt_refresh = request.jwt_refresh;
                        response.result = "ok";


                        //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                        if (SessionManager.RegisterToken(tableData.AccountUniqueId, response.jwt_access))
                        {
                            //로그인에 성공한 유저의 로그인 일시를 갱신.
                            var playerData = dbContext.PlayerInfos
                                .Where(table =>
                                    //email주소가 일치하는 row를 검색.
                                    table.AccountUniqueId.Equals(userdata.AccountUniqueId)
                                )
                                .AsNoTracking();

                            if (playerData.Count() == 1)
                            {
                                var player = playerData.FirstOrDefault();

                                //최종 로그인 일시를 UTC시간 기준으로 갱신.
                                player.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(player).State = EntityState.Modified;
                                var changedCount = await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    //잘못된 정보
                    else
                    {
                        response.jwt_access = response.jwt_refresh = null;
                        response.result = "invalid jwt info";
                    }
                }
                //jwt refresh 토큰이 만료되었거나, 유효하지 않다면.
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "expiration or invalid jwt info. need login";
                }
            }
            //게스트 로그인의 경우, 일단 임시 발급된 jwt로 계정정보를 판단함.
            else if (authType.Equals("guest"))
            {
                var guestToken = request.oauth_token;

                //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var result = dbContext.AccountInfos
                    .Where(table =>
                        //jwt 토큰에서 지정된 AccountUniqueId기준으로 테이블 검색.
                        table.AccountGuestToken.Equals(guestToken)
                    )
                    .AsNoTracking();

                //로그인 성공
                if (result.Count() == 1)
                {
                    var tableData = result.FirstOrDefault();

                    UserData userdata = new UserData()
                    {
                        AccountUniqueId = tableData.AccountUniqueId,
                        AccountEmail = tableData.AccountEmail,
                        AuthLv = tableData.AccountAuthLv,
                        //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                        //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                    };

                    try
                    {
                        userdata.UserLv = (int) tableData.PlayerInfo.PlayerLv;
                        userdata.UserName = tableData.PlayerInfo.PlayerNickname;
                    }
                    catch (Exception)
                    {
                        userdata.UserLv = 0;
                        userdata.UserName = null;
                    }

                    //새로운 jwt토큰 발행후 반환.
                    response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                    response.jwt_refresh = JWTManager.createNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                    if (SessionManager.RegisterToken(tableData.AccountUniqueId, response.jwt_access))
                    {
                        //로그인에 성공한 유저의 로그인 일시를 갱신.
                        var playerData = dbContext.PlayerInfos
                            .Where(table =>
                                //email주소가 일치하는 row를 검색.
                                table.AccountUniqueId.Equals(userdata.AccountUniqueId)
                            )
                            .AsNoTracking();

                        if (playerData.Count() == 1)
                        {
                            var player = playerData.FirstOrDefault();

                            //최종 로그인 일시를 UTC시간 기준으로 갱신.
                            player.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(player).State = EntityState.Modified;
                            var changedCount = await dbContext.SaveChangesAsync();
                        }
                    }
                }
                else if (result.Count() == 0)
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "need join account";
                }
                //잘못된 정보
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid guest token info";
                }
            }
            //로그인 일시 갱신.
            else if (authType.Equals("update"))
            {
                //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
                SecurityToken tokenInfo = new JwtSecurityToken();

                if (JWTManager.checkValidationJWT(request.jwt_refresh, out tokenInfo))
                {
                    //유효성 검증이 완료된 토큰 정보.
                    JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                    object AccountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");

                    var result = dbContext.AccountInfos
                        .Where(table =>
                            table.AccountUniqueId.Equals(uint.Parse(AccountUniqueId.ToString()))
                        )
                        .AsNoTracking();
                    
                    if (result.Count() == 1)
                    {
                        var tableData = result.FirstOrDefault();
                        
                        // 계정이 정지되었을 경우
                        if (tableData.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)tableData.AccountBanExpire);
                            if (expire < 0) // 기간 만료 전
                            {
                                response.jwt_access = response.jwt_refresh = null;
                                response.result = $"banned, {tableData.AccountBanExpire}, {tableData.AccountBanReason}";
                                dbPoolManager.Return(dbContext);
                                return response;
                            }
                            if (expire > 0) // 기간 만료 후
                            {
                                tableData.AccountBanned = 0;
                                dbContext.Entry(tableData).State = EntityState.Modified;
                                var changedCount = await dbContext.SaveChangesAsync();
                            }
                        }

                        //토큰 갱신작업이 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                        if (SessionManager.RegisterToken(tableData.AccountUniqueId, request.jwt_refresh))
                        {
                            //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                            var playerData = dbContext.PlayerInfos
                                .Where(table =>
                                    //jwt 토큰에서 지정된 AccountUniqueId기준으로 테이블 검색.
                                    table.AccountUniqueId.Equals(uint.Parse(AccountUniqueId.ToString()))
                                )
                                .AsNoTracking();
                            
                            if (playerData.Count() == 1)
                            {
                                var player = playerData.FirstOrDefault();
                                
                                //최종 로그인 일시를 UTC시간 기준으로 갱신.
                                player.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(player).State = EntityState.Modified;
                                var changedCount = await dbContext.SaveChangesAsync();
                                response.result = "ok";
                            }
                        }
                    }
                    //잘못된 정보
                    else
                    {
                        response.jwt_access = response.jwt_refresh = null;
                        response.result = "invalid jwt info";
                    }
                }
                //jwt refresh 토큰이 만료되었거나, 유효하지 않다면.
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "expiration or invalid jwt info. need login";
                }
            }
            else
            {
                response.jwt_access = response.jwt_refresh = null;
                response.result = "invalid authType";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 회원가입(계정입력) - 요청

        //요청 URI
        // http://serverAddress/auth/join/send-request
        [HttpPost("auth/join/send-request")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_Join_SendRequest> Post(Request_Auth_Join_SendRequest request)
        {
            Response_Auth_Join_SendRequest response = new Response_Auth_Join_SendRequest();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.join_token = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            //입력된 값이 이메일 값이 아닌 경우.
            if (emailPattern.IsMatch(request.account_email) == false)
            {
                response.join_token = null;
                response.result = "invalid email address";
                dbPoolManager.Return(dbContext);
                return response;
            }

            //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
            var result = dbContext.AccountInfos
                .Where(table =>
                    //email주소가 일치하는 row를 검색.
                    table.AccountEmail.Equals(request.account_email)
                )
                .AsNoTracking();

            //이메일이 존재할 경우.
            if (result.Count() >= 1)
            {
                response.join_token = null;
                response.result = "duplicate email";
            }
            else
            {
                EmailValidationInfo info = new EmailValidationInfo();

                //5분간 유효.
                info.expirateTime = DateTime.UtcNow.AddMinutes(5);
                //찾기 현재 진행 단계.
                info.currentStep = 1;
                info.EmailAddress = request.account_email;
                info.ValidateToken = JWTManager.createNewJWT(new UserData(), JWTManager.JWTType.AccessToken);
                info.EmailValidateConfirmNumber = new Random().Next(100000, 999998).ToString();


                EmailManager.RegisterJoinInfo(info.ValidateToken, info);

                EmailManager.sendGmail_SMTP(info.EmailAddress
                    , "siogames 인증메일"
                    , "회원가입 인증 메일 안내"
                    , $"\n\n\n\n\n인증번호 : {info.EmailValidateConfirmNumber}");

                response.join_token = info.ValidateToken;
                response.result = "ok";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
        
        #region 회원가입(계정입력) - 이메일 인증번호 인증

        //요청 URI
        // http://serverAddress/auth/join/send-auth-number
        [HttpPost("auth/join/send-auth-number")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_Join_SendAuthNumber> Post(Request_Auth_Join_SendAuthNumber request)
        {
            Response_Auth_Join_SendAuthNumber response = new Response_Auth_Join_SendAuthNumber();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var result = EmailManager.GetJoinInfo(request.join_token);

            //진행단계, 유효기간 체크.
            if (result != null && result.currentStep == 1 && result.expirateTime > DateTime.UtcNow)
            {
                if (result.EmailValidateConfirmNumber.Equals(request.auth_number))
                {
                    result.currentStep = 2;
                    EmailManager.RegisterJoinInfo(request.join_token, result);
                    response.result = "ok";
                }
                else
                {
                    //인증번호 재입력 필요.
                    response.result = "incorrect auth_number";
                }
            }
            //잘못된 데이터
            else
            {
                response.result = "invalid token.";
            }


            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
        
        #region 회원가입 (계정입력 & OAuth)

        //요청 URI
        // http://serverAddress/auth/join
        [HttpPost("auth/join")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_Join> Post(Request_Auth_Join request)
        {
            Response_Auth_Join response = new Response_Auth_Join();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.jwt_access = response.jwt_refresh = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            // UID
            // 맨 앞자리 규칙
            // 0 - 테스트 서버
            // 1 - 한국 서버
            // 2 - 일본 서버
            //     + 8자리수는 생성순서
            //
            // 한국서버 ex) 100000001


            string authType = request.authType.ToLower();
            
            if (authType.Equals("account"))
            {
                //입력된 값이 이메일 값이 아닌 경우.
                if (emailPattern.IsMatch(request.account_email) == false)
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid email address";
                    dbPoolManager.Return(dbContext);
                    return response;
                }


                var emailValidationInfo = EmailManager.GetJoinInfo(request.join_token);

                //진행단계, 유효기간 체크.
                if (emailValidationInfo != null && emailValidationInfo.currentStep == 2)
                {

                    //MariaDB+EntityFramework조합 에서 Transaction사용시 CreateExecutionStrategy 활용하여 실행해야함.
                    var strategy = dbContext.Database.CreateExecutionStrategy();

                    Func<Task> db_transaction_operation = async () =>
                    {
                        using (var transaction = await dbContext.Database.BeginTransactionAsync())
                        {
                            AccountInfo newUser = new AccountInfo()
                            {
                                AccountEmail = request.account_email,
                                AccountPassword = request.account_password,
                                AccountAuthLv = (byte)AuthLv.User_Account
                            };

                            //전달된 회원가입 정보로 db insert 실행.
                            try
                            {
                                //transaction 내에서 insert시, 
                                //innodb_autoinc_lock_mode = 0; 의 값을 0으로 해야한다. (AutoIncrement 값 증가 이슈)

                                var result = await dbContext.AccountInfos.AddAsync(newUser);
                                dbContext.Entry(newUser).State = EntityState.Added;
                                var changedCount = await dbContext.SaveChangesAsync();

                                //insert 성공시 player 생성.
                                PlayerInfo newPlayer = new PlayerInfo()
                                {
                                    AccountUniqueId = newUser.AccountUniqueId,
                                    TimestampCreated = DateTime.UtcNow,
                                    TimestampLastSignin = DateTime.UtcNow
                                };

                                var result2 = await dbContext.PlayerInfos.AddAsync(newPlayer);
                                dbContext.Entry(newPlayer).State = EntityState.Added;
                                var changedCount2 = await dbContext.SaveChangesAsync();

                                await transaction.CommitAsync();

                                UserData userdata = new UserData()
                                {
                                    AccountUniqueId = newUser.AccountUniqueId,
                                    AuthLv = newUser.AccountAuthLv
                                };

                                //새로운 jwt토큰 발행후 반환.
                                response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                                response.jwt_refresh = JWTManager.createNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                                response.result = "ok";
                            }
                            catch (Exception e)
                            {
                                await transaction.RollbackAsync();

                                response.jwt_access = response.jwt_refresh = null;

                                if (e.ToString().Contains("Duplicate entry"))
                                    response.result = "exist account info";
                                else
                                {
                                    response.result = "server Error.";
                                    debugLogger.LogError($"Exception = {e}");
                                }
                            }
                        }
                    };

                    EmailManager.RemoveJoinInfo(request.join_token);

                    //transaction 쿼리 실행.
                    await strategy.ExecuteAsync(db_transaction_operation);
                }
                //잘못된 데이터
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid token.";
                    dbPoolManager.Return(dbContext);

                    EmailManager.RemoveJoinInfo(request.join_token);
                    return response;
                }
            }
            else if (authType.Contains("oauth"))
            {
                AccountInfo newUser = new AccountInfo()
                {
                    AccountAuthLv = (byte)AuthLv.User_Account
                };

                //로그인한 OAuth 타입에 맞춰 값 입력.
                if (authType.Contains("google"))
                {
                    newUser.AccountOauthTokenGoogle = request.oauth_token;
                }
                else if (authType.Contains("apple"))
                {
                    newUser.AccountOauthTokenApple = request.oauth_token;
                }
                else
                {
                    response.jwt_access = response.jwt_refresh = null;
                    response.result = "invalid authType";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //MariaDB+EntityFramework조합 에서 Transaction사용시 CreateExecutionStrategy 활용하여 실행해야함.
                var strategy = dbContext.Database.CreateExecutionStrategy();
                Func<Task> db_transaction_operation = async delegate
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        //전달된 회원가입 정보로 db insert 실행.
                        try
                        {
                            //transaction 내에서 insert시, 
                            //innodb_autoinc_lock_mode = 0; 의 값을 0으로 해야한다. (AutoIncrement 값 증가 이슈)

                            var result = await dbContext.AccountInfos.AddAsync(newUser);
                            var changedCount = await dbContext.SaveChangesAsync();
                            
                            //insert 성공시 player 생성.
                            PlayerInfo newPlayer = new PlayerInfo()
                            {
                                AccountUniqueId = newUser.AccountUniqueId,
                                TimestampCreated = DateTime.UtcNow,
                                TimestampLastSignin = DateTime.UtcNow
                            };
                            
                            var result2 = await dbContext.PlayerInfos.AddAsync(newPlayer);
                            var changedCount2 = await dbContext.SaveChangesAsync();

                            await transaction.CommitAsync();

                            UserData userdata = new UserData()
                            {
                                AccountUniqueId = newUser.AccountUniqueId,
                                AuthLv = newUser.AccountAuthLv
                            };

                            //새로운 jwt토큰 발행후 반환.
                            response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                            response.jwt_refresh = JWTManager.createNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                            response.result = "ok";
                        }
                        catch (Exception e)
                        {
                            await transaction.RollbackAsync();
                            
                            response.jwt_access = response.jwt_refresh = null;
                            
                            if (e.ToString().Contains("Duplicate entry"))
                                response.result = "exist account info";
                            else
                            {
                                response.result = "server Error.";
                                debugLogger.LogError($"Exception = {e}");
                            }
                        }
                    }
                };

                //transaction 쿼리 실행.
                await strategy.ExecuteAsync(db_transaction_operation);
            }
            else if (authType.Equals("guest"))
            {
                //MariaDB+EntityFramework조합 에서 Transaction사용시 CreateExecutionStrategy 활용하여 실행해야함.
                var strategy = dbContext.Database.CreateExecutionStrategy();
                Func<Task> db_transaction_operation = async delegate
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        AccountInfo newUser = new AccountInfo()
                        {
                            AccountAuthLv = (byte)AuthLv.User_Guest,
                            AccountGuestToken = request.oauth_token
                        };

                        //
                        if (string.IsNullOrEmpty(request.oauth_token))
                            newUser.AccountGuestToken = Guid.NewGuid().ToString();
                        
                        //전달된 회원가입 정보로 db insert 실행.
                        try
                        {
                            var result = await dbContext.AccountInfos.AddAsync(newUser);
                            dbContext.Entry(newUser).State = EntityState.Added;
                            var changedCount = await dbContext.SaveChangesAsync();

                            //insert 성공시 player 생성.
                            PlayerInfo newPlayer = new PlayerInfo()
                            {
                                AccountUniqueId = newUser.AccountUniqueId,
                                TimestampCreated = DateTime.UtcNow,
                                TimestampLastSignin = DateTime.UtcNow
                            };
                            
                            var result2 = await dbContext.PlayerInfos.AddAsync(newPlayer);
                            dbContext.Entry(newPlayer).State = EntityState.Added;
                            var changedCount2 = await dbContext.SaveChangesAsync();

                            await transaction.CommitAsync();

                            UserData userdata = new UserData()
                            {
                                AccountUniqueId = newUser.AccountUniqueId,
                                AuthLv = newUser.AccountAuthLv
                            };

                            //새로운 jwt토큰 발행후 반환.
                            response.jwt_access = JWTManager.createNewJWT(userdata, JWTManager.JWTType.AccessToken);
                            response.jwt_refresh = JWTManager.createNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                            response.result = "ok";
                        }
                        catch (Exception e)
                        {
                            await transaction.RollbackAsync();

                            response.jwt_access = response.jwt_refresh = null;

                            if (e.ToString().Contains("Duplicate entry"))
                                response.result = "exist account info";
                            else
                            {
                                response.result = "server Error.";
                                debugLogger.LogError($"Exception = {e}");
                            }
                        }
                    }
                };

                //transaction 쿼리 실행.
                await strategy.ExecuteAsync(db_transaction_operation);
            }
            else
            {
                response.jwt_access = response.jwt_refresh = null;
                response.result = "invalid authType";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
        
        #region 비밀번호 찾기(계정입력) - 요청

        //요청 URI
        // http://serverAddress/auth/findpassword/send-request
        [HttpPost("auth/findpassword/send-request")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_FindPassword_SendRequest> Post(Request_Auth_FindPassword_SendRequest request)
        {
            Response_Auth_FindPassword_SendRequest response = new Response_Auth_FindPassword_SendRequest();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.findpassword_token = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            //입력된 값이 이메일 값이 아닌 경우.
            if (emailPattern.IsMatch(request.account_email) == false)
            {
                response.findpassword_token = null;
                response.result = "invalid email address";
                dbPoolManager.Return(dbContext);
                return response;
            }

            //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
            var result = dbContext.AccountInfos
                .Where(table =>
                    //email주소가 일치하는 row를 검색.
                    table.AccountEmail.Equals(request.account_email)
                )
                .AsNoTracking();

            //이메일이 존재할 경우.
            if (result.Count() == 1)
            {
                EmailValidationInfo info = new EmailValidationInfo();

                //5분간 유효.
                info.expirateTime = DateTime.UtcNow.AddMinutes(5);
                //찾기 현재 진행 단계.
                info.currentStep = 1;
                info.EmailAddress = request.account_email;
                info.ValidateToken = JWTManager.createNewJWT(new UserData(), JWTManager.JWTType.AccessToken);
                info.EmailValidateConfirmNumber = new Random().Next(100000, 999998).ToString();
                
                EmailManager.RegisterFindPasswordInfo(info.ValidateToken, info);

                EmailManager.sendGmail_SMTP(info.EmailAddress
                    , "siogames 인증메일"
                    , "비밀번호 찾기 인증 메일 안내"
                    , $"\n\n\n\n\n인증번호 : {info.EmailValidateConfirmNumber}");

                response.findpassword_token = info.ValidateToken;
                response.result = "ok";
            }
            else
            {
                response.findpassword_token = null;
                response.result = "not find email";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
        
        #region 비밀번호 찾기(계정입력) - 인증번호 인증

        //요청 URI
        // http://serverAddress/auth/findpassword/send-auth-number
        [HttpPost("auth/findpassword/send-auth-number")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_FindPassword_SendAuthNumber> Post(Request_Auth_FindPassword_SendAuthNumber request)
        {
            Response_Auth_FindPassword_SendAuthNumber response 
                = new Response_Auth_FindPassword_SendAuthNumber();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var result = EmailManager.GetFindPasswordInfo(request.findpassword_token);

            //진행단계, 유효기간 체크.
            if (result != null && result.currentStep == 1 && result.expirateTime > DateTime.UtcNow)
            {
                if (result.EmailValidateConfirmNumber.Equals(request.auth_number))
                {
                    result.currentStep = 2;
                    EmailManager.RegisterFindPasswordInfo(request.findpassword_token, result);
                    response.result = "ok";
                }
                else
                {
                    //인증번호 재입력 필요.
                    response.result = "incorrect auth_number";
                }
            }
            //잘못된 데이터
            else
            {
                response.result = "invalid token.";
            }


            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
        
        #region 비밀번호 찾기(계정입력) - 비밀번호 변경요청

        //요청 URI
        // http://serverAddress/auth/findpassword/update-account-password
        [HttpPost("auth/findpassword/update-account-password")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_Auth_FindPassword_UpdateAccountPassword> Post(Request_Auth_FindPassword_UpdateAccountPassword request)
        {
            Response_Auth_FindPassword_UpdateAccountPassword response 
                = new Response_Auth_FindPassword_UpdateAccountPassword();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            if (string.IsNullOrEmpty(request.account_password))
            {
                response.result = "invalid password value";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var result = EmailManager.GetFindPasswordInfo(request.findpassword_token);

            //진행단계, 유효기간 체크.
            if (result != null && result.currentStep == 2)
            {
                //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var accountData = dbContext.AccountInfos
                    .Where(table =>
                        //email주소가 일치하는 row를 검색.
                        table.AccountEmail.Equals(result.EmailAddress)
                    )
                    .AsNoTracking();

                if (accountData.Count() == 1)
                {
                    var account = accountData.FirstOrDefault();

                    //비밀번호 변경사항 db에 반영
                    account.AccountPassword = request.account_password;
                    dbContext.Entry(account).State = EntityState.Modified;
                    var changedCount = await dbContext.SaveChangesAsync();

                    response.result = "ok";
                }
                else
                {
                    response.result = "server Error.";
                }
                EmailManager.RemoveFindPasswordInfo(request.findpassword_token);
            }
            //잘못된 데이터
            else
            {
                response.result = "invalid token.";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

    }
}
