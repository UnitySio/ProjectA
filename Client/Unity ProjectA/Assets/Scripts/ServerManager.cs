using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private Popup popup;

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

    // ���� �߻��� ȣ��
    public void FailureCallback(string errorType, int responseCode, string errorMessage)
    {
        popup.confirm.onClick.RemoveAllListeners();
        popup.title.text = $"����";
        
        if (errorType.ToLower().Contains("http"))
        {
            popup.content.text = $"���� ����: {responseCode}";
            popup.confirm.onClick.AddListener(() => popup.Close());
        }
        else if (errorType.ToLower().Contains("network"))
        {
            popup.content.text = $"��Ʈ��ũ�� Ȯ���� �ּ���.";
            popup.confirm.onClick.AddListener(async () =>
            {
                popup.Close();

                await Task.Delay(1000);
                SceneManager.LoadScene("LoginScene");
            });
        }
        else
        {
            popup.content.text = $"�� �� ���� ����";
            popup.confirm.onClick.AddListener(async () =>
            {
                popup.Close();

                await Task.Delay(500);
                Application.Quit();
            });
        }

        popup.Show();
    }
}