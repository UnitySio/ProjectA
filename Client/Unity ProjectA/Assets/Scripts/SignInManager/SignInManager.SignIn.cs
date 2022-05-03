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
    [Header("SignInManager.SignIn")] public GameObject signInTypeGroup;
    public Button unknown;

    public GameObject signInGroup;
    public TextMeshProUGUI signInResult;
    public TMP_InputField signInEmail;
    public TMP_InputField signInPassword;
    public Button signIn;
    public Button signInSignUp;
    public Button signInFindPassword;

    private void WaitingSignIn()
    {
        signInTypeGroup.SetActive(true);
        unknown.onClick.RemoveAllListeners();
        unknown.onClick.AddListener(async () =>
        {
            unknown.interactable = false;
            OpenUnknownSignIn();
            await Task.Delay(1000);
            unknown.interactable = true;
        });
    }

    private void OpenUnknownSignIn()
    {
        signInEmail.text = string.Empty;
        signInPassword.text = string.Empty;
        signIn.onClick.RemoveAllListeners();
        signInSignUp.onClick.RemoveAllListeners();
        signInFindPassword.onClick.RemoveAllListeners();

        signInTypeGroup.SetActive(false);
        signInGroup.SetActive(true);
        signIn.onClick.AddListener(async () =>
        {
            signInResult.text = string.Empty;

            var email = signInEmail.text;
            var password = HashManager.HashPassword(signInPassword.text);

            signInEmail.onValueChanged.RemoveAllListeners();
            signInEmail.onValueChanged.AddListener((args) => signInResult.text = string.Empty);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                signInResult.text = $"�̸��� �Ǵ� ��й�ȣ�� ����ֽ��ϴ�.";
            else if (!emailPattern.IsMatch(email))
                signInResult.text = "�߸��� �̸��� �����Դϴ�.";
            else if (!passwordPattern.IsMatch(signInPassword.text.Trim()))
                signUpResult.text = "�ּ� Ư������ 1��, ��ҹ��� 1��, ���� 1��, 8�� �̻�";
            else
            {
                signIn.interactable = false;
                await RequestSignIn(email, password);
                signIn.interactable = true;
            }
        });

        signInSignUp.onClick.AddListener(() =>
        {
            signInEmail.text = string.Empty;
            signInPassword.text = string.Empty;
            signIn.onClick.RemoveAllListeners();
            signInSignUp.onClick.RemoveAllListeners();
            signInFindPassword.onClick.RemoveAllListeners();

            OpenUnknownSignUp();
        });

        signInFindPassword.onClick.AddListener(() =>
        {
            signInEmail.text = string.Empty;
            signInPassword.text = string.Empty;
            signIn.onClick.RemoveAllListeners();
            signInSignUp.onClick.RemoveAllListeners();
            signInFindPassword.onClick.RemoveAllListeners();

            OpenUnknownFindPassword();
        });
    }

    private async Task RequestSignIn(string email, string passwordHash)
    {
        var request = new RequestSignIn()
        {
            authType = "account",
            accountEmail = email,
            accountPassword = passwordHash,
            userIP = ServerManager.Instance.GetPublicIP()
        };

        await Task.Delay(333);

        var response =
            await APIManager.SendAPIRequestAsync(API.SignIn, request, ServerManager.Instance.FailureCallback);

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
            else if (text.ToLower().Contains("banned"))
            {
                var str = text.Split(",");

                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"�˸�";
                popup.content.text = $"�ش� ������ ���� ���� ��������\n{str[1]} ���� ����\n�α����� �����մϴ�.";
                popup.confirm.onClick.AddListener(() => { popup.Close(); });

                popup.Show();
            }
            else
                signInResult.text = "�������� �ʴ� �����Դϴ�.";
        }
    }
}