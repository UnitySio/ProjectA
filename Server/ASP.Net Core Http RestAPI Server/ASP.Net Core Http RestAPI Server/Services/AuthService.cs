using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ASP.Net_Core_Http_RestAPI_Server.Services
{
    public class AuthService
    {
        private ILogger<AuthService> log;

        private TransactionService transactionService;
        private AccountInfoMapper accountInfoMapper;
        private UserCharacterInfoMapper userCharacterInfoMapper;
        private UserInfoMapper userInfoMapper;
        private UserSigninLogMapper userSigninLogMapper;

        //Construct Dependency Injections
        public AuthService(ILogger<AuthService> log, TransactionService transactionService, AccountInfoMapper accountInfoMapper, UserCharacterInfoMapper userCharacterInfoMapper, UserInfoMapper userInfoMapper, UserSigninLogMapper userSigninLogMapper)
        {
            this.log = log;
            this.transactionService = transactionService;
            this.accountInfoMapper = accountInfoMapper;
            this.userCharacterInfoMapper = userCharacterInfoMapper;
            this.userInfoMapper = userInfoMapper;
            this.userSigninLogMapper = userSigninLogMapper;
        }

        
        public async Task<ResponseSignIn> signin(RequestSignIn request)
        {
            var response = new ResponseSignIn();

            if (request == null || string.IsNullOrEmpty(request.authType))
            {
                response.result = "invalid data";
                return response;
            }
        
            var authType = request.authType.ToLower();
        
            if (authType.Equals("guest")) // 게스트 로그인의 경우, UUID로 판단
            {
                var guestToken = request.oauthToken;
                
                // 전달된 게스트 토큰이 없을 경우
                if (string.IsNullOrEmpty(guestToken))
                    guestToken = Guid.NewGuid().ToString();

                //디비에서 accountInfo 조회
                var account = accountInfoMapper.GetAccountInfoByGuestToken(guestToken);
                
                //디비에 값이 존재한다면
                if (account != null)
                {
                    // 계정이 정지되었을 경우
                    if (account.AccountBanned == 1)
                    {
                        var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                        if (expire > 0)
                        {
                            // 정지 해제
                            account.AccountBanned = 0;
                            
                            //db에 반영
                            int affectRows = await accountInfoMapper.UpdateAccountInfo(account);
                            if (affectRows != 1)
                            {
                                throw new Exception("AuthService::signin() db update failure! accountInfoMapper.GetAccountInfoByGuestToken() ");
                            }
                        }
                        else
                        {
                            response.jwtAccess = null;
                            response.jwtRefresh = null;
                            response.result = "account banned";
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
                        //디비에 userInfo 조회
                        var user = userInfoMapper.GetUserInfoByAccountUniqueID(userData.AccountUniqueID.ToString());
        
                        //값이 존재한다면
                        if (user != null)
                        {
                            // 로그인 기록
                            var newLog = new UserSigninLog()
                            {
                                AccountUniqueId = account.AccountUniqueId,
                                UserNickname = user.UserNickname,
                                UserIp = request.userIP,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            int affectRows = await userSigninLogMapper.InsertUserSigninLog(newLog);
                            if (affectRows != 1)
                            {
                                throw new Exception("AuthService::signin() db insert failure! userSigninLogMapper.InsertUserSigninLog() ");
                            }
                            
                            
                            // 최종 로그인 일시 갱신
                            user.TimestampLastSignin = DateTime.UtcNow;
                            
                            affectRows = await userInfoMapper.UpdateUserInfo(user);
                            if (affectRows != 1)
                            {
                                throw new Exception("AuthService::signin() db update failure! userInfoMapper.UpdateUserInfo() ");
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        //로그인 처리 관련 transaction method
                        transactionService.RunTaskWithTx(async delegate
                        {
                            var newAccount = new AccountInfo()
                            {
                                AccountAuthLv = (byte)AuthLv.UserGuest,
                                AccountGuestToken = guestToken
                            };
                            
                            // 전달된 회원가입 정보로 db insert 실행
                            int affectRows = await accountInfoMapper.InsertAccountInfo(newAccount);
                            if (affectRows != 1)
                            {
                                throw new Exception("AuthService::signin() db insert failure! accountInfoMapper.InsertAccountInfo() ");
                            }
                            
                            
                            var newUser = new UserInfo()
                            {
                                AccountUniqueId = newAccount.AccountUniqueId,
                                TimestampCreated = DateTime.UtcNow,
                                TimestampLastSignin = DateTime.UtcNow
                            };

                            affectRows = await userInfoMapper.InsertUserInfo(newUser);
                            if (affectRows != 1)
                            {
                                throw new Exception("AuthService::signin() db insert failure! userInfoMapper.InsertUserInfo() ");
                            }

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

                            affectRows = await userSigninLogMapper.InsertUserSigninLog(newLog);
                            if (affectRows != 1)
                            {
                                throw new Exception("AuthService::signin() db insert failure! userSigninLogMapper.InsertUserSigninLog(2) ");
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        response.jwtAccess = null;
                        response.jwtRefresh = null;

                        if (e.ToString().Contains("Duplicate entry"))
                            response.result = "duplicate account";
                        else
                        {
                            response.result = "db error";
                            log.LogError($"signup exception: {e}");
                        }
                    }
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
        
                    //db에서 accountInfo 조회.
                    AccountInfo account = accountInfoMapper.GetAccountInfoByAccountUniqueID(accountUniqueID.ToString());
                    
                    if (account != null)
                    {
                        // 계정이 정지되었을 경우
                        if (account.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                            if (expire > 0)
                            {
                                // 정지 해제
                                account.AccountBanned = 0;

                                //db에 반영
                                int affectRows = await accountInfoMapper.UpdateAccountInfo(account);
                                if (affectRows != 1)
                                {
                                    throw new Exception("AuthService::signin() db update failure! accountInfoMapper.GetAccountInfoByGuestToken() ");
                                }
                            }
                            else
                            {
                                response.jwtAccess = null;
                                response.jwtRefresh = null;
                                response.result = "account banned";
                                return response;
                            }
                        }
        
                        // 토큰 갱신작업이 완료되었다면, 로그인 중복방지를 위해 SessionManager에 정보를 등록
                        if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                        {
                            var user = userInfoMapper.GetUserInfoByAccountUniqueID(accountUniqueID.ToString());
                            if (user != null)
                            {
                                // 로그인 기록
                                var newLog = new UserSigninLog()
                                {
                                    AccountUniqueId = account.AccountUniqueId,
                                    UserNickname = user.UserNickname,
                                    UserIp = request.userIP,
                                    TimestampLastSignin = DateTime.UtcNow
                                };

                                await userSigninLogMapper.InsertUserSigninLog(newLog);
        
                                // 최종 로그인 일시 갱신
                                user.TimestampLastSignin = DateTime.UtcNow;

                                await userInfoMapper.UpdateUserInfo(user);
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
        
                    
                    var account = accountInfoMapper.GetAccountInfoByAccountUniqueID(accountUniqueID.ToString());
                    if (account != null)
                    {
                        // 계정이 정지되었을 경우
                        if (account.AccountBanned == 1)
                        {
                            var expire = DateTime.Compare(DateTime.UtcNow, (DateTime)account.AccountBanExpire);
                            if (expire > 0)
                            {
                                // 정지 해제
                                account.AccountBanned = 0;

                                await accountInfoMapper.UpdateAccountInfo(account);
                            }
                            else
                            {
                                response.jwtAccess = null;
                                response.jwtRefresh = null;
                                response.result = "account banned";
                                return response;
                            }
                        }
        
                        var userData = new UserData()
                        {
                            AccountUniqueID = account.AccountUniqueId,
                            AuthLv = account.AccountAuthLv
                        };
        
                        response.jwtAccess = JWTManager.CreateNewJWT(userData, JWTManager.JWTType.AccessToken);
                        // 기존 jwtRefresh 토큰을 그대로 사용
                        response.jwtRefresh = request.jwtRefresh;
                        response.result = "ok";
        
        
                        // 로그인 과정이 성공적으로 완료되었다면, 로그인 중복방지를 위해 SessionManager에 정보를 등록
                        if (SessionManager.RegisterToken(account.AccountUniqueId, response.jwtAccess))
                        {
                            var user = userInfoMapper.GetUserInfoByAccountUniqueID(accountUniqueID.ToString());
                            if (user != null)
                            {
                                // 로그인 기록
                                var newLog = new UserSigninLog()
                                {
                                    AccountUniqueId = account.AccountUniqueId,
                                    UserNickname = user.UserNickname,
                                    UserIp = request.userIP,
                                    TimestampLastSignin = DateTime.UtcNow
                                };
        
                                await userSigninLogMapper.InsertUserSigninLog(newLog);
        
                                // 최종 로그인 일시 갱신
                                user.TimestampLastSignin = DateTime.UtcNow;
                                
                                await userInfoMapper.UpdateUserInfo(user);
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
            
            return response;
        }

    } //end of class
}
