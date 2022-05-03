using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;

public class GetUserData : MonoBehaviour
{
    private Popup popup;
    [field: SerializeField] public string UserNickname { get; private set; }
    [field: SerializeField] public int UserLv { get; private set; }
    [field: SerializeField] public int UserStamina { get; private set; }


    private void Awake()
    {
        popup = ServerManager.Instance.Popup;
    }

    private async Task Start()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestUserData()
            {
                jwtAccess = jwtAccess
            };

            var response =
                await APIManager.SendAPIRequestAsync(API.UserData, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseUserData;

                var text = result.result;

                if (text.Equals("ok"))
                {
                    UserNickname = result.userData.UserNickname;
                    UserLv = result.userData.UserLv;
                    UserStamina = result.userData.UserStamina;
                }
                else if (text.ToLower().Equals("duplicate session")) // 중복 로그인 방지
                {
                    popup.confirm.onClick.RemoveAllListeners();
                    popup.title.text = $"알림";
                    popup.content.text = $"다른 기기에서 로그인되었습니다.";
                    popup.confirm.onClick.AddListener(() =>
                    {
                        popup.Close();

                        Application.Quit();
                    });

                    popup.Show();
                }
            }
        }
    }
}