using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
                registerResult.text = "�̸����� �Է��� �ּ���.";
            else if (!emailPattern.IsMatch(email))
                registerResult.text = "�̸��� ������ �ƴմϴ�.";
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
                registerResult.text = "�̸��� ������ ���ּ���.";
            else
            {
                if (string.IsNullOrEmpty(authNumber) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                    registerResult.text = "��� �׸��� �Է��� �ּ���.";
                else if (!passwordPattern.IsMatch(registerPassword.text.Trim()))
                    registerResult.text = "�ּ� Ư������ 1��, ��ҹ��� 1��, ���� 1��, 8�� �̻�";
                else if (!password.Equals(passwordCheck))
                    registerResult.text = "��й�ȣ�� ��ġ���� �ʽ��ϴ�.";
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
        var request = new RequestRegisterAuthNumber()
        {
            accountEmail = email
        };

        var response = await APIManager.SendAPIRequestAsync(API.RegisterAuthNumber, request, ServerManager.Instance.FailureCallback);

        if (response != null)
        {
            ResponseRegisterAuthNumber result = response as ResponseRegisterAuthNumber;

            var text = result.result;
            
            if (text.Equals("ok"))
            {
                await Task.Delay(333);
                var token = result.registerToken;
                registerResult.text = "������ȣ�� 5�а� ��ȿ�մϴ�.";

                registerEmail.interactable = false;

                return token;

            }
            else if (text.Equals("duplicate email"))
            {
                registerResult.text = "������� �̸����Դϴ�.";
                return "";
            }
            else
            {
                registerResult.text = "�߸��� �������Դϴ�.";
                return "";
            }
        }

        return "";
    }
    
    private async Task RequestRegister(string email, string password, string authNumber, string registerToken)
    {
        var requestAuthNumber = new RequestRegisterAuthNumberCheck()
        {
            registerToken = registerToken,
            authNumber = authNumber
        };

        var responseAuthNumber = await APIManager.SendAPIRequestAsync(API.RegisterAuthNumberCheck, requestAuthNumber, ServerManager.Instance.FailureCallback) as ResponseRegisterAuthNumberCheck;

        if (responseAuthNumber.result.Equals("ok"))
        {
            var request = new RequestRegister()
            {
                authType = "account",
                accountEmail = email,
                accountPassword = password,
                registerToken = registerToken
            };

            await Task.Delay(333);
            var response = await APIManager.SendAPIRequestAsync(API.Register, request, ServerManager.Instance.FailureCallback);
            
            if (response != null)
            {
                ResponseRegister result = response as ResponseRegister;

                var text = result.result;

                if (text.Equals("ok"))
                {
                    var jwtAccess = result.jwtAccess;
                    var jwtRefresh = result.jwtRefresh;

                    SecurityPlayerPrefs.SetString("JWTAccess", jwtAccess);
                    SecurityPlayerPrefs.SetString("JWTRefresh", jwtRefresh);
                    SecurityPlayerPrefs.Save();

                    SceneManager.LoadScene("BattleScene");
                }
                else
                    registerResult.text = "�̹� ���Ե� ���� �����Դϴ�.";
            }
        }
        else
            registerResult.text = "������ȣ�� Ȯ���� �ּ���.";
    }
}