using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine.SceneManagement;

public partial class SignInManager : MonoBehaviour
{
    [Header("SignInManager.SignIn")]
    public GameObject signInTypeGroup;
    public Button unknownButton;
    public Button guestButton;
    
    public GameObject signInGroup;
    public TextMeshProUGUI signInResultText;
    public TMP_InputField signInEmailInputField;
    public TMP_InputField signInAuthNumberInputField;
    public Button signInSendAuthNumberButton;
    public Button signInButton;

    private void WaitingSignIn()
    {
        signInTypeGroup.SetActive(true);
        unknownButton.onClick.RemoveAllListeners();
        unknownButton.onClick.AddListener(async () =>
        {
            unknownButton.interactable = false;
            OpenUnknownSignIn();
            await Task.Delay(1000);
            unknownButton.interactable = true;
        });
        
        guestButton.onClick.RemoveAllListeners();
        guestButton.onClick.AddListener(async () =>
        {
            guestButton.interactable = false;
            GuestSignIn();
            await Task.Delay(1000);
            guestButton.interactable = true;
        });
    }

    private void OpenUnknownSignIn()
    {
        signInResultText.text = String.Empty;
        signInEmailInputField.text = string.Empty;
        signInAuthNumberInputField.text = string.Empty;
        signInSendAuthNumberButton.onClick.RemoveAllListeners();
        signInButton.onClick.RemoveAllListeners();

        signInTypeGroup.SetActive(false);
        signInGroup.SetActive(true);

        var email = "";
        var token = "";

        signInSendAuthNumberButton.onClick.AddListener(async () =>
        {
            email = signInEmailInputField.text;

            signInEmailInputField.onValueChanged.RemoveAllListeners();
            signInEmailInputField.onValueChanged.AddListener((args) => signInResultText.text = string.Empty);

            if (string.IsNullOrEmpty(email))
                signInResultText.text = "Please enter the email.";
            else if (!emailPattern.IsMatch(email))
                signInResultText.text = "Incorrect email.";
            else
            {
                signInSendAuthNumberButton.interactable = false;
                token = await SendSignInAuthNumber(email);
                signInSendAuthNumberButton.interactable = true;
            }
        });

        signInButton.onClick.AddListener(async () =>
        {
            var authNumber = signInAuthNumberInputField.text;

            if (string.IsNullOrEmpty(token))
                signInResultText.text = "Please verify your email.";
            else
            {
                if (string.IsNullOrEmpty(authNumber))
                    signInResultText.text = "Please enter the auth number.";
                else
                {
                    signInButton.interactable = false;
                    await SignIn(token, authNumber, email);
                    signInButton.interactable = true;
                }
            }
        });
    }

    private async Task<string> SendSignInAuthNumber(string email)
    {
        var request = new RequestSendSignInAuthNumber()
        {
            accountEmail = email
        };

        var response = await APIManager.SendAPIRequestAsync(API.SendSignInAuthNumber, request,
            ServerManager.Instance.FailureCallback);

        if (response != null)
        {
            var result = response as ResponseSendSignInAuthNumber;

            var text = result.result;

            if (text.Equals("ok"))
            {
                await Task.Delay(333);
                var token = result.signInToken;
                signInResultText.text = "Auth number has been sent.";
                signInEmailInputField.interactable = false;
                return token;
            }
            else
            {
                signInResultText.text = "Failed to send auth number.";
                return "";
            }
        }

        return "";
    }

    private async Task SignIn(string token, string authNumber, string accountEmail)
    {
        // 인증번호 검증
        var request = new RequestVerifySignInAuthNumber()
        {
            signInToken = token,
            authNumber = authNumber
        };
        
        var response = await APIManager.SendAPIRequestAsync(API.VerifySignInAuthNumber, request,
            ServerManager.Instance.FailureCallback);

        if (response != null)
        {
            var result = response as ResponseVerifySignInAuthNumber;
            
            var text = result.result;

            if (text.Equals("ok"))
            {
                // 로그인
                var request2 = new RequestSignIn()
                {
                    authType = "account",
                    accountEmail = accountEmail,
                    signInToken = token,
                    userIP = ServerManager.Instance.GetPublicIP()
                };
                
                await Task.Delay(333);
                var response2 = await APIManager.SendAPIRequestAsync(API.SignIn, request2,
                    ServerManager.Instance.FailureCallback);

                if (response2 != null)
                {
                    var result2 = response2 as ResponseSignIn;
                    
                    var text2 = result2.result;

                    if (text2.Equals("ok"))
                    {
                        var jwtAccess = result2.jwtAccess;
                        var jwtRefresh = result2.jwtRefresh;
                        
                        SecurityPlayerPrefs.SetString("JWTAccess", jwtAccess);
                        SecurityPlayerPrefs.SetString("JWTRefresh", jwtRefresh);
                        SecurityPlayerPrefs.Save();
                        
                        SceneManager.LoadScene("LobbyScene");
                    }
                    else if (text.Equals("account banned"))
                    {
                        SecurityPlayerPrefs.DeleteKey("JWTAccess");
                        SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                        SecurityPlayerPrefs.Save();
                    }
                }
            }
        }
    }
    
    private async Task GuestSignIn()
    {
        var request = new RequestSignIn()
        {
            authType = "guest",
            oauthToken = null
        };
        
        var response = await APIManager.SendAPIRequestAsync(API.SignIn, request,
            ServerManager.Instance.FailureCallback);

        if (response != null)
        {
            var result = response as ResponseSignIn;
            
            var text = result.result;
            
            if (text.Equals("ok"))
            {
                var jwtAccess = result.jwtAccess;
                var jwtRefresh = result.jwtRefresh;
                
                SecurityPlayerPrefs.SetString("JWTAccess", jwtAccess);
                SecurityPlayerPrefs.SetString("JWTRefresh", jwtRefresh);
                SecurityPlayerPrefs.Save();
                
                SceneManager.LoadScene("LobbyScene");
            }
        }
    }
}