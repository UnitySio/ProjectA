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

        // 에러 발생시 호출
        UnityAction<string, int, string> failureCallback = (errorType, responseCode, errorMessage) =>
        {
            loginState = LoginState.None;

            if (errorType.ToLower().Contains("http"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"서버 에러: {responseCode}";
                popup.confirm.onClick.AddListener(() =>
                {
                    popup.Close();
                });
            }
            else if (errorType.ToLower().Contains("network"))
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"에러";
                popup.content.text = $"네트워크를 확인해 주세요.";
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
                popup.title.text = $"에러";
                popup.content.text = $"알 수 없는 에러";
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
                if (result.needUpdate) // 업데이트할 필요가 있다면
                {
                    // 추후 업데이트 관련 함수 실행
                }
                else // 최신버전이여서 업데이트할 필요가 없다면
                {
                    StartCoroutine(CheckAsset());
                }
            else // 버전 데이터에 이상이 있다면
            {
                popup.title.text = $"에러";
                popup.content.text = $"서버 에러: {text}";
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
