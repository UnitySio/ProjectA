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
        private static DBContextPoolManager<projectaContext> dbPoolManager;
        public static ILogger<AuthController> debugLogger;

        public AuthController(ILogger<AuthController> logger)
        {
            debugLogger = logger;
            if (dbPoolManager == null)
                dbPoolManager = new DBContextPoolManager<projectaContext>();
        }

        //이메일 정규식 (aaa@gmail.com)
        Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");

        // 비밀번호 형식
        private Regex passwordPattern = new Regex(@"^(?=.*?[a-z])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[\W]).{8,}$");

        #region 로그인 (계정입력 & OAuth) 및 JWT AccessToken 재발급

        //요청 URL
        // http://serverAddress/signin
        [HttpPost("signin")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponseSignIn> Post(RequestSignIn request)
        {
            var response = new ResponseSignIn();
            var dbContext = dbPoolManager.Rent();

            if (request == null || string.IsNullOrEmpty(request.authType))
            {
                response.jwtAccess = response.jwtRefresh = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var authType = request.authType.ToLower();

            //계정정보로 로그인할 경우.
            if (authType.Equals("account"))
            {
                //입력된 값이 이메일 값이 아닌 경우.
                if (emailPattern.IsMatch(request.accountEmail) == false)
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid email address";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        //email주소가 일치하는 row를 검색.
                        table.AccountEmail.Equals(request.accountEmail)
                    )
                    .AsNoTracking();

                //해당 이메일의 계정이 존재할 경우.
                if (accountQuery.Count() == 1)
                {
                    var account = accountQuery.FirstOrDefault();

                    //비밀번호 정보가 틀린 경우 종료.
                    if (account.AccountPassword.Equals(request.accountPassword) == false)
                    {
                        response.jwtAccess = response.jwtRefresh = null;
                        response.result = "invalid account info.";
                        dbPoolManager.Return(dbContext);
                        return response;
                    }

                    // 계정이 정지되었을 경우
                    if (account.AccountBanned == 1)
                    {
                        var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                        if (expire < 0) // 기간 만료 전
                        {
                            response.jwtAccess = response.jwtRefresh = null;
                            response.result = $"banned, {account.AccountBanExpire}, {account.AccountBanReason}";
                            dbPoolManager.Return(dbContext);
                            return response;
                        }

                        if (expire > 0) // 기간 만료 후
                        {
                            account.AccountBanned = 0;
                            dbContext.Entry(account).State = EntityState.Modified;
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    var userData = new UserData()
                    {
                        AccountUniqueId = account.AccountUniqueId,
                        AccountEmail = account.AccountEmail,
                        AuthLv = account.AccountAuthLv,
                        //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                        //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                    };

                    try
                    {
                        userData.UserLv = (int)account.UserInfo.UserLv;
                        userData.UserNickname = account.UserInfo.UserNickname;
                    }
                    catch (Exception e)
                    {
                        userData.UserLv = 0;
                        userData.UserNickname = null;
                        debugLogger.LogWarning($"login exception : {e}");
                    }

                    //새로운 jwt토큰 발행후 반환.
                    response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                    response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                    if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                    {
                        //로그인에 성공한 유저의 로그인 일시를 갱신.
                        var userQuery = dbContext.UserInfos
                            .Where(table =>
                                //email주소가 일치하는 row를 검색.
                                table.AccountUniqueId.Equals(userData.AccountUniqueId)
                            )
                            .AsNoTracking();

                        if (userQuery.Count() == 1)
                        {
                            var user = userQuery.FirstOrDefault();

                            var newLog = new UserSigninLog()
                            {
                                AccountUniqueId = account.AccountUniqueId,
                                UserNickname = user.UserNickname,
                                UserIp = request.userIP,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            await dbContext.UserSigninLogs.AddAsync(newLog);
                            dbContext.Entry(newLog).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            //최종 로그인 일시를 UTC시간 기준으로 갱신.
                            user.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(user).State = EntityState.Modified;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                //가입된 이메일이 아닌 경우.
                else if (accountQuery.Count() == 0)
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "need join account";
                }
                //잘못된 정보
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid account info.";
                }
            }
            //google, apple등의 oauth정보로 로그인할 경우.
            else if (authType.Contains("oauth"))
            {
                //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                IQueryable<AccountInfo> accountQuery;

                if (authType.Contains("google"))
                {
                    //oauth 토큰이 일치하는 row를 검색.
                    accountQuery = dbContext.AccountInfos
                        .Where(table => table.AccountOauthTokenGoogle.Equals(request.oauthToken))
                        .AsNoTracking();
                }
                else if (authType.Contains("apple"))
                {
                    //oauth 토큰이 일치하는 row를 검색.
                    accountQuery = dbContext.AccountInfos
                        .Where(table => table.AccountOauthTokenApple.Equals(request.oauthToken))
                        .AsNoTracking();
                }
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid authType";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //로그인 성공
                if (accountQuery.Count() == 1)
                {
                    var account = accountQuery.FirstOrDefault();

                    var userData = new UserData()
                    {
                        AccountUniqueId = account.AccountUniqueId,
                        AccountEmail = account.AccountEmail,
                        AuthLv = account.AccountAuthLv,
                        //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                        //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                    };

                    try
                    {
                        userData.UserLv = (int)account.UserInfo.UserLv;
                        userData.UserNickname = account.UserInfo.UserNickname;
                    }
                    catch (Exception)
                    {
                        userData.UserLv = 0;
                        userData.UserNickname = null;
                    }

                    //새로운 jwt토큰 발행후 반환.
                    response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                    response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                    if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                    {
                        //로그인에 성공한 유저의 로그인 일시를 갱신.
                        var userQuery = dbContext.UserInfos
                            .Where(table =>
                                //email주소가 일치하는 row를 검색.
                                table.AccountUniqueId.Equals(userData.AccountUniqueId)
                            )
                            .AsNoTracking();

                        if (userQuery.Count() == 1)
                        {
                            var user = userQuery.FirstOrDefault();

                            var newLog = new UserSigninLog()
                            {
                                AccountUniqueId = account.AccountUniqueId,
                                UserNickname = user.UserNickname,
                                UserIp = request.userIP,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            await dbContext.UserSigninLogs.AddAsync(newLog);
                            dbContext.Entry(newLog).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            //최종 로그인 일시를 UTC시간 기준으로 갱신.
                            user.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(user).State = EntityState.Modified;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                else if (accountQuery.Count() == 0)
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "need join account";
                }
                //잘못된 정보
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid oauthtoken info.";
                }
            }
            //jwt access token 갱신요청의 경우.
            else if (authType.Equals("jwt"))
            {
                //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
                SecurityToken tokenInfo = new JwtSecurityToken();

                if (JWTManager.CheckValidationJWT(request.jwtRefresh, out tokenInfo))
                {
                    //유효성 검증이 완료된 토큰 정보.
                    JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                    object accountUniqueId, authLv;
                    accountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");
                    //AuthLv = jwt.Payload.GetValueOrDefault("AuthLv");

                    //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                    var accountQuery = dbContext.AccountInfos
                        .Where(table =>
                            //jwt 토큰에서 지정된 AccountUniqueId기준으로 테이블 검색.
                            table.AccountUniqueId.Equals(uint.Parse(accountUniqueId.ToString()))
                        )
                        .AsNoTracking();

                    if (accountQuery.Count() == 1)
                    {
                        var account = accountQuery.FirstOrDefault();

                        var userData = new UserData()
                        {
                            AccountUniqueId = account.AccountUniqueId,
                            AccountEmail = account.AccountEmail,
                            AuthLv = account.AccountAuthLv,
                            //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                            //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                        };

                        try
                        {
                            userData.UserLv = (int)account.UserInfo.UserLv;
                            userData.UserNickname = account.UserInfo.UserNickname;
                        }
                        catch (Exception)
                        {
                            userData.UserLv = 0;
                            userData.UserNickname = null;
                        }

                        response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                        //기존 jwt_refresh 그대로 적용.
                        response.jwtRefresh = request.jwtRefresh;
                        response.result = "ok";


                        //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                        if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                        {
                            //로그인에 성공한 유저의 로그인 일시를 갱신.
                            var userQuery = dbContext.UserInfos
                                .Where(table =>
                                    //email주소가 일치하는 row를 검색.
                                    table.AccountUniqueId.Equals(userData.AccountUniqueId)
                                )
                                .AsNoTracking();

                            if (userQuery.Count() == 1)
                            {
                                var user = userQuery.FirstOrDefault();

                                var newLog = new UserSigninLog()
                                {
                                    AccountUniqueId = account.AccountUniqueId,
                                    UserNickname = user.UserNickname,
                                    UserIp = request.userIP,
                                    TimestampLastSignin = DateTime.UtcNow
                                };

                                await dbContext.UserSigninLogs.AddAsync(newLog);
                                dbContext.Entry(newLog).State = EntityState.Added;
                                await dbContext.SaveChangesAsync();

                                //최종 로그인 일시를 UTC시간 기준으로 갱신.
                                user.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(user).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    //잘못된 정보
                    else
                    {
                        response.jwtAccess = response.jwtRefresh = null;
                        response.result = "invalid jwt info";
                    }
                }
                //jwt refresh 토큰이 만료되었거나, 유효하지 않다면.
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "expiration or invalid jwt info. need login";
                }
            }
            //게스트 로그인의 경우, 일단 임시 발급된 jwt로 계정정보를 판단함.
            else if (authType.Equals("guest"))
            {
                var guestToken = request.oauthToken;

                //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        //jwt 토큰에서 지정된 AccountUniqueId기준으로 테이블 검색.
                        table.AccountGuestToken.Equals(guestToken)
                    )
                    .AsNoTracking();

                //로그인 성공
                if (accountQuery.Count() == 1)
                {
                    var account = accountQuery.FirstOrDefault();

                    var userData = new UserData()
                    {
                        AccountUniqueId = account.AccountUniqueId,
                        AccountEmail = account.AccountEmail,
                        AuthLv = account.AccountAuthLv,
                        //UserLv = (int)tableData.PlayerInfos.FirstOrDefault().PlayerLv,
                        //UserName = tableData.PlayerInfos.FirstOrDefault().PlayerNickname
                    };

                    try
                    {
                        userData.UserLv = (int)account.UserInfo.UserLv;
                        userData.UserNickname = account.UserInfo.UserNickname;
                    }
                    catch (Exception)
                    {
                        userData.UserLv = 0;
                        userData.UserNickname = null;
                    }

                    //새로운 jwt토큰 발행후 반환.
                    response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                    response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    //로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                    if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                    {
                        //로그인에 성공한 유저의 로그인 일시를 갱신.
                        var userQuery = dbContext.UserInfos
                            .Where(table =>
                                //email주소가 일치하는 row를 검색.
                                table.AccountUniqueId.Equals(userData.AccountUniqueId)
                            )
                            .AsNoTracking();

                        if (userQuery.Count() == 1)
                        {
                            var user = userQuery.FirstOrDefault();

                            var newLog = new UserSigninLog()
                            {
                                AccountUniqueId = account.AccountUniqueId,
                                UserNickname = user.UserNickname,
                                UserIp = request.userIP,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            await dbContext.UserSigninLogs.AddAsync(newLog);
                            dbContext.Entry(newLog).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            //최종 로그인 일시를 UTC시간 기준으로 갱신.
                            user.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(user).State = EntityState.Modified;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                else if (accountQuery.Count() == 0)
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "need join account";
                }
                //잘못된 정보
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid guest token info";
                }
            }
            //로그인 일시 갱신.
            else if (authType.Equals("update"))
            {
                //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
                SecurityToken tokenInfo = new JwtSecurityToken();

                if (JWTManager.CheckValidationJWT(request.jwtRefresh, out tokenInfo))
                {
                    //유효성 검증이 완료된 토큰 정보.
                    JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                    object accountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");

                    var accountQuery = dbContext.AccountInfos
                        .Where(table =>
                            table.AccountUniqueId.Equals(uint.Parse(accountUniqueId.ToString()))
                        )
                        .AsNoTracking();

                    if (accountQuery.Count() == 1)
                    {
                        var account = accountQuery.FirstOrDefault();

                        // 계정이 정지되었을 경우
                        if (account.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                            if (expire < 0) // 기간 만료 전
                            {
                                response.jwtAccess = response.jwtRefresh = null;
                                response.result = $"banned, {account.AccountBanExpire}, {account.AccountBanReason}";
                                dbPoolManager.Return(dbContext);
                                return response;
                            }

                            if (expire > 0) // 기간 만료 후
                            {
                                account.AccountBanned = 0;
                                dbContext.Entry(account).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                        }

                        //토큰 갱신작업이 완료되었다면, 로그인 중복방지를 위해 SessionManager에 JWT_AccessToken정보를 등록.
                        if (SessionManager.RegisterToken(account.AccountUniqueId, request.jwtRefresh))
                        {
                            //전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                            var userQuery = dbContext.UserInfos
                                .Where(table =>
                                    //jwt 토큰에서 지정된 AccountUniqueId기준으로 테이블 검색.
                                    table.AccountUniqueId.Equals(uint.Parse(accountUniqueId.ToString()))
                                )
                                .AsNoTracking();

                            if (userQuery.Count() == 1)
                            {
                                var user = userQuery.FirstOrDefault();

                                var newLog = new UserSigninLog()
                                {
                                    AccountUniqueId = account.AccountUniqueId,
                                    UserNickname = user.UserNickname,
                                    UserIp = request.userIP,
                                    TimestampLastSignin = DateTime.UtcNow
                                };

                                await dbContext.UserSigninLogs.AddAsync(newLog);
                                dbContext.Entry(newLog).State = EntityState.Added;
                                await dbContext.SaveChangesAsync();

                                //최종 로그인 일시를 UTC시간 기준으로 갱신.
                                user.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(user).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                                response.result = "ok";
                            }
                        }
                    }
                    //잘못된 정보
                    else
                    {
                        response.jwtAccess = response.jwtRefresh = null;
                        response.result = "invalid jwt info";
                    }
                }
                //jwt refresh 토큰이 만료되었거나, 유효하지 않다면.
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "expiration or invalid jwt info. need login";
                }
            }
            else
            {
                response.jwtAccess = response.jwtRefresh = null;
                response.result = "invalid authType";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 회원가입(계정입력) - 요청

        //요청 URL
        // http://serverAddress/signup/authnumber
        [HttpPost("signup/authnumber")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponseSignUpAuthNumber> Post(RequestSignUpAuthNumber request)
        {
            var response = new ResponseSignUpAuthNumber();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.signUpToken = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            //입력된 값이 이메일 값이 아닌 경우.
            if (emailPattern.IsMatch(request.accountEmail) == false)
            {
                response.signUpToken = null;
                response.result = "invalid email address";
                dbPoolManager.Return(dbContext);
                return response;
            }

            //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
            var accountQuery = dbContext.AccountInfos
                .Where(table =>
                    //email주소가 일치하는 row를 검색.
                    table.AccountEmail.Equals(request.accountEmail)
                )
                .AsNoTracking();

            //이메일이 존재할 경우.
            if (accountQuery.Count() >= 1)
            {
                response.signUpToken = null;
                response.result = "duplicate email";
            }
            else
            {
                var info = new EmailValidationInfo();

                //5분간 유효.
                info.expirateTime = DateTime.UtcNow.AddMinutes(5);
                //찾기 현재 진행 단계.
                info.currentStep = 1;
                info.emailAddress = request.accountEmail;
                info.validateToken = JWTManager.CreateNewJWT(new UserData(), JWTManager.JWTType.AccessToken);
                info.emailValidateConfirmNumber = new Random().Next(100000, 999998).ToString();


                EmailManager.RegisterSignUpInfo(info.validateToken, info);

                EmailManager.SendGmailSMTP(info.emailAddress
                    , "siogames 인증메일"
                    , "회원가입 인증 메일 안내"
                    , $"\n\n\n\n\n인증번호 : {info.emailValidateConfirmNumber}");

                response.signUpToken = info.validateToken;
                response.result = "ok";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 회원가입(계정입력) - 이메일 인증번호 인증

        //요청 URL
        // http://serverAddress/signup/authnumber/check
        [HttpPost("signup/authnumber/check")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponseSignUpAuthNumberCheck> Post(RequestSignUpAuthNumberCheck request)
        {
            var response = new ResponseSignUpAuthNumberCheck();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var result = EmailManager.GetSignUpInfo(request.signUpToken);

            //진행단계, 유효기간 체크.
            if (result != null && result.currentStep == 1 && result.expirateTime > DateTime.UtcNow)
            {
                if (result.emailValidateConfirmNumber.Equals(request.authNumber))
                {
                    result.currentStep = 2;
                    EmailManager.RegisterSignUpInfo(request.signUpToken, result);
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

        //요청 URL
        // http://serverAddress/signup
        [HttpPost("signup")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponseSignUp> Post(RequestSignUp request)
        {
            var response = new ResponseSignUp();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.jwtAccess = response.jwtRefresh = null;
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


            var authType = request.authType.ToLower();

            if (authType.Equals("account"))
            {
                //입력된 값이 이메일 값이 아닌 경우.
                if (emailPattern.IsMatch(request.accountEmail) == false)
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid email address";
                    dbPoolManager.Return(dbContext);
                    return response;
                }


                var emailValidationInfo = EmailManager.GetSignUpInfo(request.signUpToken);

                //진행단계, 유효기간 체크.
                if (emailValidationInfo != null && emailValidationInfo.currentStep == 2)
                {
                    //MariaDB+EntityFramework조합 에서 Transaction사용시 CreateExecutionStrategy 활용하여 실행해야함.
                    var strategy = dbContext.Database.CreateExecutionStrategy();

                    Func<Task> dbTransactionOperation = async () =>
                    {
                        using (var transaction = await dbContext.Database.BeginTransactionAsync())
                        {
                            var newAccount = new AccountInfo()
                            {
                                AccountEmail = request.accountEmail,
                                AccountPassword = request.accountPassword,
                                AccountAuthLv = (byte)AuthLv.UserAccount
                            };

                            //전달된 회원가입 정보로 db insert 실행.
                            try
                            {
                                //transaction 내에서 insert시, 
                                //innodb_autoinc_lock_mode = 0; 의 값을 0으로 해야한다. (AutoIncrement 값 증가 이슈)

                                await dbContext.AccountInfos.AddAsync(newAccount);
                                dbContext.Entry(newAccount).State = EntityState.Added;
                                await dbContext.SaveChangesAsync();

                                //insert 성공시 player 생성.
                                var newUser = new UserInfo()
                                {
                                    AccountUniqueId = newAccount.AccountUniqueId,
                                    UserLv = 1,
                                    TimestampCreated = DateTime.UtcNow,
                                    TimestampLastSignin = DateTime.UtcNow
                                };

                                await dbContext.UserInfos.AddAsync(newUser);
                                dbContext.Entry(newUser).State = EntityState.Added;
                                await dbContext.SaveChangesAsync();

                                await transaction.CommitAsync();

                                UserData userdata = new UserData()
                                {
                                    AccountUniqueId = newAccount.AccountUniqueId,
                                    AuthLv = newAccount.AccountAuthLv
                                };

                                //새로운 jwt토큰 발행후 반환.
                                response.jwtAccess = JWTManager.CreateNewJWT(userdata, JWTManager.JWTType.AccessToken);
                                response.jwtRefresh =
                                    JWTManager.CreateNewJWT(userdata, JWTManager.JWTType.RefreshToken);
                                response.result = "ok";

                                var newLog = new UserSigninLog()
                                {
                                    AccountUniqueId = newAccount.AccountUniqueId,
                                    UserIp = request.userIP,
                                    TimestampLastSignin = DateTime.UtcNow
                                };

                                await dbContext.UserSigninLogs.AddAsync(newLog);
                                dbContext.Entry(newLog).State = EntityState.Added;
                                await dbContext.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                await transaction.RollbackAsync();

                                response.jwtAccess = response.jwtRefresh = null;

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

                    EmailManager.RemoveSignUpInfo(request.signUpToken);

                    //transaction 쿼리 실행.
                    await strategy.ExecuteAsync(dbTransactionOperation);
                }
                //잘못된 데이터
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid token.";
                    dbPoolManager.Return(dbContext);

                    EmailManager.RemoveSignUpInfo(request.signUpToken);
                    return response;
                }
            }
            else if (authType.Contains("oauth"))
            {
                var newAccount = new AccountInfo()
                {
                    AccountAuthLv = (byte)AuthLv.UserAccount
                };

                //로그인한 OAuth 타입에 맞춰 값 입력.
                if (authType.Contains("google"))
                {
                    newAccount.AccountOauthTokenGoogle = request.oauthToken;
                }
                else if (authType.Contains("apple"))
                {
                    newAccount.AccountOauthTokenApple = request.oauthToken;
                }
                else
                {
                    response.jwtAccess = response.jwtRefresh = null;
                    response.result = "invalid authType";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //MariaDB+EntityFramework조합 에서 Transaction사용시 CreateExecutionStrategy 활용하여 실행해야함.
                var strategy = dbContext.Database.CreateExecutionStrategy();
                Func<Task> dbTransactionOperation = async () =>
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        //전달된 회원가입 정보로 db insert 실행.
                        try
                        {
                            //transaction 내에서 insert시, 
                            //innodb_autoinc_lock_mode = 0; 의 값을 0으로 해야한다. (AutoIncrement 값 증가 이슈)

                            await dbContext.AccountInfos.AddAsync(newAccount);
                            await dbContext.SaveChangesAsync();

                            //insert 성공시 player 생성.
                            var newUser = new UserInfo()
                            {
                                AccountUniqueId = newAccount.AccountUniqueId,
                                TimestampCreated = DateTime.UtcNow,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            await dbContext.UserInfos.AddAsync(newUser);
                            dbContext.Entry(newUser).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            await transaction.CommitAsync();

                            var userData = new UserData()
                            {
                                AccountUniqueId = newAccount.AccountUniqueId,
                                AuthLv = newAccount.AccountAuthLv
                            };

                            //새로운 jwt토큰 발행후 반환.
                            response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                            response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                            response.result = "ok";
                        }
                        catch (Exception e)
                        {
                            await transaction.RollbackAsync();

                            response.jwtAccess = response.jwtRefresh = null;

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
                await strategy.ExecuteAsync(dbTransactionOperation);
            }
            else if (authType.Equals("guest"))
            {
                //MariaDB+EntityFramework조합 에서 Transaction사용시 CreateExecutionStrategy 활용하여 실행해야함.
                var strategy = dbContext.Database.CreateExecutionStrategy();
                Func<Task> dbTransactionOperation = async () =>
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        AccountInfo newAccount = new AccountInfo()
                        {
                            AccountAuthLv = (byte)AuthLv.UserGuest,
                            AccountGuestToken = request.oauthToken
                        };

                        //
                        if (string.IsNullOrEmpty(request.oauthToken))
                            newAccount.AccountGuestToken = Guid.NewGuid().ToString();

                        //전달된 회원가입 정보로 db insert 실행.
                        try
                        {
                            await dbContext.AccountInfos.AddAsync(newAccount);
                            dbContext.Entry(newAccount).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            //insert 성공시 player 생성.
                            var newUser = new UserInfo()
                            {
                                AccountUniqueId = newAccount.AccountUniqueId,
                                TimestampCreated = DateTime.UtcNow,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            await dbContext.UserInfos.AddAsync(newUser);
                            dbContext.Entry(newUser).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            await transaction.CommitAsync();

                            var userData = new UserData()
                            {
                                AccountUniqueId = newAccount.AccountUniqueId,
                                AuthLv = newAccount.AccountAuthLv
                            };

                            //새로운 jwt토큰 발행후 반환.
                            response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                            response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                            response.result = "ok";
                        }
                        catch (Exception e)
                        {
                            await transaction.RollbackAsync();

                            response.jwtAccess = response.jwtRefresh = null;

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
                await strategy.ExecuteAsync(dbTransactionOperation);
            }
            else
            {
                response.jwtAccess = response.jwtRefresh = null;
                response.result = "invalid authType";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 비밀번호 찾기(계정입력) - 요청

        //요청 URL
        // http://serverAddress/passwordfind/authnumber
        [HttpPost("passwordfind/authnumber")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponsePasswordFindAuthNumber> Post(RequestPasswordFindAuthNumber request)
        {
            var responsePasswordFind = new ResponsePasswordFindAuthNumber();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                responsePasswordFind.passwordFindToken = null;
                responsePasswordFind.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return responsePasswordFind;
            }

            //입력된 값이 이메일 값이 아닌 경우.
            if (emailPattern.IsMatch(request.accountEmail) == false)
            {
                responsePasswordFind.passwordFindToken = null;
                responsePasswordFind.result = "invalid email address";
                dbPoolManager.Return(dbContext);
                return responsePasswordFind;
            }

            //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
            var accountQuery = dbContext.AccountInfos
                .Where(table =>
                    //email주소가 일치하는 row를 검색.
                    table.AccountEmail.Equals(request.accountEmail)
                )
                .AsNoTracking();

            //이메일이 존재할 경우.
            if (accountQuery.Count() == 1)
            {
                EmailValidationInfo info = new EmailValidationInfo();

                //5분간 유효.
                info.expirateTime = DateTime.UtcNow.AddMinutes(5);
                //찾기 현재 진행 단계.
                info.currentStep = 1;
                info.emailAddress = request.accountEmail;
                info.validateToken = JWTManager.CreateNewJWT(new UserData(), JWTManager.JWTType.AccessToken);
                info.emailValidateConfirmNumber = new Random().Next(100000, 999998).ToString();

                EmailManager.RegisterPasswordFindInfo(info.validateToken, info);

                EmailManager.SendGmailSMTP(info.emailAddress
                    , "siogames 인증메일"
                    , "비밀번호 찾기 인증 메일 안내"
                    , $"\n\n\n\n\n인증번호 : {info.emailValidateConfirmNumber}");

                responsePasswordFind.passwordFindToken = info.validateToken;
                responsePasswordFind.result = "ok";
            }
            else
            {
                responsePasswordFind.passwordFindToken = null;
                responsePasswordFind.result = "not find email";
            }

            dbPoolManager.Return(dbContext);
            return responsePasswordFind;
        }

        #endregion

        #region 비밀번호 찾기(계정입력) - 인증번호 인증

        //요청 URL
        // http://serverAddress/passwordfind/authnumber/check
        [HttpPost("passwordfind/authnumber/check")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponsePasswordFindAuthNumberCheck> Post(RequestPasswordFindAuthNumberCheck request)
        {
            var responsePasswordFind = new ResponsePasswordFindAuthNumberCheck();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                responsePasswordFind.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return responsePasswordFind;
            }

            var result = EmailManager.GetPasswordFindInfo(request.passwordFindToken);

            //진행단계, 유효기간 체크.
            if (result != null && result.currentStep == 1 && result.expirateTime > DateTime.UtcNow)
            {
                if (result.emailValidateConfirmNumber.Equals(request.authNumber))
                {
                    result.currentStep = 2;
                    EmailManager.RegisterPasswordFindInfo(request.passwordFindToken, result);
                    responsePasswordFind.result = "ok";
                }
                else
                {
                    //인증번호 재입력 필요.
                    responsePasswordFind.result = "incorrect auth_number";
                }
            }
            //잘못된 데이터
            else
            {
                responsePasswordFind.result = "invalid token.";
            }


            dbPoolManager.Return(dbContext);
            return responsePasswordFind;
        }

        #endregion

        #region 비밀번호 찾기(계정입력) - 비밀번호 변경요청

        //요청 URL
        // http://serverAddress/passwordfind/change
        [HttpPost("passwordfind/change")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponsePasswordChange> Post(RequestPasswordChange request)
        {
            var response = new ResponsePasswordChange();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            if (string.IsNullOrEmpty(request.accountPassword))
            {
                response.result = "invalid password value";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var result = EmailManager.GetPasswordFindInfo(request.passwordFindToken);

            //진행단계, 유효기간 체크.
            if (result != null && result.currentStep == 2)
            {
                //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        //email주소가 일치하는 row를 검색.
                        table.AccountEmail.Equals(result.emailAddress)
                    )
                    .AsNoTracking();

                if (accountQuery.Count() == 1)
                {
                    var account = accountQuery.FirstOrDefault();

                    //비밀번호 변경사항 db에 반영
                    account.AccountPassword = request.accountPassword;
                    dbContext.Entry(account).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    response.result = "ok";
                }
                else
                {
                    response.result = "server Error.";
                }

                EmailManager.RemovePasswordFindInfo(request.passwordFindToken);
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