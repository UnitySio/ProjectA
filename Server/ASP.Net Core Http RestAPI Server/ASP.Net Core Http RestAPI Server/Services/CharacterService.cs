using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ASP.Net_Core_Http_RestAPI_Server.Services;

public class CharacterService
{
    private ILogger<CharacterService> log;

    private AccountInfoMapper accountInfoMapper;
    private UserCharacterInfoMapper userCharacterInfoMapper;
    private UserInfoMapper userInfoMapper;
    private UserSigninLogMapper userSigninLogMapper;

    //Construct Dependency Injections
    public CharacterService(ILogger<CharacterService> log, AccountInfoMapper accountInfoMapper,
        UserCharacterInfoMapper userCharacterInfoMapper, UserInfoMapper userInfoMapper,
        UserSigninLogMapper userSigninLogMapper)
    {
        this.log = log;
        this.accountInfoMapper = accountInfoMapper;
        this.userCharacterInfoMapper = userCharacterInfoMapper;
        this.userInfoMapper = userInfoMapper;
        this.userSigninLogMapper = userSigninLogMapper;
    }


    
    
    
    //logic methods

    public async Task<ResponseAddCharacter> AddCharacter(RequestAddCharacter request)
    {
        var response = new ResponseAddCharacter();

        if (request == null)
        {
            response.result = "invalid data";
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
                return response;
            }


            AccountInfo account =
                accountInfoMapper.GetAccountInfoByAccountUniqueID(accountUniqueId.ToString());

            if (account != null)
            {
                UserCharacterInfo userCharacterInfo = userCharacterInfoMapper.GetUserCharacterInfoByAccountUniqueIDAndCharacterUniqueID(
                        accountUniqueId.ToString(), 
                        request.characterUniqueID.ToString()
                        );

                // 캐릭터가 이미 해당 계정에 존재하는 경우 빠꾸처리
                if (userCharacterInfo != null)
                {
                    response.result = "This character is already exists.";
                    return response;
                }


                //새로운 캐릭터 정보 create
                var characterData = new UserCharacterInfo()
                {
                    AccountUniqueId = account.AccountUniqueId,
                    CharacterUniqueId = (uint)request.characterUniqueID,
                    CharacterLv = (uint)request.characterLv
                };
                
                //생성한 캐릭터 정보를 디비에 insert
                int affectRows =
                    await userCharacterInfoMapper.InsertUserCharacterInfo(characterData);
                if (affectRows != 1)
                {
                    throw new Exception(
                        "CharacterService::AddCharacter() db insert failure! userCharacterInfoMapper.InsertUserCharacterInfo() ");
                }

                response.result = "ok";
            }
        }

        return response;
    }


    public async Task<ResponseGetCharacter> GetCharacter(RequestGetCharacter request)
    {
        var response = new ResponseGetCharacter();

        if (request == null)
        {
            response.result = "invalid data";
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
                return response;
            }

            AccountInfo account =
                accountInfoMapper.GetAccountInfoByAccountUniqueID(accountUniqueId.ToString());

            if (account != null)
            {
                var characters =
                    userCharacterInfoMapper.GetUserCharacterInfoListByAccountUniqueID(
                        accountUniqueId.ToString());

                var characterData = new List<CharacterData>();

                foreach (var character in characters)
                {
                    characterData.Add(new CharacterData()
                    {
                        CharacterUniqueID = character.CharacterUniqueId,
                        CharacterGrade = (int)character.CharacterGrade,
                        CharacterLv = (int)character.CharacterLv
                    });
                }

                response.result = "ok";
                response.characterData = characterData;
            }
        }

        return response;
    }
}