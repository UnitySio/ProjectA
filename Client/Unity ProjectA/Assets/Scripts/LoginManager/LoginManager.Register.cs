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
        // ���� �߻��� ȣ��
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
        {
            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"���� ����: {responseCode}";
                popup.confirm.onClick.AddListener(() => popup.Close());
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"��Ʈ��ũ�� Ȯ���� �ּ���.";
                popup.confirm.onClick.AddListener(() => popup.Close());
            }
            else
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"�� �� ���� ����";
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
            registerResult.text = "������ȣ�� 5�а� ��ȿ�մϴ�.";

            registerEmail.interactable = false;

            return token;

        }
        else if (responseAuthNumberResult.result.Equals("duplicate email"))
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
    
    private async Task RequestRegister(string email, string password, string authNumber, string registerToken)
    {
        // ���� �߻��� ȣ��
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
        {
            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"���� ����: {responseCode}";
                popup.confirm.onClick.AddListener(() => popup.Close());
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"��Ʈ��ũ�� Ȯ���� �ּ���.";
                popup.confirm.onClick.AddListener(() => popup.Close());
            }
            else
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"�� �� ���� ����";
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
                    registerResult.text = "�̹� ���Ե� ���� �����Դϴ�.";
            }
        }
        else
            registerResult.text = "������ȣ�� Ȯ���� �ּ���.";
    }
}