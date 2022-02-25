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
        private static DBContextPoolManager<siogames_mainContext> dbPoolManager;
        private readonly ILogger<AuthController> debugLogger;

        public UserController(ILogger<AuthController> logger)
        {
            this.debugLogger = logger;
            if (dbPoolManager == null)
                dbPoolManager = new DBContextPoolManager<siogames_mainContext>();
        }


        #region 유저 데이터 요청

        //요청 URI
        // http://serverAddress/user/gamedata
        [HttpPost("user/gamedata")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_User_Gamedata> Post(Request_User_Gamedata request)
        {
            Response_User_Gamedata response = new Response_User_Gamedata();
            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
            if (JWTManager.checkValidationJWT(request.jwt_access, out SecurityToken tokenInfo))
            {
                //유효성 검증이 완료된 토큰 정보.
                JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                object id = jwt.Payload.GetValueOrDefault("AccountUniqueId");

                uint AccountUniqueId = uint.Parse(id.ToString());
                
                if (SessionManager.isDuplicate(AccountUniqueId, request.jwt_access))
                {
                    response.result = "Duplicate Session";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var playerData =
                    //TableJoin
                    from account in dbContext.Set<AccountInfo>()
                    join player in dbContext.Set<PlayerInfo>()
                    on account.AccountUniqueId equals player.AccountUniqueId
                    where account.AccountUniqueId.Equals(AccountUniqueId)
                    select new { account, player };

                if (playerData.Count() == 1)
                {
                    var playerInfo = playerData.FirstOrDefault();
                    

                    response.result = "ok";
                    response.userDataInfo = new UserData()
                    {
                        AccountUniqueId = playerInfo.account.AccountUniqueId,
                        AccountEmail = playerInfo.account.AccountEmail,
                        AuthLv = playerInfo.account.AccountAuthLv,
                        UserLv = (int)playerInfo.player.PlayerLv,
                        UserName = playerInfo.player.PlayerNickname
                    };
                }
                else
                {
                    response.result = "server Error.";
                }
            }
            else
            {
                response.result = "invalid jwt";
            }

            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion

        #region 닉네임 생성 및 변경

        //요청 URI
        // http://serverAddress/user/gamedata/update-username
        [HttpPost("user/gamedata/update-username")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<Response_User_Gamedata_UpdateUserName> Post(Request_User_Gamedata_UpdateUserName request)
        {
            Response_User_Gamedata_UpdateUserName response = new Response_User_Gamedata_UpdateUserName();

            //DB에 접속하여 데이터를 조작하는 DBContext객체.
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }


            //추후, 닉네임 변경권 소유여부나, 정규식 추가하여 판단
            if (string.IsNullOrEmpty(request.user_name))
            {
                response.result = "invalid user_name data";
                dbPoolManager.Return(dbContext);
                return response;
            }


            //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
            SecurityToken tokenInfo = new JwtSecurityToken();

            if (JWTManager.checkValidationJWT(request.jwt_access, out tokenInfo))
            {
                //유효성 검증이 완료된 토큰 정보.
                JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                object AccountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");

                //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
                var accountData = dbContext.PlayerInfos
                    .Where(table => table.AccountUniqueId.Equals(AccountUniqueId))
                    .AsNoTracking();

                if (accountData.Count() == 1)
                {
                    var playerInfo = accountData.FirstOrDefault();

                    //닉네임 변경사항 반영
                    playerInfo.PlayerNickname = request.user_name;
                    dbContext.Entry(playerInfo).State = EntityState.Modified;
                    var changedCount = await dbContext.SaveChangesAsync();

                    response.result = "ok";
                }
                else
                {
                    response.result = "server Error.";
                }
            }
            else
            {
                response.result = "invalid jwt";
            }
            
            dbPoolManager.Return(dbContext);
            return response;
        }

        #endregion
    }
}
