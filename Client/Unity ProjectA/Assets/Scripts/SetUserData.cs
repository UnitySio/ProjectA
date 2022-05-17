using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;

public class SetUserData : MonoBehaviour
{
    private Popup popup;

    private void Awake()
    {
        popup = ServerManager.Instance.Popup;
    }

    public async Task UpdateUserData()
    {
        
    }

    public async Task UpdateUserNickname(string nickname)
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestUpdateUserNickname()
            {
                jwtAccess = jwtAccess,
                userNickname = nickname
            };

            var response =
                await APIManager.SendAPIRequestAsync(API.UpdateUserNickname, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseUpdateUserNikname;

                var text = result.result;

                if (text.Equals("ok"))
                {
                    
                }
            }
        }
    }
}