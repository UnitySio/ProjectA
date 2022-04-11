using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public partial class LoginManager : MonoBehaviour
{
    private async void CheckVersion()
    {
        loginState = LoginState.VersionCheck;

        var request = new Request_VersionCheck()
        {
            currentClientVersion = clientVersion
        };

        // ���� �߻��� ȣ��
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
        {
            loginState = LoginState.None;

            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"���� ����: {responseCode}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();
                });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"��Ʈ��ũ�� Ȯ���� �ּ���.";
                popup.confirm.onClick.AddListener(async () =>
                {
                    popup.Close();

                    await Task.Delay(1000);
                    CheckVersion();
                });
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

        var response = await APIManager.SendAPIRequestAsync(API.versioncheck, request, failureCallback);
        if (response != null)
        {
            await Task.Delay(333);
            Response_VersionUpdate result = response as Response_VersionUpdate;

            var text = result.result;
            if (text.Equals("ok"))
                if (result.needUpdate) // ������Ʈ�� �ʿ䰡 �ִٸ�
                {
                    // ���� ������Ʈ ���� �Լ� ����
                }
                else // �ֽŹ����̿��� ������Ʈ�� �ʿ䰡 ���ٸ�
                {
                    StartCoroutine(CheckAsset());
                }
            else // ���� �����Ϳ� �̻��� �ִٸ�
            {
                popup.title.text = $"����";
                popup.content.text = $"���� ����: {text}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();

                    Application.Quit();
                });

                popup.Show();
            }
        }
    }
}
