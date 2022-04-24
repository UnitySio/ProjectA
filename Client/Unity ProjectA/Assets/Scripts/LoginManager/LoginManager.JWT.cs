using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;

public partial class LoginManager : MonoBehaviour
{
    private async void CheckJWT()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        await Task.Delay(333);

        // ≈‰≈´¿Ã æ¯¿∏π«∑Œ ∑Œ±◊¿Œ »≠∏È¿∏∑Œ ¿Ãµø
        if (string.IsNullOrEmpty(jwtAccess) || string.IsNullOrEmpty(jwtRefresh))
            WaitingLogin();
        else // ≈‰≈´¿Ã ¿÷¥Ÿ∏È ¿Ø»øº∫ √º≈©¥‹∞Ë∑Œ ¿Ãµø
            CheckValidateJWT();
    }

    private async void CheckValidateJWT()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);
        JwtSecurityToken refreshToken = JWTManager.DecryptJWT(jwtRefresh);

        await Task.Delay(333);

        // æ∆¡˜ accessToken¿Ã ¿Ø»ø«œ¥Ÿ∏È
        if (JWTManager.CheckValidateJWT(accessToken))
        {
            // ∑Œ±◊¿Œ ∞ªΩ≈Ω√∞£ ¿¸¥ﬁøÎ
            var requestCompleteAuthenticate = new RequestLogin()
            {
                authType = "update",
                jwtRefresh = jwtAccess
            };
<<<<<<< HEAD

            var result = await APIManager.SendAPIRequestAsync(API.auth_login, requestCompleteAuthenticate, failureCallback);
=======
            
            var response = await APIManager.SendAPIRequestAsync(API.Login, requestCompleteAuthenticate, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                ResponseLogin result = response as ResponseLogin;
>>>>>>> 029fd61... Î¶¨Ìå©ÌÜ†ÎßÅ 1Ï∞® Ïû¨ÏûëÏóÖ

            var text = result.result;

            if (text.ToLower().Contains("banned"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"∞Ë¡§";
                popup.content.text = $"{text}";
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
            else
                SceneManager.LoadScene("BattleScene");
        }
        else if (JWTManager.CheckValidateJWT(refreshToken)) // refreshToken¿Ã ¿Ø»ø«œ∞Ì accessToken¿Ã ∞ªΩ≈¿Ã « ø‰«œ¥Ÿ∏È
            RefreshJWT(); // JWT ≈‰≈´ ∞ªΩ≈
        else // ∏µÁ ≈‰≈´¿Ã ∏∏∑·µ» ∞ÊøÏ
            WaitingLogin(); // ∑Œ±◊¿Œ πˆ∆∞ «•Ω√
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

<<<<<<< HEAD
        var response = await APIManager.SendAPIRequestAsync(API.auth_login, request, failureCallback);
=======
        var response = await APIManager.SendAPIRequestAsync(API.Login, request, ServerManager.Instance.FailureCallback);
        
>>>>>>> 029fd61... Î¶¨Ìå©ÌÜ†ÎßÅ 1Ï∞® Ïû¨ÏûëÏóÖ
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
                // ø°∑Ø πﬂª˝
                popup.title.text = $"ø°∑Ø";
                popup.content.text = $"ø°∑Ø: {text}";
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
