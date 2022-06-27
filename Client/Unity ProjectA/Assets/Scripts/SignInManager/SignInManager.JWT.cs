using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;

public partial class SignInManager : MonoBehaviour
{
    private async void CheckJWT()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        await Task.Delay(333);

        // 토큰이 없으므로 로그인 화면으로 이동
        if (string.IsNullOrEmpty(jwtAccess) || string.IsNullOrEmpty(jwtRefresh))
            WaitingSignIn();
        else // 토큰이 있다면 유효성 체크단계로 이동
            CheckValidateJWT();
    }

    private async void CheckValidateJWT()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);
        JwtSecurityToken refreshToken = JWTManager.DecryptJWT(jwtRefresh);

        await Task.Delay(333);

        // 아직 accessToken이 유효하다면
        if (JWTManager.CheckValidateJWT(accessToken))
        {
            // 로그인 갱신시간 전달용
            var requestCompleteAuthenticate = new RequestSignIn()
            {
                authType = "update",
                jwtRefresh = jwtAccess,
                userIP = ServerManager.Instance.GetPublicIP()
            };

            var response = await APIManager.SendAPIRequestAsync(API.SignIn, requestCompleteAuthenticate,
                ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseSignIn;

                var text = result.result;

                if (text.Equals("ok"))
                    SceneManager.LoadScene("LobbyScene");
                else if (text.Equals("account banned"))
                {
                    SecurityPlayerPrefs.DeleteKey("JWTAccess");
                    SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                    SecurityPlayerPrefs.Save();

                    WaitingSignIn();
                }
            }
        }
        else if (JWTManager.CheckValidateJWT(refreshToken)) // refreshToken이 유효하고 accessToken이 갱신이 필요하다면
            RefreshJWT(); // JWT 토큰 갱신
        else // 모든 토큰이 만료된 경우
            WaitingSignIn(); // 로그인 버튼 표시
    }

    private async void RefreshJWT()
    {
        await Task.Delay(333);

        var refreshToken = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        var request = new RequestSignIn()
        {
            authType = "jwt",
            jwtRefresh = refreshToken,
            userIP = ServerManager.Instance.GetPublicIP()
        };

        var response =
            await APIManager.SendAPIRequestAsync(API.SignIn, request, ServerManager.Instance.FailureCallback);

        if (response != null)
        {
            var result = response as ResponseSignIn;

            var text = result.result;

            if (text.Equals("ok"))
            {
                SecurityPlayerPrefs.SetString("JWTAccess", result.jwtAccess);
                SecurityPlayerPrefs.SetString("JWTRefresh", result.jwtRefresh);
                SecurityPlayerPrefs.Save();

                SceneManager.LoadScene("LobbyScene");
            }
            else if (text.Equals("account banned"))
            {
                SecurityPlayerPrefs.DeleteKey("JWTAccess");
                SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                SecurityPlayerPrefs.Save();

                WaitingSignIn();
            }
            else
            {
                // 에러 발생
                popup.title.text = $"Error";
                popup.content.text = $"Error: {text}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();

                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                });

                popup.Show();
            }
        }
    }
}