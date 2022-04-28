using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public partial class SignInManager : MonoBehaviour
{
    [Header("SignInManager")]
    [SerializeField]
    private Popup popup;
    
    private Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");
    private Regex passwordPattern = new Regex(@"^(?=.*?[a-z])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[\W]).{8,}$");

    private void Awake()
    {
        Caching.ClearCache();
        
        SecurityPlayerPrefs.DeleteKey("JWTAccess");
        SecurityPlayerPrefs.DeleteKey("JWTRefresh");
        SecurityPlayerPrefs.Save();
    }

    private void Start()
    {
        StartCoroutine(CheckAsset());
    }
}