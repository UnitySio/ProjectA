using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("LoginManager Settings")] 
    public string serverAddress = "https://db.wizard87.com";
    public string clientversion = "2021.08.01.121";
    
    public LoginSequenceStep currentStep;
    public GameObject image_Buffering;
    public popup_window popupWindow;
    
    
    [Header("Panel_BG")] 
    public GameObject panel_BG;
    public Text text_version;
    public Text text_loginStatus;
    public Button btn_logout;

    [Header("Panel_01_AccountTypeSelect")] 
    public GameObject panel_AccountTypeSelect;
    public Button btn_login_siogames;
    public Button btn_login_google;
    public Button btn_login_apples;
    public Button btn_login_guest;

    [Header("Panel_02_LoginAccount_SioGames")]
    public GameObject panel_LoginAccount_SioGames;
    public Button btn_backmaintitle;
    public InputField inputfield_login_email;
    public InputField inputfield_login_pw;
    public Text text_loginResult;
    public Button btn_login;
    public Button btn_join;
    public Button btn_findpassword;

    [Header("Panel_03_CreateAccount_SioGames")]
    public GameObject panel_CreateAccount_SioGames;
    public Button btn_backaccountlogin;
    public InputField inputfield_join_email;
    public InputField inputfield_join_pw;
    public InputField inputfield_join_pw_retry;
    public Text text_joinResult;
    public Button btn_join_request;

    [Header("Panel_04_FindAccount_SioGames")]
    public GameObject panel_FindAccount_SioGames;
    public GameObject panel_SendEmail;
    public GameObject panel_CheckEmail;
    public GameObject panel_ResetPassword;

    public Button btn_sendemail_backaccountlogin;
    public InputField inputfield_sendemail_email;
    public Button btn_sendemail;
    public Text text_sendemail_findpasswordstatus;

    public InputField inputfield_checkemail_authnumber;
    public Button btn_checkemail_resetcode;
    public Text text_checkemail_findpasswordstatus;

    public InputField inputfield_resetpw_newpw;
    public InputField inputfield_resetpw_newpw_retry;
    public Button btn_resetpw_submit_newpw;
    public Text text_resetpw_findpasswordstatus;

    [Header("Panel_05_CreateNickname")] 
    public GameObject panel_CreateNickname;
    public InputField inputfield_nickname;
    public Text text_createnicknamestatus;
    public Button btn_submit_nickname;

    [Header("Panel_06_StartGame")] 
    public GameObject panel_StartGame;
    public Button btn_startgame;


    void Awake()
    {
        init_LoginManager();
    }

    void Update()
    {
        text_version.text = $"build_version_{clientversion}";
        text_loginStatus.text = getSequenceStepMessage(currentStep);
    }


    void Start()
    {
        checkVersionUpdate();
    }



    #region Login Sequence Methods

    //버전체크.
    async void checkVersionUpdate()
    {
        currentStep = LoginSequenceStep.versioncheck;

        var request = new Request_VersionCheck()
        {
            currentClientVersion = clientversion
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(1000);
                        checkVersionUpdate();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }
                
                
                popupWindow.showwindow();
            };

        var response = await APIManager.SendAPIRequestAsync(API.versioncheck, request, failureCallback);

        if (response != null)
        {
            await UniTask.Delay(333);
            Response_VersionUpdate result = response as Response_VersionUpdate;

            var text = result.result;

            //업데이트 할 필요가 있다면.
            if (result.needUpdate)
            {
                //추후 업데이트 관련 함수 실행.
            }
            //최신버전이어서 업데이트할 필요가 없다면.
            else
            {
                confirmJWT();
            }
        }
    }

    //JWT토큰 유무여부 확인.
    async void confirmJWT()
    {
        currentStep = LoginSequenceStep.confirmJWT;
        
        var jwt_access = SecurityPlayerPrefs.GetString("jwt_access", null);
        var jwt_refresh = SecurityPlayerPrefs.GetString("jwt_refresh", null);

        await UniTask.Delay(333);
        
        //토큰이 없으므로 로그인 화면으로 이동..
        if (string.IsNullOrEmpty(jwt_access) || string.IsNullOrEmpty(jwt_refresh))
        {
            waitingLogin();
        }
        //토큰이 있다면 유효성 체크단계로 이행.
        else
        {
            checkValidateJWT();
        }
    }

    async void checkValidateJWT()
    {
        currentStep = LoginSequenceStep.validateCheckJWT;
        
        var jwt_access = SecurityPlayerPrefs.GetString("jwt_access", null);
        var jwt_refresh = SecurityPlayerPrefs.GetString("jwt_refresh", null);

        var access_token = JWTManager.DecryptJWT(jwt_access);
        var refresh_token = JWTManager.DecryptJWT(jwt_refresh);
        
        await UniTask.Delay(333);
        
        //아직 access_token이 유효하다면
        if (JWTManager.checkValidateJWT(access_token))
        {
            //게임 시작 버튼 표시 화면으로 이동.
            completeAuthenticate();
        }
        //refresh_token이 유효하고 access_token토큰 갱신이 필요하다면
        else if(JWTManager.checkValidateJWT(refresh_token))
        {
            //JWT토큰 갱신
            refreshJWT();
        }
        //모든 토큰이 만료된 경우
        else
        {
            //로그인 버튼 표시
            waitingLogin();
        }
    }

    //토큰 갱신
    async void refreshJWT()
    {
        currentStep = LoginSequenceStep.requestrefreshJWT;
        
        await UniTask.Delay(333);
        
        var refresh_token = SecurityPlayerPrefs.GetString("jwt_refresh", null);
        
        var request = new Request_Auth_Login()
        {
            authType = "jwt",
            jwt_refresh = refresh_token
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(1000);
                        refreshJWT();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }
                
                
                popupWindow.showwindow();
            };

        var response = await APIManager.SendAPIRequestAsync(API.auth_login, request, failureCallback);

        if (response != null)
        {
            Response_Auth_Login result = response as Response_Auth_Login;
            
            var text = result.result;

            if (text.Equals("ok"))
            {
                SecurityPlayerPrefs.SetString("jwt_access", result.jwt_access);
                SecurityPlayerPrefs.SetString("jwt_refresh", result.jwt_refresh);
                SecurityPlayerPrefs.Save();
                
                completeAuthenticate();
            }
            else
            {
                currentStep = LoginSequenceStep.none;
                //에러 발생.
                popupWindow.text_message.text = $"error:{text}";
                popupWindow.setConfirm();
                popupWindow.btn_confirm.onClick.AddListener(() =>
                {
                    popupWindow.closewindow();

                    SceneManager.LoadScene("01 Login");
                });
                popupWindow.showwindow();
            }
        }
    }

    async void waitingLogin()
    {
        currentStep = LoginSequenceStep.waitingLogin;

        panel_AccountTypeSelect.SetActive(true);
        init_Panel_01_AccountTypeSelect();
        btn_login_siogames.onClick.AddListener(async () =>
        {
            btn_login_siogames.interactable = false;

            openSiogamesLoginUI();

            await UniTask.Delay(1000);
            btn_login_siogames.interactable = true;
        });
        
        btn_login_guest.onClick.AddListener(async() =>
        {
            btn_login_guest.interactable = false;

            await startGuestLogin();

            btn_login_guest.interactable = true;
        });
    }

    Regex emailPattern = new Regex("^([a-zA-Z0-9-]+\\@[a-zA-Z0-9-]+\\.[a-zA-Z]{2,10})*$");
    
    void openSiogamesLoginUI()
    {
        init_Panel_02_LoginAccount_SioGames();
        panel_LoginAccount_SioGames.SetActive(true);
        btn_backmaintitle.onClick.AddListener(() =>
        {
            panel_LoginAccount_SioGames.SetActive(false);
        });
        
        btn_login.onClick.AddListener(async() =>
        {
            text_loginResult.text = string.Empty;
            
            var email = inputfield_login_email.text;
            var pw = HashManager.HashPassword(inputfield_login_pw.text.Trim());
            
            inputfield_login_email.onValueChanged.RemoveAllListeners();
            inputfield_login_email.onValueChanged.AddListener((args) =>
            {
                text_loginResult.text = string.Empty;
            });
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(inputfield_login_pw.text))
            {
                text_loginResult.text = "blank";
            }
            else if (!emailPattern.IsMatch(email))
            {
                text_loginResult.text = "not validate email type";
            }
            else
            {
                btn_login.interactable = false;

                await startSiogamesLogin(email, pw);
                
                btn_login.interactable = true;
            }
        });
        
        
        btn_join.onClick.AddListener(() =>
        {
            init_Panel_02_LoginAccount_SioGames();
            openSiogamesCreateAccountUI();
        });
        
        btn_findpassword.onClick.AddListener(() =>
        {
            init_Panel_02_LoginAccount_SioGames();
            openSiogamesFindAccountUI();
        });
    }

    void openSiogamesCreateAccountUI()
    {
        init_Panel_03_CreateAccount_SioGames();
        btn_backaccountlogin.onClick.AddListener(() =>
        {
            panel_CreateAccount_SioGames.gameObject.SetActive(false);
            openSiogamesLoginUI();
        });
        panel_CreateAccount_SioGames.gameObject.SetActive(true);

        btn_join_request.onClick.AddListener(async () =>
        {
            var email = inputfield_join_email.text;
            var pw = HashManager.HashPassword(inputfield_join_pw.text.Trim());
            var pw_retry = HashManager.HashPassword(inputfield_join_pw_retry.text.Trim());
            
            inputfield_join_email.onValueChanged.RemoveAllListeners();
            inputfield_join_email.onValueChanged.AddListener((args) =>
            {
                text_joinResult.text = string.Empty;
            });
            
            if (string.IsNullOrEmpty(email) 
                || string.IsNullOrEmpty(inputfield_join_pw.text)
                || string.IsNullOrEmpty(inputfield_join_pw_retry.text))
            {
                text_joinResult.text = "blank";
            }
            else if (!emailPattern.IsMatch(email))
            {
                text_joinResult.text = "not validate email type";
            }
            else if (!pw.Equals(pw_retry))
            {
                text_joinResult.text = "not matching password";
            }
            else
            {
                btn_join_request.interactable = false;

                await startSiogamesJoin(email, pw);
                
                btn_join_request.interactable = true;
            }
        });
    }

    void openSiogamesFindAccountUI()
    {
        init_Panel_04_FindAccount_SioGames();
        btn_sendemail_backaccountlogin.onClick.AddListener(() =>
        {
            panel_FindAccount_SioGames.gameObject.SetActive(false);
            openSiogamesLoginUI();
        });
        panel_FindAccount_SioGames.gameObject.SetActive(true);
        
        btn_sendemail.onClick.AddListener(async () =>
        {
            var email = inputfield_sendemail_email.text;
            
            inputfield_sendemail_email.onValueChanged.RemoveAllListeners();
            inputfield_sendemail_email.onValueChanged.AddListener((args) =>
            {
                text_sendemail_findpasswordstatus.text = string.Empty;
            });
            
            if (string.IsNullOrEmpty(email))
            {
                text_sendemail_findpasswordstatus.text = "email field is blank";
            }
            else if (!emailPattern.IsMatch(email))
            {
                text_sendemail_findpasswordstatus.text = "not validate email type";
            }
            else
            {
                btn_sendemail.interactable = false;

                await startSiogamesFindPassword(email);
                
                btn_sendemail.interactable = true;
            }
        });
    }
    
    
    async UniTask startSiogamesLogin(string email, string pw_hash)
    {
        currentStep = LoginSequenceStep.requestLogin;
        
        var request = new Request_Auth_Login()
        {
            authType = "account",
            account_email = email,
            account_password = pw_hash
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }

                popupWindow.showwindow();
            };

        await UniTask.Delay(333);
        currentStep = LoginSequenceStep.pendingLogin;

        var response = await APIManager
            .SendAPIRequestAsync(API.auth_login, request, failureCallback);
        
        if (response != null)
        {
            Response_Auth_Login result = response as Response_Auth_Login;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var jwt_access = result.jwt_access;
                var jwt_refresh = result.jwt_refresh;
            
                SecurityPlayerPrefs.SetString("jwt_access", jwt_access);
                SecurityPlayerPrefs.SetString("jwt_refresh", jwt_refresh);
                SecurityPlayerPrefs.Save();

                completeAuthenticate();
            }
            else
            {
                currentStep = LoginSequenceStep.waitingLogin;
                
                text_loginResult.text = "wrong auth info";
            }
        }
    }
    
    async Task startGuestLogin()
    {
        currentStep = LoginSequenceStep.requestLogin;
        
        
    }

    void startGoogleLogin()
    {
        currentStep = LoginSequenceStep.requestLogin;
        
        
    }

    void startAppleLogin()
    {
        currentStep = LoginSequenceStep.requestLogin;
        
        
    }


    async UniTask startSiogamesJoin(string email, string pw_hash)
    {
        currentStep = LoginSequenceStep.requestJoin;
        
        var request = new Request_Auth_Join()
        {
            authType = "account",
            account_email = email,
            account_password = pw_hash
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }

                popupWindow.showwindow();
            };

        await UniTask.Delay(333);
        currentStep = LoginSequenceStep.pendingJoin;
        
        var response = await APIManager
            .SendAPIRequestAsync(API.auth_join, request, failureCallback);
        
        if (response != null)
        {
            Response_Auth_Join result = response as Response_Auth_Join;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var jwt_access = result.jwt_access;
                var jwt_refresh = result.jwt_refresh;
            
                SecurityPlayerPrefs.SetString("jwt_access", jwt_access);
                SecurityPlayerPrefs.SetString("jwt_refresh", jwt_refresh);
                SecurityPlayerPrefs.Save();

                completeAuthenticate();
            }
            else
            {
                currentStep = LoginSequenceStep.none;
                text_joinResult.text = "already signed account info";
            }
        }
    }


    async UniTask startSiogamesFindPassword(string email)
    {
        var request = new Request_Auth_FindPassword_SendRequest()
        {
            account_email = email
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }

                popupWindow.showwindow();
            };

        var response = await APIManager
            .SendAPIRequestAsync(API.auth_findpassword_sendrequest, request, failureCallback);
        
        if (response != null)
        {
            Response_Auth_FindPassword_SendRequest result = response as Response_Auth_FindPassword_SendRequest;

            var text = result.result;

            if (text.Equals("ok"))
            {
                var findpassword_token = result.findpassword_token;
                
                SecurityPlayerPrefs.SetString("findpassword_token", findpassword_token);
                SecurityPlayerPrefs.Save();

                panel_SendEmail.gameObject.SetActive(false);
                panel_CheckEmail.gameObject.SetActive(true);
                
                btn_checkemail_resetcode.onClick.AddListener(async () =>
                {
                    var authnumber = inputfield_checkemail_authnumber.text;
            
                    inputfield_checkemail_authnumber.onValueChanged.RemoveAllListeners();
                    inputfield_checkemail_authnumber.onValueChanged.AddListener((args) =>
                    {
                        text_checkemail_findpasswordstatus.text = string.Empty;
                    });
            
                    if (string.IsNullOrEmpty(authnumber))
                    {
                        text_checkemail_findpasswordstatus.text = "field is blank";
                    }
                    else
                    {
                        btn_checkemail_resetcode.interactable = false;

                        await startSiogamesFindPassword_AuthNumber(authnumber);
                
                        btn_checkemail_resetcode.interactable = true;
                    }
                });
            }
            else
            {
                currentStep = LoginSequenceStep.none;
                text_sendemail_findpasswordstatus.text = "not validate account";
            }
        }
    }


    async UniTask startSiogamesFindPassword_AuthNumber(string numberText)
    {
        var token = SecurityPlayerPrefs.GetString("findpassword_token", "");
        var request = new Request_Auth_FindPassword_SendAuthNumber()
        {
            findpassword_token = token,
            auth_number = numberText
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }

                popupWindow.showwindow();
            };

        var response = await APIManager
            .SendAPIRequestAsync(API.auth_findpassword_sendauthnumber, request, failureCallback);
        
        if (response != null)
        {
            var result = response as Response_Auth_FindPassword_SendAuthNumber;

            var text = result.result;

            if (text.Equals("ok"))
            {
                panel_CheckEmail.gameObject.SetActive(false);
                panel_ResetPassword.gameObject.SetActive(true);
                
                btn_resetpw_submit_newpw.onClick.AddListener(async () =>
                {
                    var newpw = HashManager
                        .HashPassword(inputfield_resetpw_newpw.text);
                    var newpw_retry = HashManager
                        .HashPassword(inputfield_resetpw_newpw_retry.text);
            
                    inputfield_resetpw_newpw.onValueChanged.RemoveAllListeners();
                    inputfield_resetpw_newpw.onValueChanged.AddListener((args) =>
                    {
                        text_resetpw_findpasswordstatus.text = string.Empty;
                    });
            
                    if (string.IsNullOrEmpty(newpw) || string.IsNullOrEmpty(newpw_retry))
                    {
                        text_resetpw_findpasswordstatus.text = "need input password";
                    }
                    else if (!newpw.Equals(newpw_retry))
                    {
                        text_resetpw_findpasswordstatus.text = "not matching password";
                    }
                    else
                    {
                        btn_resetpw_submit_newpw.interactable = false;

                        await startSiogamesFindPassword_ResetPassword(newpw);
                
                        btn_resetpw_submit_newpw.interactable = true;
                    }
                });
            }
            else
            {
                currentStep = LoginSequenceStep.none;
                text_checkemail_findpasswordstatus.text = "wrong auth number";
            }
        }
    }



    async UniTask startSiogamesFindPassword_ResetPassword(string newPassword)
    {
        var token = SecurityPlayerPrefs.GetString("findpassword_token", "");
        var request = new Request_Auth_FindPassword_UpdateAccountPassword()
        {
            findpassword_token = token,
            account_password = newPassword
        };

        //에러시 호출되는 콜백함수.
        UnityAction<string, int, string> failureCallback =
            (errorType, responseCode, errorMessage) =>
            {
                currentStep = LoginSequenceStep.none;
                
                if (errorType.ToLower().Contains("http"))
                {
                    popupWindow.text_message.text = $"server error:{responseCode}";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(() =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else if (errorType.ToLower().Contains("network"))
                {
                    popupWindow.text_message.text = $"check network environment";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                    });
                }
                else
                {
                    popupWindow.text_message.text = $"unknown error";
                    popupWindow.setConfirm();
                    popupWindow.btn_confirm.onClick.AddListener(async () =>
                    {
                        popupWindow.closewindow();
                        
                        await UniTask.Delay(500);
                        Application.Quit();
                    });
                }

                popupWindow.showwindow();
            };

        var response = await APIManager
            .SendAPIRequestAsync(API.auth_findpassword_updateaccountpassword, request, failureCallback);
        
        if (response != null)
        {
            var result = response as Response_Auth_FindPassword_UpdateAccountPassword;

            var text = result.result;

            if (text.Equals("ok"))
            {
                panel_FindAccount_SioGames.SetActive(false);
                init_Panel_04_FindAccount_SioGames();
                init_Panel_02_LoginAccount_SioGames();
                openSiogamesLoginUI();
                
                waitingLogin();
            }
            else
            {
                currentStep = LoginSequenceStep.none;
                text_resetpw_findpasswordstatus.text = $"server error : {text}";
            }
        }
    }
    
    
    async void completeAuthenticate()
    {
        init_LoginManager();
        currentStep = LoginSequenceStep.completeAuthenticate;
        
        btn_logout.gameObject.SetActive(true);
        btn_logout.onClick.AddListener(async () =>
        {
            btn_logout.interactable = false;
            
            //jwt토큰 제거 및 로그인 화면으로 이동
            await Logout();
            
            btn_logout.interactable = true;
            
            btn_logout.gameObject.SetActive(false);
            panel_StartGame.SetActive(false);
        });
        
        await UniTask.Delay(333);
        
        panel_StartGame.SetActive(true);
    }

    async Task Logout()
    {
        image_Buffering.SetActive(true);
        await UniTask.Delay(333);
        
        SecurityPlayerPrefs.DeleteKey("jwt_access");
        SecurityPlayerPrefs.DeleteKey("jwt_refresh");
        SecurityPlayerPrefs.DeleteKey("guest_token");
        SecurityPlayerPrefs.Save();
        
        waitingLogin();
        image_Buffering.SetActive(false);
    }
    #endregion
    

    #region Panel Initialize Methods

    void init_LoginManager()
    {
        init_Panel_BG();
        init_Panel_01_AccountTypeSelect();
        init_Panel_02_LoginAccount_SioGames();
        init_Panel_03_CreateAccount_SioGames();
        init_Panel_04_FindAccount_SioGames();
        init_Panel_05_CreateNickname();
        init_Panel_06_StartGame();
        
        if(!string.IsNullOrEmpty(serverAddress))
            APIManager.serverAddress = serverAddress;
        
        image_Buffering.SetActive(false);
        panel_BG.SetActive(true);
        panel_AccountTypeSelect.SetActive(false);
        panel_LoginAccount_SioGames.SetActive(false);
        panel_CreateAccount_SioGames.SetActive(false);
        panel_FindAccount_SioGames.SetActive(false);
        panel_CreateNickname.SetActive(false);
        panel_StartGame.SetActive(false);

        currentStep = LoginSequenceStep.none;
    }
    
    void init_Panel_BG()
    {
        text_loginStatus.text = string.Empty;
        btn_logout.gameObject.SetActive(false);
        btn_logout.onClick.RemoveAllListeners();
    }

    void init_Panel_01_AccountTypeSelect()
    {
        btn_login_siogames.onClick.RemoveAllListeners();
        btn_login_google.onClick.RemoveAllListeners();
        btn_login_apples.onClick.RemoveAllListeners();
        btn_login_guest.onClick.RemoveAllListeners();
    }

    void init_Panel_02_LoginAccount_SioGames()
    {
        btn_backmaintitle.onClick.RemoveAllListeners();
        inputfield_login_email.text = string.Empty;
        inputfield_login_pw.text = string.Empty;
        text_loginResult.text = string.Empty;
        btn_login.onClick.RemoveAllListeners();
        btn_join.onClick.RemoveAllListeners();
        btn_findpassword.onClick.RemoveAllListeners();
    }

    void init_Panel_03_CreateAccount_SioGames()
    {
        btn_backaccountlogin.onClick.RemoveAllListeners();
        inputfield_join_email.text = string.Empty;
        inputfield_join_pw.text = string.Empty;
        inputfield_join_pw_retry.text = string.Empty;
        text_joinResult.text = string.Empty;
        btn_join_request.onClick.RemoveAllListeners();
    }

    void init_Panel_04_FindAccount_SioGames()
    {
        panel_SendEmail.SetActive(true);
        panel_CheckEmail.SetActive(false);
        panel_ResetPassword.SetActive(false);

        btn_sendemail_backaccountlogin.onClick.RemoveAllListeners();
        inputfield_sendemail_email.text = string.Empty;
        btn_sendemail.onClick.RemoveAllListeners();
        text_sendemail_findpasswordstatus.text = string.Empty;

        inputfield_checkemail_authnumber.text = string.Empty;
        btn_checkemail_resetcode.onClick.RemoveAllListeners();
        text_checkemail_findpasswordstatus.text = string.Empty;

        inputfield_resetpw_newpw.text = string.Empty;
        inputfield_resetpw_newpw_retry.text = string.Empty;
        btn_resetpw_submit_newpw.onClick.RemoveAllListeners();
        text_resetpw_findpasswordstatus.text = string.Empty;
    }

    void init_Panel_05_CreateNickname()
    {
        inputfield_nickname.text = string.Empty;
        text_createnicknamestatus.text = string.Empty;
        btn_submit_nickname.onClick.RemoveAllListeners();
    }

    void init_Panel_06_StartGame()
    {
        btn_startgame.onClick.RemoveAllListeners();
    }

    #endregion

    
    
    #region Utils Methods

    public long getUnixTimeNowSeconds() //1 Sec단위.
    {
        var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return (long)timeSpan.TotalSeconds;
    }

    string getSequenceStepMessage(LoginSequenceStep currentStep)
    {
        // switch (currentStep)
        // {
        //     case LoginSequenceStep.versioncheck:
        //         return "버전 체크중...";
        //     case LoginSequenceStep.updatedownloading:
        //         return "업데이트 다운로드중...";
        //     case LoginSequenceStep.filehashchecking:
        //         return "파일 무결성 검사중...";
        //     case LoginSequenceStep.dataloading:
        //         return "데이터 로드중...";
        //     case LoginSequenceStep.confirmJWT:
        //         return "인증토큰 확인중...";
        //     case LoginSequenceStep.validateCheckJWT:
        //         return "인증토큰 유효성 검사중...";
        //     case LoginSequenceStep.requestrefreshJWT:
        //         return "인증토큰 갱신 요청중...";
        //     case LoginSequenceStep.waitingLogin:
        //         return "로그인 대기";
        //     case LoginSequenceStep.requestLogin:
        //         return "로그인 요청";
        //     case LoginSequenceStep.pendingLogin:
        //         return "로그인 요청 대기중...";
        //     case LoginSequenceStep.requestJoin:
        //         return "계정생성 요청";
        //     case LoginSequenceStep.pendingJoin:
        //         return "계정생성 요청 대기중...";
        //     case LoginSequenceStep.completeAuthenticate:
        //         return "사용자 인증 완료";
        //
        //     default:
        //         return string.Empty;
        // }
        
        switch (currentStep)
        {
            case LoginSequenceStep.versioncheck:
                return "version checking...";
            case LoginSequenceStep.updatedownloading:
                return "update...";
            case LoginSequenceStep.filehashchecking:
                return "check file hash...";
            case LoginSequenceStep.dataloading:
                return "loading data...";
            case LoginSequenceStep.confirmJWT:
                return "auth token check...";
            case LoginSequenceStep.validateCheckJWT:
                return "auth token validate check...";
            case LoginSequenceStep.requestrefreshJWT:
                return "auth token refreshing...";
            case LoginSequenceStep.waitingLogin:
                return "stanby login";
            case LoginSequenceStep.requestLogin:
                return "sign in request";
            case LoginSequenceStep.pendingLogin:
                return "sign in request...";
            case LoginSequenceStep.requestJoin:
                return "sign up request";
            case LoginSequenceStep.pendingJoin:
                return "sign up request...";
            case LoginSequenceStep.completeAuthenticate:
                return "authentication complete";

            default:
                return string.Empty;
        }
    }

    #endregion
}


public enum LoginSequenceStep
{
    none,
    versioncheck,
    updatedownloading,
    filehashchecking,
    dataloading,
    confirmJWT,
    validateCheckJWT,
    requestrefreshJWT,
    waitingLogin,
    requestLogin,
    pendingLogin,
    requestJoin,
    pendingJoin,
    completeAuthenticate
}