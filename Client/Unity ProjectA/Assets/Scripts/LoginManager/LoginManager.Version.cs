using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class LoginManager : MonoBehaviour
{
    private async void CheckVersion()
    {
        var request = new Request_VersionCheck()
        {
            currentClientVersion = clientVersion
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
                    StartCoroutine(CheckAsset());
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
