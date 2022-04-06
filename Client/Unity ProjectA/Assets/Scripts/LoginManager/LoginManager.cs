using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoginState
{
    None,
    VersionCheck,
    AssetCheck,
    AssetDownload,
    JWTConfirm,
    JWTValidateCheck,
    JWTRefresh,
    LoginWaiting,
    LoginRequest,
    LoginPending
}

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager")]
    public LoginState loginState = LoginState.None;

    public string serverAddress = "http://127.0.0.1:5001";
    public string clientVersion = "1.0.0";

    public Popup popup;

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