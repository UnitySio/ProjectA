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

        // ��ū�� �����Ƿ� �α��� ȭ������ �̵�
        if (string.IsNullOrEmpty(jwtAccess) || string.IsNullOrEmpty(jwtRefresh))
            WaitingSignIn();
        else // ��ū�� �ִٸ� ��ȿ�� üũ�ܰ�� �̵�
            CheckValidateJWT();
    }

    private async void CheckValidateJWT()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        var jwtRefresh = SecurityPlayerPrefs.GetString("JWTRefresh", null);

        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);
        JwtSecurityToken refreshToken = JWTManager.DecryptJWT(jwtRefresh);

        await Task.Delay(333);

        // ���� accessToken�� ��ȿ�ϴٸ�
        if (JWTManager.CheckValidateJWT(accessToken))
        {
            // �α��� ���Žð� ���޿�
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
                else if (text.ToLower().Contains("banned"))
                {
                    var str = text.Split(",");

                    popup.confirm.onClick.RemoveAllListeners();
                    popup.title.text = $"�˸�";
                    popup.content.text = $"�ش� ������ ���� ���� ��������\n{str[1]} ���� ����\n�α����� �����մϴ�.";
                    popup.confirm.onClick.AddListener(async () =>
                    {
                        popup.Close();

                        await Task.Delay(333);

                        SecurityPlayerPrefs.DeleteKey("JWTAccess");
                        SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                        SecurityPlayerPrefs.Save();

                        WaitingSignIn();
                    });

                    popup.Show();
                }
            }
        }
        else if (JWTManager.CheckValidateJWT(refreshToken)) // refreshToken�� ��ȿ�ϰ� accessToken�� ������ �ʿ��ϴٸ�
            RefreshJWT(); // JWT ��ū ����
        else // ��� ��ū�� ����� ���
            WaitingSignIn(); // �α��� ��ư ǥ��
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

        var response = await APIManager.SendAPIRequestAsync(API.SignIn, request, ServerManager.Instance.FailureCallback);

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
            else if (text.ToLower().Contains("banned"))
            {
                var str = text.Split(",");

                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"�˸�";
                popup.content.text = $"�ش� ������ ���� ���� ��������\n{str[1]} ���� ����\n�α����� �����մϴ�.";
                popup.confirm.onClick.AddListener(async () =>
                {
                    popup.Close();

                    await Task.Delay(333);

                    SecurityPlayerPrefs.DeleteKey("JWTAccess");
                    SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                    SecurityPlayerPrefs.Save();

                    WaitingSignIn();
                });

                popup.Show();
            }
            else
            {
                // ���� �߻�
                popup.title.text = $"����";
                popup.content.text = $"����: {text}";
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