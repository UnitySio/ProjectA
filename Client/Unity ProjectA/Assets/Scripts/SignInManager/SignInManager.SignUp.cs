using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public partial class SignInManager : MonoBehaviour
{
    [Header("SignInManager.SignUp")]
    public GameObject signUpGroup;
    public TextMeshProUGUI signUpResult;
    public TMP_InputField signUpEmail;
    public TMP_InputField signUpAuthNumber;
    public TMP_InputField signUpPassword;
    public TMP_InputField signUpCofirmPassword;
    public Button signUpAuthNumberRequest;
    public Button signUp;

    private void OpenUnknownSignUp()
    {
        var token = "";
        var email = "";
        
        signUpResult.text = string.Empty;
        signUpEmail.text = string.Empty;
        signUpAuthNumber.text = string.Empty;
        signUpPassword.text = string.Empty;
        signUpCofirmPassword.text = string.Empty;
        signUpAuthNumberRequest.onClick.RemoveAllListeners();
        signUp.onClick.RemoveAllListeners();
        
        signInGroup.SetActive(false);
        signUpGroup.SetActive(true);

        signUpAuthNumberRequest.onClick.AddListener(async () =>
        {
            email = signUpEmail.text;
            
            signUpEmail.onValueChanged.RemoveAllListeners();
            signUpEmail.onValueChanged.AddListener((args) => signUpResult.text = string.Empty);

            if (string.IsNullOrEmpty(email))
                signUpResult.text = "이메일을 입력해 주세요.";
            else if (!emailPattern.IsMatch(email))
                signUpResult.text = "이메일 형식이 아닙니다.";
            else
            {
                signUpAuthNumberRequest.interactable = false;
                token = await RequestSignUpAuthNumber(email);
                signUpAuthNumberRequest.interactable = true;
            }
        });
        
        signUp.onClick.AddListener(async () =>
        {
            var authNumber = signUpAuthNumber.text;
            var password = HashManager.HashPassword(signUpPassword.text.Trim());
            var passwordCheck = HashManager.HashPassword(signUpCofirmPassword.text.Trim());

            if (token == "")
                signUpResult.text = "이메일 인증을 해주세요.";
            else
            {
                if (string.IsNullOrEmpty(authNumber) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                    signUpResult.text = "모든 항목을 입력해 주세요.";
                else if (!passwordPattern.IsMatch(signUpPassword.text.Trim()))
                    signUpResult.text = "최소 특수문자 1개, 대소문자 1개, 숫자 1개, 8자 이상";
                else if (!password.Equals(passwordCheck))
                    signUpResult.text = "비밀번호가 일치하지 않습니다.";
                else
                {
                    signUp.interactable = false;
                    await RequestSignUp(email, password, authNumber, token);
                    signUp.interactable = true;
                }
            }
        });
    }

    private async Task<string> RequestSignUpAuthNumber(string email)
    {
        var request = new RequestSignUpAuthNumber()
        {
            accountEmail = email
        };

        var response = await APIManager.SendAPIRequestAsync(API.SignUpAuthNumber, request, ServerManager.Instance.FailureCallback);

        if (response != null)
        {
            var result = response as ResponseSignUpAuthNumber;

            var text = result.result;
            
            if (text.Equals("ok"))
            {
                await Task.Delay(333);
                var token = result.signUpToken;
                signUpResult.text = "인증번호는 5분간 유효합니다.";

                signUpEmail.interactable = false;

                return token;

            }
            else if (text.Equals("duplicate email"))
            {
                signUpResult.text = "사용중인 이메일입니다.";
                return "";
            }
            else
            {
                signUpResult.text = "잘못된 데이터입니다.";
                return "";
            }
        }

        return "";
    }
    
    private async Task RequestSignUp(string email, string password, string authNumber, string registerToken)
    {
        var requestAuthNumber = new RequestSignUpAuthNumberVerify()
        {
            signUpToken = registerToken,
            authNumber = authNumber
        };

        var responseAuthNumber = await APIManager.SendAPIRequestAsync(API.SignUpAuthNumberVerify, requestAuthNumber, ServerManager.Instance.FailureCallback) as ResponseSignUpAuthNumberVerify;

        if (responseAuthNumber.result.Equals("ok"))
        {
            var request = new RequestSignUp()
            {
                authType = "account",
                accountEmail = email,
                accountPassword = password,
                signUpToken = registerToken,
                userIP = ServerManager.Instance.GetPublicIP()
            };

            await Task.Delay(333);
            var response = await APIManager.SendAPIRequestAsync(API.SignUp, request, ServerManager.Instance.FailureCallback);
            
            if (response != null)
            {
                ResponseSignUp result = response as ResponseSignUp;

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
                else
                    signUpResult.text = "이미 가입된 계정 정보입니다.";
            }
        }
        else
            signUpResult.text = "인증번호를 확인해 주세요.";
    }
}