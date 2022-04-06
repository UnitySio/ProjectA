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

        // ��ū�� �����Ƿ� �α��� ȭ������ �̵�
        if (string.IsNullOrEmpty(jwtAccess) || string.IsNullOrEmpty(jwtRefresh))
            WaitingLogin();
        else // ��ū�� �ִٸ� ��ȿ�� üũ�ܰ�� �̵�
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

        // ���� accessToken�� ��ȿ�ϴٸ�
        if (JWTManager.checkValidateJWT(accessToken))
        {
            // �α��� ���Žð� ���޿�
            var requestCompleteAuthenticate = new Request_Auth_Login()
            {
                authType = "update",
                jwt_refresh = jwtAccess
            };

            var result = await APIManager.SendAPIRequestAsync(API.auth_login, requestCompleteAuthenticate, null);

            // ���� �ð� ��ư ǥ�� ȭ������ �̵�
            SceneManager.LoadScene("BattleScene");
        }
        else if (JWTManager.checkValidateJWT(refreshToken)) // refreshToken�� ��ȿ�ϰ� accessToken�� ������ �ʿ��ϴٸ�
            RefreshJWT(); // JWT ��ū ����
        else // ��� ��ū�� ����� ���
            WaitingLogin(); // �α��� ��ư ǥ��
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

        // ���� �߻��� ȣ��
        UnityAction<string, int, string> failureCallBack = (errorType, responseCode, errorMessage) =>
        {
            loginState = LoginState.None;

            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"���� ����: {responseCode}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();
                });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"��Ʈ��ũ�� Ȯ���� �ּ���.";
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
                popup.title.text = $"����";
                popup.content.text = $"�� �� ���� ����";
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

                // ���� �߻�
                popup.title.text = $"����";
                popup.content.text = $"����: {text}";
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
