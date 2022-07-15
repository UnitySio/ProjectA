using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    /*

    UserController

    User (사용자) 관련 로직을 처리하는 컨트롤러 클래스.


    API 리스트

    - 유저 데이터 요청
    - 닉네임 생성 및 변경

    */


    [ApiController]
    public class UserController : ControllerBase
    {
        //private static DBContextPoolManager<PrimaryDataSource> dbPoolManager;
        private readonly ILogger<UserController> debugLogger;


        //Construct Dependency Injections
        public UserController(ILogger<UserController> logger)
        {
        }


        //
        //
        // #region 유저 데이터 요청
        //
        // //요청 URL
        // // http://serverAddress/userdata
        // [HttpPost("userdata")]
        // [Consumes(MediaTypeNames.Application.Json)] // application/json
        // public async Task<ResponseUserData> Post(RequestUserData request)
        // {
        //     var response = new ResponseUserData();
        //     //DB에 접속하여 데이터를 조작하는 DBContext객체.
        //     var dbContext = dbPoolManager.Rent();
        //
        //     //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
        //     if (JWTManager.CheckValidationJWT(request.jwtAccess, out SecurityToken tokenInfo))
        //     {
        //         //유효성 검증이 완료된 토큰 정보.
        //         var jwt = tokenInfo as JwtSecurityToken;
        //
        //         var id = jwt.Payload.GetValueOrDefault("AccountUniqueId");
        //
        //         var accountUniqueId = uint.Parse(id.ToString());
        //
        //         // 로그인 중복 체크
        //         if (SessionManager.IsDuplicate(accountUniqueId, request.jwtAccess))
        //         {
        //             response.result = "Duplicate Session";
        //             dbPoolManager.Return(dbContext);
        //             return response;
        //         }
        //
        //         //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
        //         var userQuery =
        //             //TableJoin
        //             from account in dbContext.Set<AccountInfo>()
        //             join user in dbContext.Set<UserInfo>()
        //                 on account.AccountUniqueId equals user.AccountUniqueId
        //             where account.AccountUniqueId.Equals(accountUniqueId)
        //             select new { account, player = user };
        //
        //         if (userQuery.Count() == 1)
        //         {
        //             var user = userQuery.FirstOrDefault();
        //
        //             response.result = "ok";
        //             response.userData = new UserData()
        //             {
        //                 AccountUniqueID = user.account.AccountUniqueId,
        //                 AuthLv = user.account.AccountAuthLv,
        //                 UserLv = (int)user.player.UserLv,
        //                 UserNickname = user.player.UserNickname,
        //                 UserStamina = (int)user.player.UserStamina
        //             };
        //         }
        //         else
        //         {
        //             response.result = "server Error.";
        //         }
        //     }
        //     else
        //     {
        //         response.result = "invalid jwt";
        //     }
        //
        //     dbPoolManager.Return(dbContext);
        //     return response;
        // }
        //
        // #endregion
        //
        // #region 닉네임 생성 및 변경
        //
        // //요청 URL
        // // http://serverAddress/userdata/nickname/update
        // [HttpPost("userdata/nickname/update")]
        // [Consumes(MediaTypeNames.Application.Json)] // application/json
        // public async Task<ResponseUpdateUserNickname> Post(RequestUpdateUserNickname request)
        // {
        //     var response = new ResponseUpdateUserNickname();
        //
        //     //DB에 접속하여 데이터를 조작하는 DBContext객체.
        //     var dbContext = dbPoolManager.Rent();
        //
        //     if (request == null)
        //     {
        //         response.result = "invalid data";
        //         dbPoolManager.Return(dbContext);
        //         return response;
        //     }
        //
        //
        //     //추후, 닉네임 변경권 소유여부나, 정규식 추가하여 판단
        //     if (string.IsNullOrEmpty(request.userNickname))
        //     {
        //         response.result = "invalid user_name data";
        //         dbPoolManager.Return(dbContext);
        //         return response;
        //     }
        //
        //
        //     //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
        //     SecurityToken tokenInfo = new JwtSecurityToken();
        //
        //     if (JWTManager.CheckValidationJWT(request.jwtAccess, out tokenInfo))
        //     {
        //         //유효성 검증이 완료된 토큰 정보.
        //         var jwt = tokenInfo as JwtSecurityToken;
        //
        //         var accountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");
        //
        //         //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
        //         var userQuery = dbContext.UserInfos
        //             .Where(table => table.AccountUniqueId.Equals(accountUniqueId))
        //             .AsNoTracking();
        //
        //         if (userQuery.Count() == 1)
        //         {
        //             var user = userQuery.FirstOrDefault();
        //
        //             //닉네임 변경사항 반영
        //             user.UserNickname = request.userNickname;
        //             dbContext.Entry(user).State = EntityState.Modified;
        //             await dbContext.SaveChangesAsync();
        //
        //             response.result = "ok";
        //         }
        //         else
        //         {
        //             response.result = "server Error.";
        //         }
        //     }
        //     else
        //     {
        //         response.result = "invalid jwt";
        //     }
        //
        //     dbPoolManager.Return(dbContext);
        //     return response;
        // }
        //
        // #endregion
    }
}