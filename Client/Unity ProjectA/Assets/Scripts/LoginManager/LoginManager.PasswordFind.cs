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
        var request = new Request_Auth_FindPassword_SendRequest()
        {
            account_email = email
        };

        var response = await APIManager.SendAPIRequestAsync(API.auth_findpassword_sendrequest, request, ServerManager.Instance.failureCallback);
        
        if (response != null)
        {
            Response_Auth_FindPassword_SendRequest result = response as Response_Auth_FindPassword_SendRequest;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var token = result.findpassword_token;

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
        var request = new Request_Auth_FindPassword_SendAuthNumber()
        {
            findpassword_token = token,
            auth_number = authNumber
        };

        var response = await APIManager.SendAPIRequestAsync(API.auth_findpassword_sendauthnumber, request, ServerManager.Instance.failureCallback);
        
        
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
        var request = new Request_Auth_FindPassword_UpdateAccountPassword()
        {
            findpassword_token = token,
            account_password = password
        };

        var response = await APIManager.SendAPIRequestAsync(API.auth_findpassword_updateaccountpassword, request, ServerManager.Instance.failureCallback);
        
        
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
                passwordChangeResult.text = $"서버 에러: {text}";
        }
    }
}
