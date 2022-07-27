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

public class UserService
{
    private ILogger<UserService> log;

    private AccountInfoMapper accountInfoMapper;
    private UserCharacterInfoMapper userCharacterInfoMapper;
    private UserInfoMapper userInfoMapper;
    private UserSigninLogMapper userSigninLogMapper;


    //Construct Dependency Injections
    public UserService(ILogger<UserService> log, AccountInfoMapper accountInfoMapper,
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

    public async Task<ResponseUserData> GetUserData(RequestUserData request)
    {
        var response = new ResponseUserData();

        //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
        if (JWTManager.CheckValidationJWT(request.jwtAccess, out SecurityToken tokenInfo))
        {
            //유효성 검증이 완료된 토큰 정보.
            var jwt = tokenInfo as JwtSecurityToken;

            var id = jwt.Payload.GetValueOrDefault("AccountUniqueId");

            var accountUniqueId = uint.Parse(id.ToString());

            // 로그인 중복 체크
            if (SessionManager.IsDuplicate(accountUniqueId, request.jwtAccess))
            {
                response.result = "Duplicate Session";
                return response;
            }

            //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.


            AccountInfo accountInfo = accountInfoMapper.GetAccountInfoByAccountUniqueID(accountUniqueId.ToString());
            UserInfo userInfo = userInfoMapper.GetUserInfoByAccountUniqueID(accountUniqueId.ToString());
            
            if (accountInfo != null && userInfo != null)
            {
                response.result = "ok";
                response.userData = new UserData()
                {
                    AccountUniqueID = accountInfo.AccountUniqueId,
                    AuthLv = accountInfo.AccountAuthLv,
                    UserLv = (int)userInfo.UserLv,
                    UserNickname = userInfo.UserNickname,
                    UserStamina = (int)userInfo.UserStamina
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
        
        return response;
    }


    public async Task<ResponseUpdateUserNickname> UpdateNickName(RequestUpdateUserNickname request)
    {
        var response = new ResponseUpdateUserNickname();

        if (request == null)
        {
            response.result = "invalid data";
            return response;
        }


        //추후, 닉네임 변경권 소유여부나, 정규식 추가하여 판단
        if (string.IsNullOrEmpty(request.userNickname))
        {
            response.result = "invalid user_name data";
            return response;
        }


        //jwt refresh 토큰 유효성 검사 및, jwt_access 토큰 재발급 준비.
        SecurityToken tokenInfo = new JwtSecurityToken();

        if (JWTManager.CheckValidationJWT(request.jwtAccess, out tokenInfo))
        {
            //유효성 검증이 완료된 토큰 정보.
            var jwt = tokenInfo as JwtSecurityToken;

            var accountUniqueId = jwt.Payload.GetValueOrDefault("AccountUniqueId");

            //전달된 정보로 db 조회후, 해당 정보를 db에서 가져온다.
            var userInfo = userInfoMapper.GetUserInfoByAccountUniqueID(accountUniqueId.ToString());

            if (userInfo != null)
            {
                //닉네임 변경사항 반영
                userInfo.UserNickname = request.userNickname;

                int affectRow = await userInfoMapper.UpdateUserInfo(userInfo);
                if (affectRow != 1)
                {
                    throw new Exception("UserService::UpdateNickName() db update failure! userInfoMapper.UpdateUserInfo() ");
                }
                
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

        return response;
    }
}