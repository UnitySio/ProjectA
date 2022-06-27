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

        // 이메일 정규식 (aaa@gmail.com)
        Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");

        #region 로그인 이메일 인증 메일 요청

        // 요청 URL
        // https://serverAddress/signin/authnumber
        [HttpPost("signin/authnumber")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseSendSignInAuthNumber> Post(RequestSendSignInAuthNumber request)
        {
            var response = new ResponseSendSignInAuthNumber();
            // DB에 접속하여 데이터를 조작하는 DBContext객체
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.signInToken = null;
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            // 입력된 값이 이메일 값이 아닌 경우
            if (emailPattern.IsMatch(request.accountEmail) == false)
            {
                response.signInToken = null;
                response.result = "invalid email address";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var info = new EmailValidationInfo();

            // 5분간 유효
            info.expirateTime = DateTime.UtcNow.AddMinutes(5);
            // 현재 진행 단계
            info.currentStep = 1;
            info.emailAddress = request.accountEmail;
            info.validateToken = JWTManager.CreateNewJWT(new UserData(), JWTManager.JWTType.AccessToken);
            info.emailValidateAuthNumber = new Random().Next(100000, 999998).ToString();

            EmailManager.RegisterSignInInfo(info.validateToken, info);
            EmailManager.SendGmailSMTP(info.emailAddress,
                "System",
                "이메일 인증 안내",
                $"<b>인증번호: {info.emailValidateAuthNumber}</b><br>해당 인증번호는 5분간 유효합니다.<br>본 메일은 발신 전용 메일입니다.");

            response.signInToken = info.validateToken;
            response.result = "ok";

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 로그인 이메일 인증번호 검증

        // 요청 URL
        // https://serverAddress/signin/authnumber/verify
        [HttpPost("signin/authnumber/verify")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseVerifySignInAuthNumber> Post(RequestVerifySignInAuthNumber request)
        {
            var response = new ResponseVerifySignInAuthNumber();
            // DB에 접속하여 데이터를 조작하는 DBContext객체
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var result = EmailManager.GetSignInInfo(request.signInToken);

            // 진행 단계, 유효기간 체크
            if (result != null && result.currentStep == 1 && result.expirateTime > DateTime.UtcNow)
            {
                if (result.emailValidateAuthNumber.Equals(request.authNumber))
                {
                    result.currentStep = 2;
                    EmailManager.RegisterSignInInfo(request.signInToken, result);
                    response.result = "ok";
                }
                else
                {
                    // 인증번호 재입력 필요
                    response.result = "invalid auth number";
                }
            }
            else
            {
                // 잘못된 데이터
                response.result = "invalid sign in token";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 로그인

        // 요청 URL
        // https://serverAddress/signin
        [HttpPost("signin")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseSignIn> Post(RequestSignIn request)
        {
            var response = new ResponseSignIn();
            // DB에 접속하여 데이터를 조작하는 DBContext객체
            var dbContext = dbPoolManager.Rent();

            if (request == null || string.IsNullOrEmpty(request.authType))
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            var authType = request.authType.ToLower();

            // 계정 로그인
            if (authType.Equals("account"))
            {
                var emailValidationInfo = EmailManager.GetSignInInfo(request.signInToken);

                if (emailPattern.IsMatch(request.accountEmail) == false)
                {
                    response.jwtAccess = null;
                    response.jwtRefresh = null;
                    response.result = "invalid email address";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                if (emailValidationInfo != null && emailValidationInfo.currentStep == 2)
                {
                    var accountQuery = dbContext.AccountInfos
                        .Where(table =>
                            table.AccountEmail.Equals(request.accountEmail)
                        ).AsNoTracking();

                    // 해당 계정이 존재한다면 로그인
                    if (accountQuery.Any())
                    {
                        var account = accountQuery.FirstOrDefault();

                        // 계정이 정지되었을 경우
                        if (account.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                            if (expire > 0)
                            {
                                // 정지 해제
                                account.AccountBanned = 0;
                                dbContext.Entry(account).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                response.jwtAccess = null;
                                response.jwtRefresh = null;
                                response.result = "account banned";
                                dbPoolManager.Return(dbContext);

                                EmailManager.RemoveSignInInfo(request.signInToken);
                                return response;
                            }
                        }

                        var userData = new UserData()
                        {
                            AccountUniqueID = account.AccountUniqueId,
                            AuthLv = account.AccountAuthLv
                        };

                        // 새로운 jwt 토큰 발행후 반환
                        response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                        response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                        response.result = "ok";

                        // 로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 정보를 등록
                        if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                        {
                            var userQuery = dbContext.UserInfos
                                .Where(table =>
                                    table.AccountUniqueId.Equals(userData.AccountUniqueID)
                                ).AsNoTracking();

                            if (userQuery.Any())
                            {
                                var user = userQuery.FirstOrDefault();

                                // 로그인 기록
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

                                // 최종 로그인 일시 갱신
                                user.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(user).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    else // 해당 계정이 존재하지 않는다면 회원가입
                    {
                        var strategy = dbContext.Database.CreateExecutionStrategy();

                        Func<Task> dbTransactionOperation = async () =>
                        {
                            using (var transaction = await dbContext.Database.BeginTransactionAsync())
                            {
                                var newAccount = new AccountInfo()
                                {
                                    AccountEmail = request.accountEmail,
                                    AccountAuthLv = (byte)AuthLv.UserAccount
                                };

                                // 전달된 회원가입 정보로 db insert 실행
                                try
                                {
                                    await dbContext.AccountInfos.AddAsync(newAccount);
                                    dbContext.Entry(newAccount).State = EntityState.Added;
                                    await dbContext.SaveChangesAsync();

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

                                    var userData = new UserData()
                                    {
                                        AccountUniqueID = newAccount.AccountUniqueId,
                                        AuthLv = newAccount.AccountAuthLv
                                    };

                                    // 새로운 jwt 토큰 발행후 반환
                                    response.jwtAccess =
                                        JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                                    response.jwtRefresh =
                                        JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
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

                                    response.jwtAccess = null;
                                    response.jwtRefresh = null;

                                    if (e.ToString().Contains("Duplicate entry"))
                                        response.result = "duplicate account";
                                    else
                                    {
                                        response.result = "db error";
                                        debugLogger.LogError($"signup exception: {e}");
                                    }
                                }
                            }
                        };

                        await strategy.ExecuteAsync(dbTransactionOperation);
                    }

                    EmailManager.RemoveSignInInfo(request.signInToken);
                }
                else
                {
                    response.jwtAccess = null;
                    response.jwtRefresh = null;
                    response.result = "invalid signin token";
                    dbPoolManager.Return(dbContext);

                    EmailManager.RemoveSignInInfo(request.signInToken);
                    return response;
                }
            }
            else if (authType.Equals("guest")) // 게스트 로그인의 경우, 일단 임시 발급된 jwt로 계정정보를 판단
            {
                var guestToken = request.oauthToken;

                // 전달된 로그인 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        table.AccountGuestToken.Equals(guestToken)
                    ).AsNoTracking();

                if (accountQuery.Any())
                {
                    var account = accountQuery.FirstOrDefault();

                    var userData = new UserData()
                    {
                        AccountUniqueID = account.AccountUniqueId,
                        AuthLv = account.AccountAuthLv
                    };

                    // 새로운 jwt 토큰 발행후 반환
                    response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                    response.jwtRefresh = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
                    response.result = "ok";

                    // 로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 정보를 등록
                    if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                    {
                        var userQuery = dbContext.UserInfos
                            .Where(table =>
                                table.AccountUniqueId.Equals(userData.AccountUniqueID)
                            ).AsNoTracking();

                        if (userQuery.Any())
                        {
                            var user = userQuery.FirstOrDefault();

                            // 로그인 기록
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

                            // 최종 로그인 일시 갱신
                            user.TimestampLastSignin = DateTime.UtcNow;
                            dbContext.Entry(user).State = EntityState.Modified;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    var strategy = dbContext.Database.CreateExecutionStrategy();

                    Func<Task> dbTransactionOperation = async () =>
                    {
                        using (var transaction = await dbContext.Database.BeginTransactionAsync())
                        {
                            var newAccount = new AccountInfo()
                            {
                                AccountAuthLv = (byte)AuthLv.UserGuest,
                                AccountGuestToken = guestToken
                            };

                            if (string.IsNullOrEmpty(guestToken))
                                newAccount.AccountGuestToken = Guid.NewGuid().ToString();

                            // 전달된 회원가입 정보로 db insert 실행
                            try
                            {
                                await dbContext.AccountInfos.AddAsync(newAccount);
                                dbContext.Entry(newAccount).State = EntityState.Added;
                                await dbContext.SaveChangesAsync();

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
                                    AccountUniqueID = newAccount.AccountUniqueId,
                                    AuthLv = newAccount.AccountAuthLv
                                };

                                // 새로운 jwt 토큰 발행후 반환
                                response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                                response.jwtRefresh =
                                    JWTManager.CreateNewJWT(userData, JWTManager.JWTType.RefreshToken);
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

                                response.jwtAccess = null;
                                response.jwtRefresh = null;

                                if (e.ToString().Contains("Duplicate entry"))
                                    response.result = "duplicate account";
                                else
                                {
                                    response.result = "db error";
                                    debugLogger.LogError($"signup exception: {e}");
                                }
                            }
                        }
                    };

                    await strategy.ExecuteAsync(dbTransactionOperation);
                }
            }
            else if (authType.Equals("update")) // 로그인 일시 갱신
            {
                SecurityToken tokenInfo = new JwtSecurityToken();

                if (JWTManager.CheckValidationJWT(request.jwtRefresh, out tokenInfo))
                {
                    // 유효성 검증이 완료된 토큰 정보
                    var jwt = tokenInfo as JwtSecurityToken;

                    object accountUniqueID = jwt.Payload.GetValueOrDefault("AccountUniqueId");

                    var accountQuery = dbContext.AccountInfos
                        .Where(table =>
                            table.AccountUniqueId.Equals(uint.Parse(accountUniqueID.ToString()))
                        ).AsNoTracking();

                    if (accountQuery.Any())
                    {
                        var account = accountQuery.FirstOrDefault();

                        // 계정이 정지되었을 경우
                        if (account.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                            if (expire > 0)
                            {
                                // 정지 해제
                                account.AccountBanned = 0;
                                dbContext.Entry(account).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                response.jwtAccess = null;
                                response.jwtRefresh = null;
                                response.result = "account banned";
                                dbPoolManager.Return(dbContext);
                                return response;
                            }
                        }

                        // 토큰 갱신작업이 완료되었다면, 로그인 중복방지를 위해 SessionManager에 정보를 등록
                        if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                        {
                            var userQuery = dbContext.UserInfos
                                .Where(table =>
                                    table.AccountUniqueId.Equals(uint.Parse(accountUniqueID.ToString()))
                                ).AsNoTracking();

                            if (userQuery.Any())
                            {
                                var user = userQuery.FirstOrDefault();

                                // 로그인 기록
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

                                // 최종 로그인 일시 갱신
                                user.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(user).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                                response.result = "ok";
                            }
                        }
                    }
                    else // 잘못된 정보
                    {
                        response.jwtAccess = null;
                        response.jwtRefresh = null;
                        response.result = "invalid jwt info";
                    }
                }
                else // jwt refresh 토큰이 만료되었거나, 유효하지 않다면
                {
                    response.jwtAccess = null;
                    response.jwtRefresh = null;
                    response.result = "expiration or invalid jwt info";
                }
            }
            else if (authType.Equals("jwt")) // jwt access 토큰을 갱신 요청
            {
                SecurityToken tokenInfo = new JwtSecurityToken();

                if (JWTManager.CheckValidationJWT(request.jwtRefresh, out tokenInfo))
                {
                    var jwt = tokenInfo as JwtSecurityToken;

                    object accountUniqueID = jwt.Payload.GetValueOrDefault("AccountUniqueId");

                    var accountQuery = dbContext.AccountInfos
                        .Where(table =>
                            table.AccountUniqueId.Equals(uint.Parse(accountUniqueID.ToString()))
                        ).AsNoTracking();

                    if (accountQuery.Any())
                    {
                        var account = accountQuery.FirstOrDefault();

                        // 계정이 정지되었을 경우
                        if (account.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                            if (expire > 0)
                            {
                                // 정지 해제
                                account.AccountBanned = 0;
                                dbContext.Entry(account).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                response.jwtAccess = null;
                                response.jwtRefresh = null;
                                response.result = "account banned";
                                dbPoolManager.Return(dbContext);
                                return response;
                            }
                        }

                        var userData = new UserData()
                        {
                            AccountUniqueID = account.AccountUniqueId,
                            AccountEmail = account.AccountEmail,
                            AuthLv = account.AccountAuthLv
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
                        }

                        response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                        // 기존 jwtRefresh 토큰을 그대로 사용
                        response.jwtRefresh = request.jwtRefresh;
                        response.result = "ok";


                        // 로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 정보를 등록
                        if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                        {
                            var userQuery = dbContext.UserInfos
                                .Where(table =>
                                    table.AccountUniqueId.Equals(userData.AccountUniqueID)
                                ).AsNoTracking();

                            if (userQuery.Any())
                            {
                                var user = userQuery.FirstOrDefault();

                                // 로그인 기록
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

                                // 최종 로그인 일시 갱신
                                user.TimestampLastSignin = DateTime.UtcNow;
                                dbContext.Entry(user).State = EntityState.Modified;
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        response.jwtAccess = null;
                        response.jwtRefresh = null;
                        response.result = "invalid jwt info";
                    }
                }
                else
                {
                    response.jwtAccess = null;
                    response.jwtRefresh = null;
                    response.result = "expiration or invalid jwt info";
                }
            }
            else
            {
                response.jwtAccess = null;
                response.jwtRefresh = null;
                response.result = "invalid auth type";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
    }
}