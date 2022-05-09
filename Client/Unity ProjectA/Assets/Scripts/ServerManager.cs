using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    private static ServerManager instance;
    public static ServerManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }
    
    public string serverAddress = "http://127.0.0.1:5001";

    [field: SerializeField]
    public Popup Popup { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
            Destroy(instance);
        
        if (!string.IsNullOrEmpty(serverAddress))
            APIManager.serverAddress = serverAddress;
    }

    // 에러 발생시 호출
    public void FailureCallback(string errorType, int responseCode, string errorMessage)
    {
        Popup.confirm.onClick.RemoveAllListeners();
        Popup.title.text = $"Error";
        
        if (errorType.ToLower().Contains("http"))
        {
            Popup.content.text = $"Error Server: {responseCode}";
            Popup.confirm.onClick.AddListener(() => Popup.Close());
        }
        else if (errorType.ToLower().Contains("network"))
        {
            Popup.content.text = $"";
            Popup.confirm.onClick.AddListener(async () =>
            {
                Popup.Close();

                await Task.Delay(1000);
                SceneManager.LoadScene("LoginScene");
            });
        }
        else
        {
            Popup.content.text = $"An unknown error has occurred.";
            Popup.confirm.onClick.AddListener(async () =>
            {
                Popup.Close();

                await Task.Delay(500);
                Application.Quit();
            });
        }

        Popup.Show();
    }

    public string GetPublicIP()
    {
        var ip = new WebClient().DownloadString("http://ipinfo.io/ip");
        return ip;
    }
}