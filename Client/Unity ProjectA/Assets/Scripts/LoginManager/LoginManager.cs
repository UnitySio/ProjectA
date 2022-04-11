using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    LoginPending,
    RegisterRequest,
    RegisterPending
}

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager")]
    public LoginState loginState = LoginState.None;

    public string serverAddress = "http://127.0.0.1:5001";
    public string clientVersion = "1.0.0";

    public Popup popup;

    private Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");

    private void Awake()
    {
        if (!string.IsNullOrEmpty(serverAddress))
            APIManager.serverAddress = serverAddress;

        InitaiteAddressable();
    }

    private void Start()
    {
        CheckVersion();

        Caching.ClearCache(); // 다운로드 된 에셋 번들 전부 제거
        
        // 토큰 정보 전부 제거
        SecurityPlayerPrefs.DeleteAll();
        SecurityPlayerPrefs.Save();
    }
}