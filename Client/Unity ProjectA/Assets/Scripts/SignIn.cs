using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine.SceneManagement;

public class SignIn : MonoBehaviour
{
    public GameObject signInType;
    public Button guestButton;

    public void WaitingSignIn()
    {
        signInType.SetActive(true);
        
        guestButton.onClick.RemoveAllListeners();
        guestButton.onClick.AddListener(async () =>
        {
            guestButton.interactable = false;
            GuestSignIn();
            await Task.Delay(1000);
            guestButton.interactable = true;
        });
    }
    
    private async Task GuestSignIn()
    {
        string uuid = null;
        
        DeviceIDManager.deviceIDHandler += (string deviceid) =>
        {
            if (!string.IsNullOrEmpty(deviceid))
                uuid = deviceid;
        };
        
        DeviceIDManager.GetDeviceID();
        
        var request = new RequestSignIn()
        {
            authType = "guest",
            oauthToken = uuid,
            userIP = ServerManager.Instance.GetPublicIP()
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
            else if (text.Equals("account banned"))
            {
                SecurityPlayerPrefs.DeleteKey("JWTAccess");
                SecurityPlayerPrefs.DeleteKey("JWTRefresh");
                SecurityPlayerPrefs.Save();
            }
        }
    }
}