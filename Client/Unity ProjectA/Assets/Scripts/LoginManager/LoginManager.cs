using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager")]
    public string serverAddress = "http://127.0.0.1:5001";
    public string clientVersion = "1.0.0";

    public Popup popup;

    private Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");
    private Regex passwordPattern = new Regex(@"^(?=.*?[a-z])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[\W]).{8,}$");

    private void Awake()
    {
        if (!string.IsNullOrEmpty(serverAddress))
            APIManager.serverAddress = serverAddress;

        InitaiteAddressable();
    }

    private void Start()
    {
        CheckVersion();

        Caching.ClearCache();
        
        SecurityPlayerPrefs.DeleteAll();
        SecurityPlayerPrefs.Save();
    }
}