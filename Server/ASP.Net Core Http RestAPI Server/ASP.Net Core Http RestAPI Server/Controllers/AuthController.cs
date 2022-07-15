using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts.Models;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using ASP.Net_Core_Http_RestAPI_Server.Services;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private ILogger<AuthController> log;
        private AuthService authService;
        private IDbContextFactory<PrimaryDataSource> dbContextFactory;

        //Construct Dependency Injections
        public AuthController(ILogger<AuthController> logger, AuthService authService, IDbContextFactory<PrimaryDataSource> dbContextFactory)
        {
            this.authService = authService;
            this.log = logger;
            this.dbContextFactory = dbContextFactory;
        }

        [SqlMethod()] 
        void test()
        {
        
        }

        
		// private static string ConvertToInsertIntoSQL(object obj)
		// {
  //           PrimaryDataSource dbContext = dbContextFactory.CreateDbContext();
  //
  //           var aa = dbContext.UserInfos.FromSqlInterpolated($"select * from user_info where 1=1").ToList();
  //
  //           int affectRows = dbContext.Database.ExecuteSqlInterpolated($"insert into (a,b,c) values ()");
  //
  //
  //           var firstName = "John";
  //           var id = 12;
  //           int affectRow1 = dbContext.Database.ExecuteSqlInterpolated($"Update [User] SET FirstName = {firstName} WHERE Id = {id}");
  //
  //
  //           var test = dbContext.GetType().GetCustomAttribute(typeof(HttpPostAttribute)) as HttpPostAttribute;
  //
  //           var test2 = response.GetType().GetMethod("").GetCustomAttribute(typeof(HttpPostAttribute)) as HttpPostAttribute;
  //
  //           var data = test2.Template;
  //
  //           var property = response.GetType().GetProperties();
  //
  //           var attr = property[0].GetCustomAttribute(typeof(HttpPostAttribute)) as HttpPostAttribute;
  //
  //
  //
  //
  //
  //
  //           var table = obj.GetType().GetCustomAttribute(typeof(Table)) as Table;
		// 	var sql = "insert into " + table.Name + "(";
		// 	var columns = new List<string>();
		// 	var values = new List<object>();
		// 	foreach (var propertyInfo in obj.GetType().GetProperties())
		// 	{
		// 		var column = propertyInfo.GetCustomAttribute(typeof(Column)) as Column;
		// 		columns.Add(column.Name);
		// 		if (propertyInfo.PropertyType.Name == "String" || propertyInfo.PropertyType.Name == "Boolean")
		// 		{
		// 			values.Add("\"" + propertyInfo.GetValue(obj).ToString() + "\"");
		// 		}
		// 		else if (propertyInfo.PropertyType.Name == "DateTime")
		// 		{
		// 			var dateTime = (DateTime)propertyInfo.GetValue(obj);
		// 			values.Add("\"" + dateTime.ToString("yyyy-MM-dd") + "\"");
		// 		}
		// 		else
		// 		{
		// 			values.Add(propertyInfo.GetValue(obj).ToString());
		// 		}
		// 	}
		// 	sql += string.Join(", ", columns) + ") values(";
		// 	sql += string.Join(", ", values) + ")";
		// 	return sql;
		// }

        #region 로그인

        // 요청 URL
        // https://serverAddress/signin
        [HttpPost("signin")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ResponseSignIn> Post(RequestSignIn request)
        {
            var response = new ResponseSignIn();

            PrimaryDataSource dbContext = dbContextFactory.CreateDbContext();

            if (request == null || string.IsNullOrEmpty(request.authType))
            {
                response.result = "invalid data";
                return response;
            }

            var authType = request.authType.ToLower();

            if (authType.Equals("guest")) // 게스트 로그인의 경우, UUID로 판단
            {
                var guestToken = request.oauthToken;

                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        table.AccountGuestToken.Equals(guestToken)
                    ).AsNoTracking();

                // 전달된 게스트 토큰이 없을 경우
                if (string.IsNullOrEmpty(guestToken))
                    guestToken = Guid.NewGuid().ToString();

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
                                    log.LogError($"signup exception: {e}");
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

            dbContext.Dispose();
            return response;
        }
        #endregion
    }
}