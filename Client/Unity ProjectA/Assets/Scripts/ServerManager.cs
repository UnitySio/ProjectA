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
        if (errorType.ToLower().Contains("http"))
        {
            PopupManager.Instance.Show("Error", $"Error Server: {responseCode}");
        }
        else if (errorType.ToLower().Contains("network"))
        {
            PopupManager.Instance.Show("Error", "Please, Check your network.", async () =>
            {
                await Task.Delay(1000);
                SceneManager.LoadScene("SignInScene");
            });
        }
        else
        {
            PopupManager.Instance.Show("Error", "An unknown error has occurred.", async () =>
            {
                await Task.Delay(500);
                Application.Quit();
            });
        }
    }

    public string GetPublicIP()
    {
        var ip = new WebClient().DownloadString("http://ipinfo.io/ip");
        return ip;
    }
}