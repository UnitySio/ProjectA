using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager.PasswordFind")] [Header("Password Find")]
    public GameObject passwordFindGroup;

    public TextMeshProUGUI passwordFindResult;
    public TMP_InputField passwordFindEmail;
    public TMP_InputField passwordFindAuthNumber;
    public Button passwordFindAuthNumberRequest;
    public Button passwordFind;

    [Header("Password Change")] public GameObject passwordChangeGroup;
    public TextMeshProUGUI passwordChangeResult;
    public TMP_InputField passwordChangePassword;
    public TMP_InputField passwordChangePasswordCheck;
    public Button passwordChange;

    private void OpenUnknownPasswordFind()
    {
        var result = "";

        passwordFindResult.text = string.Empty;
        passwordFindEmail.text = string.Empty;
        passwordFindAuthNumber.text = string.Empty;
        passwordFindAuthNumberRequest.onClick.RemoveAllListeners();
        passwordFind.onClick.RemoveAllListeners();

        loginGroup.SetActive(false);
        passwordFindGroup.SetActive(true);

        passwordFindAuthNumberRequest.onClick.AddListener(async () =>
        {
            var email = passwordFindEmail.text;

            passwordFindEmail.onValueChanged.RemoveAllListeners();
            passwordFindEmail.onValueChanged.AddListener((args) => passwordFindResult.text = string.Empty);

            if (string.IsNullOrEmpty(email))
                passwordFindResult.text = "이메일을 입력해 주세요.";
            else if (!emailPattern.IsMatch(email))
                passwordFindResult.text = "이메일 형식이 아닙니다.";
            else
            {
                passwordFindAuthNumberRequest.interactable = false;
                result = await RequestUnknownPasswordFindAuthNumber(email);
                passwordFindAuthNumberRequest.interactable = true;
            }
        });

        passwordFind.onClick.AddListener(async () =>
        {
            if (result.Equals("ok"))
            {
                var authNumber = passwordFindAuthNumber.text;

                passwordFindAuthNumber.onValueChanged.RemoveAllListeners();
                passwordFindAuthNumber.onValueChanged.AddListener((args) => passwordFindResult.text = string.Empty);

                if (string.IsNullOrEmpty(authNumber))
                    passwordFindResult.text = "인증번호를 입력해 주세요.";
                else
                {
                    passwordFind.interactable = false;
                    await ConfirmUnknownPasswordFindAuthNumber(authNumber);
                    passwordFind.interactable = true;
                }
            }
            else
                passwordFindResult.text = "이메일 인증을 해주세요.";
        });
    }

    private async Task<string> RequestUnknownPasswordFindAuthNumber(string email)
    {
        var request = new Request_Auth_FindPassword_SendRequest()
        {
            account_email = email
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
                popup.confirm.onClick.AddListener(() => { popup.Close(); });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
                popup.confirm.onClick.AddListener(() => { popup.Close(); });
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

        var response =
            await APIManager.SendAPIRequestAsync(API.auth_findpassword_sendrequest, request, failureCallback);

        if (response != null)
        {
            Response_Auth_FindPassword_SendRequest result = response as Response_Auth_FindPassword_SendRequest;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var token = result.findpassword_token;

                SecurityPlayerPrefs.SetString("findpassword_token", token);
                SecurityPlayerPrefs.Save();

                passwordFindResult.text = "인증번호는 5분간 유효합니다.";

                passwordFindEmail.interactable = false;

                return text;
            }
            else
            {
                loginState = LoginState.None;
                passwordFindResult.text = "존재하지 않는 계정입니다.";
                return "";
            }
        }
        else
            return "";
    }

    private async Task ConfirmUnknownPasswordFindAuthNumber(string authNumber)
    {
        var token = SecurityPlayerPrefs.GetString("findpassword_token", null);
        var request = new Request_Auth_FindPassword_SendAuthNumber()
        {
            findpassword_token = token,
            auth_number = authNumber
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
                popup.confirm.onClick.AddListener(() => { popup.Close(); });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
                popup.confirm.onClick.AddListener(() => { popup.Close(); });
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

        var response =
            await APIManager.SendAPIRequestAsync(API.auth_findpassword_sendauthnumber, request, failureCallback);

        if (response != null)
        {
            var result = response as Response_Auth_FindPassword_SendAuthNumber;

            var text = result.result;

            if (text.Equals("ok"))
            {
                passwordFindResult.text = string.Empty;
                passwordFindEmail.text = string.Empty;
                passwordFindAuthNumber.text = string.Empty;
                passwordFindAuthNumberRequest.onClick.RemoveAllListeners();
                passwordFind.onClick.RemoveAllListeners();

                passwordChangeResult.text = string.Empty;
                passwordChangePassword.text = string.Empty;
                passwordChangePasswordCheck.text = string.Empty;
                passwordChange.onClick.RemoveAllListeners();

                passwordFindGroup.SetActive(false);
                passwordChangeGroup.SetActive(true);

                passwordChange.onClick.AddListener(async () =>
                {
                    var password = HashManager.HashPassword(passwordChangePassword.text);
                    var passwordCheck = HashManager.HashPassword(passwordChangePasswordCheck.text);

                    passwordChangePassword.onValueChanged.RemoveAllListeners();
                    passwordChangePassword.onValueChanged.AddListener(
                        (args) => passwordChangeResult.text = string.Empty);

                    if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                        passwordChangeResult.text = "모든 항목을 입력해 주세요.";
                    else if (!password.Equals(passwordCheck))
                        passwordChangeResult.text = "비밀번호가 일치하지 않습니다.";
                    else
                    {
                        passwordChange.interactable = false;
                        await ChangeUnknownPassword(password);
                        passwordChange.interactable = true;
                    }
                });
            }
            else
            {
                loginState = LoginState.None;
                passwordFindResult.text = "인증번호를 확인해 주세요.";
            }
        }
    }

    private async Task ChangeUnknownPassword(string password)
    {
        var token = SecurityPlayerPrefs.GetString("findpassword_token", null);
        var request = new Request_Auth_FindPassword_UpdateAccountPassword()
        {
            findpassword_token = token,
            account_password = password
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
                popup.confirm.onClick.AddListener(() => { popup.Close(); });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
                popup.confirm.onClick.AddListener(() => { popup.Close(); });
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

        var response =
            await APIManager.SendAPIRequestAsync(API.auth_findpassword_updateaccountpassword, request, failureCallback);

        if (response != null)
        {
            var result = response as Response_Auth_FindPassword_UpdateAccountPassword;

            var text = result.result;

            if (text.Equals("ok"))
            {
                passwordChangeResult.text = string.Empty;
                passwordChangePassword.text = string.Empty;
                passwordChangePasswordCheck.text = string.Empty;
                passwordChange.onClick.RemoveAllListeners();

                passwordChangeGroup.SetActive(false);

                WaitingLogin();
            }
            else
            {
                loginState = LoginState.None;
                passwordChangeResult.text = $"서버 에러: {text}";
            }
        }
    }
}
