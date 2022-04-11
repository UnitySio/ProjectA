using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine.SceneManagement;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager.Login")]
    public GameObject loginType;
    public Button unknown;

    public GameObject loginGroup;
    public TextMeshProUGUI loginResult;
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button login;
    public Button loginRegister;

    private void WaitingLogin()
    {
        loginState = LoginState.LoginWaiting;

        loginType.SetActive(true);
        unknown.onClick.RemoveAllListeners();
        unknown.onClick.AddListener(async () =>
        {
            unknown.interactable = false;
            OpenUnknownLogin();
            await Task.Delay(1000);
            unknown.interactable = true;
        });
    }

    private void OpenUnknownLogin()
    {
        loginEmail.text = string.Empty;
        loginPassword.text = string.Empty;
        login.onClick.RemoveAllListeners();
        loginRegister.onClick.RemoveAllListeners();

        loginGroup.SetActive(true);
        login.onClick.AddListener(async () =>
        {
            loginResult.text = string.Empty;

            var email = loginEmail.text;
            var password = HashManager.HashPassword(loginPassword.text);

            loginEmail.onValueChanged.RemoveAllListeners();
            loginEmail.onValueChanged.AddListener((args) => loginResult.text = string.Empty);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                loginResult.text = $"이메일 또는 비밀번호가 비어있습니다.";
            else if (!emailPattern.IsMatch(email))
                loginResult.text = "잘못된 이메일 형식입니다.";
            else
            {
                login.interactable = false;
                await TryUnknownLogin(email, password);
                login.interactable = true;
            }
        });
        
        loginRegister.onClick.AddListener(() =>
        {
            loginEmail.text = string.Empty;
            loginPassword.text = string.Empty;
            login.onClick.RemoveAllListeners();
            loginRegister.onClick.RemoveAllListeners();
            
            OpenUnknownRegister();
        });
    }

    private async Task TryUnknownLogin(string email, string passwordHash)
    {
        loginState = LoginState.LoginRequest;

        var request = new Request_Auth_Login()
        {
            authType = "account",
            account_email = email,
            account_password = passwordHash
        };

        // 에러 발생시 호출
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
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
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();
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


        await Task.Delay(333);
        loginState = LoginState.LoginPending;

        var response = await APIManager.SendAPIRequestAsync(API.auth_login, request, failureCallback);

        if (response != null)
        {
            Response_Auth_Login result = response as Response_Auth_Login;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var jwtAccess = result.jwt_access;
                var jwtRefresh = result.jwt_refresh;

                SecurityPlayerPrefs.SetString("jwt_access", jwtAccess);
                SecurityPlayerPrefs.SetString("jwt_refresh", jwtRefresh);
                SecurityPlayerPrefs.Save();

                SceneManager.LoadScene("BattleScene");
            }
            else
            {
                loginState = LoginState.LoginWaiting;

                loginResult.text = "존재하지 않는 계정입니다.";
            }
        }
    }
}
