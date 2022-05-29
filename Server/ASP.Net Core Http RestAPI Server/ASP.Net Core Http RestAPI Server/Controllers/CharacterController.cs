using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private static DBContextPoolManager<projectaContext> dbPoolManager;
        private readonly ILogger<CharacterController> debugLogger;

        public CharacterController(ILogger<CharacterController> logger)
        {
            debugLogger = logger;
            if (dbPoolManager == null)
                dbPoolManager = new DBContextPoolManager<projectaContext>();
        }
        
        #region 캐릭터 추가
        //요청 URL
        // http://serverAddress/userdata/character/add
        [HttpPost("userdata/character/add")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseAddCharacter> Post(RequestAddCharacter request)
        {
            var response = new ResponseAddCharacter();
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            SecurityToken tokenInfo = new JwtSecurityToken();

            if (JWTManager.CheckValidationJWT(request.jwtAccess, out tokenInfo))
            {
                var jwt = tokenInfo as JwtSecurityToken;
                var id = jwt.Payload.GetValueOrDefault("AccountUniqueId");
                var accountUniqueId = uint.Parse(id.ToString());

                // 로그인 중복 체크
                if (SessionManager.IsDuplicate(accountUniqueId, request.jwtAccess))
                {
                    response.result = "Duplicate Session";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                var accountQuery = dbContext.AccountInfos
                    .Where(table => table.AccountUniqueId.Equals(accountUniqueId))
                    .AsNoTracking();

                if (accountQuery.Count() == 1)
                {
                    var account = accountQuery.FirstOrDefault();

                    var characterQuery = dbContext.UserCharacterInfos
                        .Where(table =>
                            table.AccountUniqueId.Equals(account.AccountUniqueId) &&
                            table.CharacterUniqueId.Equals(uint.Parse(request.characterUniqueID.ToString())))
                        .AsNoTracking();

                    // 캐릭터가 이미 해당 계정에 존재하는 경
                    if (characterQuery.Count() > 0)
                    {
                        response.result = "This character is already exists.";
                        dbPoolManager.Return(dbContext);
                        return response;
                    }

                    var characterData = new UserCharacterInfo()
                    {
                        AccountUniqueId = account.AccountUniqueId,
                        CharacterUniqueId = (uint)request.characterUniqueID,
                        CharacterLv = (uint)request.characterLv
                    };

                    await dbContext.UserCharacterInfos.AddAsync(characterData);
                    dbContext.Entry(characterData).State = EntityState.Added;
                    await dbContext.SaveChangesAsync();
                    response.result = "ok";
                }
            }

            return response;
        }
        #endregion
        
        #region 캐릭터 목록 요청
        //요청 URL
        // http://serverAddress/userdata/character
        [HttpPost("userdata/character")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseGetCharacter> Post(RequestGetCharacter request)
        {
            var response = new ResponseGetCharacter();
            var dbContext = dbPoolManager.Rent();

            if (request == null)
            {
                response.result = "invalid data";
                dbPoolManager.Return(dbContext);
                return response;
            }

            SecurityToken tokenInfo = new JwtSecurityToken();

            if (JWTManager.CheckValidationJWT(request.jwtAccess, out tokenInfo))
            {
                var jwt = tokenInfo as JwtSecurityToken;
                var id = jwt.Payload.GetValueOrDefault("AccountUniqueId");
                var accountUniqueId = uint.Parse(id.ToString());

                // 로그인 중복 체크
                if (SessionManager.IsDuplicate(accountUniqueId, request.jwtAccess))
                {
                    response.result = "Duplicate Session";
                    dbPoolManager.Return(dbContext);
                    return response;
                }

                var accountQuery = dbContext.AccountInfos
                    .Where(table => table.AccountUniqueId.Equals(accountUniqueId))
                    .AsNoTracking();

                if (accountQuery.Count() == 1)
                {
                    var account = accountQuery.FirstOrDefault();

                    var characterQuery = dbContext.UserCharacterInfos
                        .Where(table => table.AccountUniqueId.Equals(account.AccountUniqueId))
                        .AsNoTracking();

                    var characters = characterQuery.ToList();
                    var characterData = new List<CharacterData>();
                    for (int i = 0; i < characters.Count(); i++)
                    {
                        characterData.Add(new CharacterData()
                        {
                            CharacterUniqueID = characters[i].CharacterUniqueId,
                            CharacterGrade = (int)characters[i].CharacterGrade,
                            CharacterLv = (int)characters[i].CharacterLv
                        });
                    }

                    response.result = "ok";
                    response.characterData = characterData;
                }
            }

            return response;
        }
        #endregion
    }
}