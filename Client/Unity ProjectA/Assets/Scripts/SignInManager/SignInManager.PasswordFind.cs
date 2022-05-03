using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public partial class SignInManager : MonoBehaviour
{
    [Header("SignInManager.FindPassword")]
    [Header("Find Password")]
    public GameObject findPasswordGroup;
    public TextMeshProUGUI findPasswordResult;
    public TMP_InputField findPasswordEmail;
    public TMP_InputField findPasswordAuthNumber;
    public Button findPasswordAuthNumberRequest;
    public Button findPassword;

    [Header("Reset Password")]
    public GameObject resetPasswordGroup;
    public TextMeshProUGUI resetPasswordResult;
    public TMP_InputField resetPasswordNewPassword;
    public TMP_InputField resetPasswordCofirmNewPassword;
    public Button resetPassword;

    private void OpenUnknownFindPassword()
    {
        var token = "";

        findPasswordResult.text = string.Empty;
        findPasswordEmail.text = string.Empty;
        findPasswordAuthNumber.text = string.Empty;
        findPasswordAuthNumberRequest.onClick.RemoveAllListeners();
        findPassword.onClick.RemoveAllListeners();

        signInGroup.SetActive(false);
        findPasswordGroup.SetActive(true);

        findPasswordAuthNumberRequest.onClick.AddListener(async () =>
        {
            var email = findPasswordEmail.text;

            findPasswordEmail.onValueChanged.RemoveAllListeners();
            findPasswordEmail.onValueChanged.AddListener((args) => findPasswordResult.text = string.Empty);

            if (string.IsNullOrEmpty(email))
                findPasswordResult.text = "이메일을 입력해 주세요.";
            else if (!emailPattern.IsMatch(email))
                findPasswordResult.text = "이메일 형식이 아닙니다.";
            else
            {
                findPasswordAuthNumberRequest.interactable = false;
                token = await RequestFindPasswordAuthNumber(email);
                findPasswordAuthNumberRequest.interactable = true;
            }
        });

        findPassword.onClick.AddListener(async () =>
        {
            if (token != "")
            {
                var authNumber = findPasswordAuthNumber.text;

                findPasswordAuthNumber.onValueChanged.RemoveAllListeners();
                findPasswordAuthNumber.onValueChanged.AddListener((args) => findPasswordResult.text = string.Empty);

                if (string.IsNullOrEmpty(authNumber))
                    findPasswordResult.text = "인증번호를 입력해 주세요.";
                else
                {
                    findPassword.interactable = false;
                    await CheckFindPasswordAuthNumber(authNumber, token);
                    findPassword.interactable = true;
                }
            }
            else
                findPasswordResult.text = "이메일 인증을 해주세요.";
        });
    }

    private async Task<string> RequestFindPasswordAuthNumber(string email)
    {
        var request = new RequestFindPasswordAuthaNumber()
        {
            accountEmail = email
        };

        var response = await APIManager.SendAPIRequestAsync(API.FindPasswordAuthNumber, request, ServerManager.Instance.FailureCallback);
        
        if (response != null)
        {
            var result = response as ResponseFindPasswordAuthNumber;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var token = result.findPasswordToken;

                findPasswordResult.text = "인증번호는 5분간 유효합니다.";

                findPasswordEmail.interactable = false;

                return token;
            }
            else
            {
                findPasswordResult.text = "존재하지 않는 계정입니다.";
                return "";
            }
        }
        else
            return "";
    }

    private async Task CheckFindPasswordAuthNumber(string authNumber, string token)
    {
        var request = new RequestFindPasswordAuthaNumberCheck()
        {
            findPasswordToken = token,
            authNumber = authNumber
        };

        var response = await APIManager.SendAPIRequestAsync(API.FindPasswordAuthNumberCheck, request, ServerManager.Instance.FailureCallback);
        
        
        if (response != null)
        {
            var result = response as ResponseFindPasswordAuthNumberCheck;

            var text = result.result;

            if (text.Equals("ok"))
            {
                findPasswordResult.text = string.Empty;
                findPasswordEmail.text = string.Empty;
                findPasswordAuthNumber.text = string.Empty;
                findPasswordAuthNumberRequest.onClick.RemoveAllListeners();
                findPassword.onClick.RemoveAllListeners();

                resetPasswordResult.text = string.Empty;
                resetPasswordNewPassword.text = string.Empty;
                resetPasswordCofirmNewPassword.text = string.Empty;
                resetPassword.onClick.RemoveAllListeners();

                findPasswordGroup.SetActive(false);
                resetPasswordGroup.SetActive(true);

                resetPassword.onClick.AddListener(async () =>
                {
                    var password = HashManager.HashPassword(resetPasswordNewPassword.text.Trim());
                    var passwordCheck = HashManager.HashPassword(resetPasswordCofirmNewPassword.text.Trim());

                    resetPasswordNewPassword.onValueChanged.RemoveAllListeners();
                    resetPasswordNewPassword.onValueChanged.AddListener(
                        (args) => resetPasswordResult.text = string.Empty);

                    if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                        resetPasswordResult.text = "모든 항목을 입력해 주세요.";
                    else if (!passwordPattern.IsMatch(resetPasswordNewPassword.text.Trim()))
                        resetPasswordResult.text = "최소 특수문자 1개, 대소문자 1개, 숫자 1개, 8자 이상";
                    else if (!password.Equals(passwordCheck))
                        resetPasswordResult.text = "비밀번호가 일치하지 않습니다.";
                    else
                    {
                        resetPassword.interactable = false;
                        await RequestResetPassword(password, token);
                        resetPassword.interactable = true;
                    }
                });
            }
            else
                findPasswordResult.text = "인증번호를 확인해 주세요.";
        }
    }

    private async Task RequestResetPassword(string password, string token)
    {
        var request = new RequestResetPassword()
        {
            findPasswordToken = token,
            accountPassword = password
        };

        var response = await APIManager.SendAPIRequestAsync(API.ResetPassword, request, ServerManager.Instance.FailureCallback);
        
        if (response != null)
        {
            var result = response as ResponseResetPassword;

            var text = result.result;

            if (text.Equals("ok"))
            {
                resetPasswordResult.text = string.Empty;
                resetPasswordNewPassword.text = string.Empty;
                resetPasswordCofirmNewPassword.text = string.Empty;
                resetPassword.onClick.RemoveAllListeners();

                resetPasswordGroup.SetActive(false);

                WaitingSignIn();
            }
            else
                resetPasswordResult.text = $"서버 에러: {text}";
        }
    }
}
