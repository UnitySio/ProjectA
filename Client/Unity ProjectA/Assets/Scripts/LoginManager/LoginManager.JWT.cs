using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;

public partial class LoginManager : MonoBehaviour
{
    private async void CheckJWT()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        await Task.Delay(333);

        // 토큰이 없으므로 로그인 화면으로 이동
        if (string.IsNullOrEmpty(jwtAccess) || string.IsNullOrEmpty(jwtRefresh))
            WaitingLogin();
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
            var requestCompleteAuthenticate = new RequestLogin()
            {
                authType = "update",
                jwtRefresh = jwtAccess
            };
            
            var response = await APIManager.SendAPIRequestAsync(API.Login, requestCompleteAuthenticate, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                ResponseLogin result = response as ResponseLogin;

                var text = result.result;

                if (text.Equals("ok"))
                    SceneManager.LoadScene("LobbyScene");
                else if (text.ToLower().Contains("banned"))
                {
                    var str = text.Split(",");
                
                    popup.confirm.onClick.RemoveAllListeners();
                    popup.title.text = $"알림";
                    popup.content.text = $"해당 계정은 게임 규정 위반으로\n{str[1]} 이후 부터\n로그인이 가능합니다.";
                    popup.confirm.onClick.AddListener(async () =>
                    {
                        popup.Close();
                        
                        await Task.Delay(333);
                        
                        SecurityPlayerPrefs.DeleteKey("JWTAccess");
                        SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                        SecurityPlayerPrefs.Save();

                        WaitingLogin();
                    });
                
                    popup.Show();
                }

            }
        }
        else if (JWTManager.CheckValidateJWT(refreshToken)) // refreshToken이 유효하고 accessToken이 갱신이 필요하다면
            RefreshJWT(); // JWT 토큰 갱신
        else // 모든 토큰이 만료된 경우
            WaitingLogin(); // 로그인 버튼 표시
    }

    private async void RefreshJWT()
    {
        await Task.Delay(333);

        var refreshToken = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        var request = new RequestLogin()
        {
            authType = "jwt",
            jwtRefresh = refreshToken
        };

        var response = await APIManager.SendAPIRequestAsync(API.Login, request, ServerManager.Instance.FailureCallback);
        
        if (response != null)
        {
            ResponseLogin result = response as ResponseLogin;

            var text = result.result;

            if (text.Equals("ok"))
            {
                SecurityPlayerPrefs.SetString("JWTAccess", result.jwtAccess);
                SecurityPlayerPrefs.SetString("JWTRefresh", result.jwtRefresh);
                SecurityPlayerPrefs.Save();
            }
            else
            {
                // 에러 발생
                popup.title.text = $"에러";
                popup.content.text = $"에러: {text}";
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
