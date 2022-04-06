using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public partial class LoginManager : MonoBehaviour
{
    private async void ConfirmJWT()
    {
        loginState = LoginState.JWTConfirm;

        var jwtAccess = SecurityPlayerPrefs.GetString("jwt_access", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("jwt_refresh", null);

        await Task.Delay(333);

        // 토큰이 없으므로 로그인 화면으로 이동
        if (string.IsNullOrEmpty(jwtAccess) || string.IsNullOrEmpty(jwtRefresh))
            WaitingLogin();
        else // 토큰이 있다면 유효성 체크단계로 이동
            CheckValidateJWT();
    }

    private async void CheckValidateJWT()
    {
        loginState = LoginState.JWTValidateCheck;

        var jwtAccess = SecurityPlayerPrefs.GetString("jwt_access", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("jwt_refresh", null);

        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);
        JwtSecurityToken refreshToken = JWTManager.DecryptJWT(jwtRefresh);

        await Task.Delay(333);

        // 아직 accessToken이 유효하다면
        if (JWTManager.checkValidateJWT(accessToken))
        {
            // 로그인 갱신시간 전달용
            var requestCompleteAuthenticate = new Request_Auth_Login()
            {
                authType = "update",
                jwt_refresh = jwtAccess
            };

            var result = await APIManager.SendAPIRequestAsync(API.auth_login, requestCompleteAuthenticate, null);

            // 게임 시간 버튼 표시 화면으로 이동
            SceneManager.LoadScene("BattleScene");
        }
        else if (JWTManager.checkValidateJWT(refreshToken)) // refreshToken이 유효하고 accessToken이 갱신이 필요하다면
            RefreshJWT(); // JWT 토큰 갱신
        else // 모든 토큰이 만료된 경우
            WaitingLogin(); // 로그인 버튼 표시
    }

    private async void RefreshJWT()
    {
        loginState = LoginState.JWTRefresh;

        await Task.Delay(333);

        var refreshToken = SecurityPlayerPrefs.GetString("jwt_refresh", null);

        var request = new Request_Auth_Login()
        {
            authType = "jwt",
            jwt_refresh = refreshToken
        };

        // 에러 발생시 호출
        UnityAction<string, int, string> failureCallBack = (errorType, responseCode, errorMessage) =>
        {
            loginState = LoginState.None;

            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"서버 에러: {responseCode}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();
                });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
                popup.confirm.onClick.AddListener(async () =>
                {
                    popup.Close();

                    await Task.Delay(1000);
                    RefreshJWT();
                });
            }
            else
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"알 수 없는 에러";
                popup.confirm.onClick.AddListener(async () =>
                {
                    popup.Close();

                    await Task.Delay(500);
                    Application.Quit();
                });
            }

            popup.Show();
        };

        var response = await APIManager.SendAPIRequestAsync(API.auth_login, request, failureCallBack);
        if (response != null)
        {
            Response_Auth_Login result = response as Response_Auth_Login;

            var text = result.result;

            if (text.Equals("ok"))
            {
                SecurityPlayerPrefs.SetString("jwt_access", result.jwt_access);
                SecurityPlayerPrefs.SetString("jwt_refresh", result.jwt_refresh);
                SecurityPlayerPrefs.Save();
            }
            else
            {
                loginState = LoginState.None;

                // 에러 발생
                popup.title.text = $"에러";
                popup.content.text = $"에러: {text}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();

                    SceneManager.LoadScene("LoginScene");
                });

                popup.Show();
            }
        }
    }
}
