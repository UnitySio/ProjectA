using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager")]
    public string serverAddress = "http://127.0.0.1:5001";
    public string clientVersion = "1.0.0";

    public Popup popup;

    private Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");
    private Regex passwordPattern = new Regex(@"^(?=.*?[a-z])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[\W]).{8,}$");

    private UnityAction<string, int, string> failureCallback;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(serverAddress))
            APIManager.serverAddress = serverAddress;
        
        failureCallback = new UnityAction<string, int, string>(FailureCallback);
        InitaiteAddressable();
        
        Caching.ClearCache();
    }

    private void Start()
    {
        CheckVersion();
    }
    
    // ���� �߻��� ȣ��
    private void FailureCallback(string errorType, int responseCode, string errorMessage)
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
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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