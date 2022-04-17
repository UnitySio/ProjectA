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
                if (result.needUpdate) // 업데이트할 필요가 있다면
                {
                    // 추후 업데이트 관련 함수 실행
                }
                else // 최신버전이여서 업데이트할 필요가 없다면
                    StartCoroutine(CheckAsset());
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
