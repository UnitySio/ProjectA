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
    [Header("LoginManager.PasswordFind")]
    [Header("Password Find")]
    public GameObject passwordFindGroup;
    public TextMeshProUGUI passwordFindResult;
    public TMP_InputField passwordFindEmail;
    public TMP_InputField passwordFindAuthNumber;
    public Button passwordFindAuthNumberRequest;
    public Button passwordFind;

    [Header("Password Change")]
    public GameObject passwordChangeGroup;
    public TextMeshProUGUI passwordChangeResult;
    public TMP_InputField passwordChangePassword;
    public TMP_InputField passwordChangePasswordCheck;
    public Button passwordChange;

    private void OpenUnknownPasswordFind()
    {
        var token = "";

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
                token = await RequestPasswordFindAuthNumber(email);
                passwordFindAuthNumberRequest.interactable = true;
            }
        });

        passwordFind.onClick.AddListener(async () =>
        {
            if (token != "")
            {
                var authNumber = passwordFindAuthNumber.text;

                passwordFindAuthNumber.onValueChanged.RemoveAllListeners();
                passwordFindAuthNumber.onValueChanged.AddListener((args) => passwordFindResult.text = string.Empty);

                if (string.IsNullOrEmpty(authNumber))
                    passwordFindResult.text = "인증번호를 입력해 주세요.";
                else
                {
                    passwordFind.interactable = false;
                    await CheckPasswordFindAuthNumber(authNumber, token);
                    passwordFind.interactable = true;
                }
            }
            else
                passwordFindResult.text = "이메일 인증을 해주세요.";
        });
    }

    private async Task<string> RequestPasswordFindAuthNumber(string email)
    {
        var request = new RequestPasswordFindAuthNumber()
        {
            accountEmail = email
        };

<<<<<<< HEAD
        var response =
            await APIManager.SendAPIRequestAsync(API.auth_findpassword_sendrequest, request, failureCallback);

=======
        var response = await APIManager.SendAPIRequestAsync(API.PasswordFindAuthNumber, request, ServerManager.Instance.FailureCallback);
        
>>>>>>> 029fd61... 리팩토링 1차 재작업
        if (response != null)
        {
            ResponsePasswordFindAuthNumber result = response as ResponsePasswordFindAuthNumber;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var token = result.passwordFindToken;

                passwordFindResult.text = "인증번호는 5분간 유효합니다.";

                passwordFindEmail.interactable = false;

                return token;
            }
            else
            {
                passwordFindResult.text = "존재하지 않는 계정입니다.";
                return "";
            }
        }
        else
            return "";
    }

    private async Task CheckPasswordFindAuthNumber(string authNumber, string token)
    {
        var request = new RequestPasswordFindAuthNumberCheck()
        {
            passwordFindToken = token,
            authNumber = authNumber
        };

<<<<<<< HEAD
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

        var response =
            await APIManager.SendAPIRequestAsync(API.auth_findpassword_sendauthnumber, request, failureCallback);

=======
        var response = await APIManager.SendAPIRequestAsync(API.PasswordFindAuthNumberCheck, request, ServerManager.Instance.FailureCallback);
        
        
>>>>>>> 029fd61... 리팩토링 1차 재작업
        if (response != null)
        {
            var result = response as ResponsePasswordFindAuthNumberCheck;

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
                    var password = HashManager.HashPassword(passwordChangePassword.text.Trim());
                    var passwordCheck = HashManager.HashPassword(passwordChangePasswordCheck.text.Trim());

                    passwordChangePassword.onValueChanged.RemoveAllListeners();
                    passwordChangePassword.onValueChanged.AddListener(
                        (args) => passwordChangeResult.text = string.Empty);

                    if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                        passwordChangeResult.text = "모든 항목을 입력해 주세요.";
                    else if (!passwordPattern.IsMatch(passwordChangePassword.text.Trim()))
                        passwordChangeResult.text = "최소 특수문자 1개, 대소문자 1개, 숫자 1개, 8자 이상";
                    else if (!password.Equals(passwordCheck))
                        passwordChangeResult.text = "비밀번호가 일치하지 않습니다.";
                    else
                    {
                        passwordChange.interactable = false;
                        await RequestPasswordChange(password, token);
                        passwordChange.interactable = true;
                    }
                });
            }
            else
                passwordFindResult.text = "인증번호를 확인해 주세요.";
        }
    }

    private async Task RequestPasswordChange(string password, string token)
    {
        var request = new RequestPasswordChange()
        {
            passwordFindToken = token,
            accountPassword = password
        };

<<<<<<< HEAD
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

        var response =
            await APIManager.SendAPIRequestAsync(API.auth_findpassword_updateaccountpassword, request, failureCallback);

=======
        var response = await APIManager.SendAPIRequestAsync(API.PasswordChange, request, ServerManager.Instance.FailureCallback);
        
>>>>>>> 029fd61... 리팩토링 1차 재작업
        if (response != null)
        {
            var result = response as ResponsePasswordChange;

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
                passwordChangeResult.text = $"서버 에러: {text}";
        }
    }
}
