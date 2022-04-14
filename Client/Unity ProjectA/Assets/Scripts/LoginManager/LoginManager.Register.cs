using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManger.Register")]
    public GameObject registerGroup;
    public TextMeshProUGUI registerResult;
    public TMP_InputField registerEmail;
    public TMP_InputField registerAuthNumber;
    public TMP_InputField registerPassword;
    public TMP_InputField registerPasswordCheck;
    public Button registerAuthNumberRequest;
    public Button register;

    private void OpenUnknownRegister()
    {
        var token = "";
        var email = "";
        
        registerResult.text = string.Empty;
        registerEmail.text = string.Empty;
        registerAuthNumber.text = string.Empty;
        registerPassword.text = string.Empty;
        registerPasswordCheck.text = string.Empty;
        registerAuthNumberRequest.onClick.RemoveAllListeners();
        register.onClick.RemoveAllListeners();
        
        loginGroup.SetActive(false);
        registerGroup.SetActive(true);

        registerAuthNumberRequest.onClick.AddListener(async () =>
        {
            email = registerEmail.text;
            
            registerEmail.onValueChanged.RemoveAllListeners();
            registerEmail.onValueChanged.AddListener((args) => registerResult.text = string.Empty);

            if (string.IsNullOrEmpty(email))
                registerResult.text = "이메일을 입력해 주세요.";
            else if (!emailPattern.IsMatch(email))
                registerResult.text = "이메일 형식이 아닙니다.";
            else
            {
                registerAuthNumberRequest.interactable = false;
                token = await RequestRegisterAuthNumber(email);
                registerAuthNumberRequest.interactable = true;
            }
        });
        
        register.onClick.AddListener(async () =>
        {
            var authNumber = registerAuthNumber.text;
            var password = HashManager.HashPassword(registerPassword.text.Trim());
            var passwordCheck = HashManager.HashPassword(registerPasswordCheck.text.Trim());

            if (token == "")
                registerResult.text = "이메일 인증을 해주세요.";
            else
            {
                if (string.IsNullOrEmpty(authNumber) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                    registerResult.text = "모든 항목을 입력해 주세요.";
                else if (!passwordPattern.IsMatch(registerPassword.text.Trim()))
                    registerResult.text = "최소 특수문자 1개, 대소문자 1개, 숫자 1개, 8자 이상";
                else if (!password.Equals(passwordCheck))
                    registerResult.text = "비밀번호가 일치하지 않습니다.";
                else
                {
                    register.interactable = false;
                    await RequestRegister(email, password, authNumber, token);
                    register.interactable = true;
                }
            }
        });
    }

    private async Task<string> RequestRegisterAuthNumber(string email)
    {
        // 에러 발생시 호출
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
        {
            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"서버 에러: {responseCode}";
                popup.confirm.onClick.AddListener(() => popup.Close());
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
                popup.confirm.onClick.AddListener(() => popup.Close());
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

        var requestAuthNumber = new Request_Auth_Join_SendRequest()
        {
            account_email = email
        };

        var responseAuthNumber =
            await APIManager.SendAPIRequestAsync(API.auth_join_sendrequest, requestAuthNumber, failureCallback);
        
        Response_Auth_Join_SendRequest responseAuthNumberResult = responseAuthNumber as Response_Auth_Join_SendRequest;

        if (responseAuthNumberResult.result.Equals("ok"))
        {
            await Task.Delay(333);
            var token = responseAuthNumberResult.join_token;
            registerResult.text = "인증번호는 5분간 유효합니다.";

            registerEmail.interactable = false;

            return token;

        }
        else if (responseAuthNumberResult.result.Equals("duplicate email"))
        {
            registerResult.text = "사용중인 이메일입니다.";
            return "";
        }
        else
        {
            registerResult.text = "잘못된 데이터입니다.";
            return "";
        }
    }
    
    private async Task RequestRegister(string email, string password, string authNumber, string registerToken)
    {
        // 에러 발생시 호출
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
        {
            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"서버 에러: {responseCode}";
                popup.confirm.onClick.AddListener(() => popup.Close());
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
                popup.confirm.onClick.AddListener(() => popup.Close());
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

        var requestAuthNumber = new Request_Auth_Join_SendAuthNumber()
        {
            join_token = registerToken,
            auth_number = authNumber
        };

        var responseAuthNumber =
            await APIManager.SendAPIRequestAsync(API.auth_join_sendauthnumber, requestAuthNumber, failureCallback) as
                Response_Auth_Join_SendAuthNumber;

        if (responseAuthNumber.result.Equals("ok"))
        {
            var request = new Request_Auth_Join()
            {
                authType = "account",
                account_email = email,
                account_password = password,
                join_token = registerToken
            };

            await Task.Delay(333);
            var response = await APIManager.SendAPIRequestAsync(API.auth_join, request, failureCallback);

            if (response != null)
            {
                Response_Auth_Join result = response as Response_Auth_Join;

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
                else
                    registerResult.text = "이미 가입된 계정 정보입니다.";
            }
        }
        else
            registerResult.text = "인증번호를 확인해 주세요.";
    }
}