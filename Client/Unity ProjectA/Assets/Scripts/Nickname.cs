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

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestUserData()
            {
                jwtAccess = jwtAccess
            };

            var response = await APIManager.SendAPIRequestAsync(API.UserData, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseUserData;

                var text = result.result;

                if (text.Equals("ok"))
                {
                    Debug.Log(result.userData.UserStamia);
                }
            }
        }
    }

    private async Task ChangeNickname(string nickname)
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestUserNicknameUpdate()
            {
                jwtAccess = jwtAccess,
                userNickname = nickname
            };

            var response = await APIManager.SendAPIRequestAsync(API.UserNicknameUpdate, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseUserNiknameUpdate;
                
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

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestUserNicknameCheck()
            {
                jwtAccess = jwtAccess
            };

            var response = await APIManager.SendAPIRequestAsync(API.UserNicknameCheck, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseUserNicknameCheck;
                
                var text = result.result;

                if (text.Equals("empty"))
                    ChangeNickname("GameMaster");
            }
        }
    }
}
