using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine.SceneManagement;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager.Login")]
    public GameObject loginTypeGroup;
    public Button unknown;

    public GameObject loginGroup;
    public TextMeshProUGUI loginResult;
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button login;
    public Button loginRegister;
    public Button loginPasswordFind;

    private void WaitingLogin()
    {
        loginTypeGroup.SetActive(true);
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
        loginPasswordFind.onClick.RemoveAllListeners();

        loginTypeGroup.SetActive(false);
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
                await RequestLogin(email, password);
                login.interactable = true;
            }
        });
        
        loginRegister.onClick.AddListener(() =>
        {
            loginEmail.text = string.Empty;
            loginPassword.text = string.Empty;
            login.onClick.RemoveAllListeners();
            loginRegister.onClick.RemoveAllListeners();
            loginPasswordFind.onClick.RemoveAllListeners();
            
            OpenUnknownRegister();
        });
        
        loginPasswordFind.onClick.AddListener(() =>
        {
            loginEmail.text = string.Empty;
            loginPassword.text = string.Empty;
            login.onClick.RemoveAllListeners();
            loginRegister.onClick.RemoveAllListeners();
            loginPasswordFind.onClick.RemoveAllListeners();
            
            OpenUnknownPasswordFind();
        });
    }

    private async Task RequestLogin(string email, string passwordHash)
    {
        var request = new Request_Auth_Login()
        {
            authType = "account",
            account_email = email,
            account_password = passwordHash
        };

        await Task.Delay(333);
        
        var response = await APIManager.SendAPIRequestAsync(API.auth_login, request, failureCallback);

        if (response != null)
        {
            Response_Auth_Login result = response as Response_Auth_Login;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var jwtAccess = result.jwt_access;
                var jwtRefresh = result.jwt_refresh;

                SecurityPlayerPrefs.SetString("JWTAccess", jwtAccess);
                SecurityPlayerPrefs.SetString("JWTRefresh", jwtRefresh);
                SecurityPlayerPrefs.Save();

                SceneManager.LoadScene("BattleScene");
            }
            else if (text.ToLower().Contains("banned"))
            {
                var str = text.Split(",");
                
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"알림";
                popup.content.text = $"해당 계정은 게임 규정 위반으로\n{str[1]} 이후 부터\n로그인이 가능합니다.";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();
                });
                
                popup.Show();
            }
            else
                loginResult.text = "존재하지 않는 계정입니다.";
        }
    }
}
