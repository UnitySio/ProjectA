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
    public TMP_InputField signUpPasswordCheck;
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
        signUpPasswordCheck.text = string.Empty;
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
                signUpResult.text = "�̸����� �Է��� �ּ���.";
            else if (!emailPattern.IsMatch(email))
                signUpResult.text = "�̸��� ������ �ƴմϴ�.";
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
            var passwordCheck = HashManager.HashPassword(signUpPasswordCheck.text.Trim());

            if (token == "")
                signUpResult.text = "�̸��� ������ ���ּ���.";
            else
            {
                if (string.IsNullOrEmpty(authNumber) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                    signUpResult.text = "��� �׸��� �Է��� �ּ���.";
                else if (!passwordPattern.IsMatch(signUpPassword.text.Trim()))
                    signUpResult.text = "�ּ� Ư������ 1��, ��ҹ��� 1��, ���� 1��, 8�� �̻�";
                else if (!password.Equals(passwordCheck))
                    signUpResult.text = "��й�ȣ�� ��ġ���� �ʽ��ϴ�.";
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
                signUpResult.text = "������ȣ�� 5�а� ��ȿ�մϴ�.";

                signUpEmail.interactable = false;

                return token;

            }
            else if (text.Equals("duplicate email"))
            {
                signUpResult.text = "������� �̸����Դϴ�.";
                return "";
            }
            else
            {
                signUpResult.text = "�߸��� �������Դϴ�.";
                return "";
            }
        }

        return "";
    }
    
    private async Task RequestSignUp(string email, string password, string authNumber, string registerToken)
    {
        var requestAuthNumber = new RequestSignUpAuthNumberCheck()
        {
            signUpToken = registerToken,
            authNumber = authNumber
        };

        var responseAuthNumber = await APIManager.SendAPIRequestAsync(API.SignUpAuthNumberCheck, requestAuthNumber, ServerManager.Instance.FailureCallback) as ResponseSignUpAuthNumberCheck;

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
                    signUpResult.text = "�̹� ���Ե� ���� �����Դϴ�.";
            }
        }
        else
            signUpResult.text = "������ȣ�� Ȯ���� �ּ���.";
    }
}