using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;

public class Nickname : MonoBehaviour
{
    private void Start()
    {
        CheckNickname();
    }

    private async Task GetUserData()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.checkValidateJWT(accessToken))
        {
            var request = new Request_User_Gamedata()
            {
                jwt_access = jwtAccess
            };

            var response = await APIManager.SendAPIRequestAsync(API.user_gamedata, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as Response_User_Gamedata;

                var text = result.result;

                if (text.Equals("ok"))
                {
                    Debug.Log(result.userDataInfo.UserStamia);
                }
            }
        }
    }

    private async Task ChangeNickname(string nickname)
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.checkValidateJWT(accessToken))
        {
            var request = new Request_User_Gamedata_UpdateUserName()
            {
                jwt_access = jwtAccess,
                user_name = nickname
            };

            var response = await APIManager.SendAPIRequestAsync(API.user_gamedata_updateusername, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as Response_User_Gamedata_UpdateUserName;
                
                var text = result.result;

                if (text.Equals("ok"))
                {
                    Debug.Log("닉네임을 변경했습니다.");
                }
            }
        }
    }
    
    private async Task CheckNickname()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.checkValidateJWT(accessToken))
        {
            var request = new Request_User_Gamedata_CheckUserName()
            {
                jwt_access = jwtAccess
            };

            var response = await APIManager.SendAPIRequestAsync(API.user_gamedata_checkusername, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as Response_User_Gamedata_CheckUserName;
                
                var text = result.result;

                if (text.Equals("empty"))
                    ChangeNickname("GameMaster");
            }
        }
    }
}
