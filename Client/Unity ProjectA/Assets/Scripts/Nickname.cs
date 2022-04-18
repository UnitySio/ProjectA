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
        ChangeNickname("Guest001");
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

            var result = await APIManager.SendAPIRequestAsync(API.user_gamedata_updateusername, request, ServerManager.Instance.failureCallback);

            var text = result.result;

            if (text.Equals("ok"))
            {
                Debug.Log("닉네임을 변경했습니다.");
            }
        }
    }
}
