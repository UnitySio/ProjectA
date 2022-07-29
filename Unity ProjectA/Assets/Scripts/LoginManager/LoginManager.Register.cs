using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManger.Register")]
    public GameObject registerGroup;
    public TextMeshProUGUI registerResult;
    public TMP_InputField registerEmail;
    public TMP_InputField registerAuthNumber;
    public TMP_InputField registerPassword;
    public TMP_InputField registerPasswordCheck;
    public Button registerAuthNumberRequest;
    public Button register;

    private void OpenUnknownRegister()
    {
        var token = "";
        var email = "";
        
        registerResult.text = string.Empty;
        registerEmail.text = string.Empty;
        registerAuthNumber.text = string.Empty;
        registerPassword.text = string.Empty;
        registerPasswordCheck.text = string.Empty;
        registerAuthNumberRequest.onClick.RemoveAllListeners();
        register.onClick.RemoveAllListeners();
        
        loginGroup.SetActive(false);
        registerGroup.SetActive(true);

        registerAuthNumberRequest.onClick.AddListener(async () =>
        {
            email = registerEmail.text;
            
            registerEmail.onValueChanged.RemoveAllListeners();
            registerEmail.onValueChanged.AddListener((args) => registerResult.text = string.Empty);

            if (string.IsNullOrEmpty(email))
                registerResult.text = "ÀÌ¸ŞÀÏÀ» ÀÔ·ÂÇØ ÁÖ¼¼¿ä.";
            else if (!emailPattern.IsMatch(email))
                registerResult.text = "ÀÌ¸ŞÀÏ Çü½ÄÀÌ ¾Æ´Õ´Ï´Ù.";
            else
            {
                registerAuthNumberRequest.interactable = false;
                token = await RequestRegisterAuthNumber(email);
                registerAuthNumberRequest.interactable = true;
            }
        });
        
        register.onClick.AddListener(async () =>
        {
            var authNumber = registerAuthNumber.text;
            var password = HashManager.HashPassword(registerPassword.text.Trim());
            var passwordCheck = HashManager.HashPassword(registerPasswordCheck.text.Trim());

            if (token == "")
                registerResult.text = "ÀÌ¸ŞÀÏ ÀÎÁõÀ» ÇØÁÖ¼¼¿ä.";
            else
            {
                if (string.IsNullOrEmpty(authNumber) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordCheck))
                    registerResult.text = "¸ğµç Ç×¸ñÀ» ÀÔ·ÂÇØ ÁÖ¼¼¿ä.";
                else if (!passwordPattern.IsMatch(registerPassword.text.Trim()))
                    registerResult.text = "ÃÖ¼Ò Æ¯¼ö¹®ÀÚ 1°³, ´ë¼Ò¹®ÀÚ 1°³, ¼ıÀÚ 1°³, 8ÀÚ ÀÌ»ó";
                else if (!password.Equals(passwordCheck))
                    registerResult.text = "ºñ¹Ğ¹øÈ£°¡ ÀÏÄ¡ÇÏÁö ¾Ê½À´Ï´Ù.";
                else
                {
                    register.interactable = false;
                    await RequestRegister(email, password, authNumber, token);
                    register.interactable = true;
                }
            }
        });
    }

    private async Task<string> RequestRegisterAuthNumber(string email)
    {
<<<<<<< HEAD
        var requestAuthNumber = new Request_Auth_Join_SendRequest()
=======
        var request = new RequestRegisterAuthNumber()
>>>>>>> 029fd61... ë¦¬íŒ©í† ë§ 1ì°¨ ì¬ì‘ì—…
        {
            accountEmail = email
        };

<<<<<<< HEAD
        var responseAuthNumber =
            await APIManager.SendAPIRequestAsync(API.auth_join_sendrequest, requestAuthNumber, failureCallback);
        
        Response_Auth_Join_SendRequest responseAuthNumberResult = responseAuthNumber as Response_Auth_Join_SendRequest;
=======
        var response = await APIManager.SendAPIRequestAsync(API.RegisterAuthNumber, request, ServerManager.Instance.FailureCallback);
>>>>>>> 029fd61... ë¦¬íŒ©í† ë§ 1ì°¨ ì¬ì‘ì—…

        if (responseAuthNumberResult.result.Equals("ok"))
        {
<<<<<<< HEAD
            await Task.Delay(333);
            var token = responseAuthNumberResult.join_token;
            registerResult.text = "ÀÎÁõ¹øÈ£´Â 5ºĞ°£ À¯È¿ÇÕ´Ï´Ù.";
=======
            ResponseRegisterAuthNumber result = response as ResponseRegisterAuthNumber;

            var text = result.result;
            
            if (text.Equals("ok"))
            {
                await Task.Delay(333);
                var token = result.registerToken;
                registerResult.text = "ÀÎÁõ¹øÈ£´Â 5ºĞ°£ À¯È¿ÇÕ´Ï´Ù.";
>>>>>>> 029fd61... ë¦¬íŒ©í† ë§ 1ì°¨ ì¬ì‘ì—…

            registerEmail.interactable = false;

            return token;

        }
        else if (responseAuthNumberResult.result.Equals("duplicate email"))
        {
            registerResult.text = "»ç¿ëÁßÀÎ ÀÌ¸ŞÀÏÀÔ´Ï´Ù.";
            return "";
        }
        else
        {
            registerResult.text = "Àß¸øµÈ µ¥ÀÌÅÍÀÔ´Ï´Ù.";
            return "";
        }
    }
    
    private async Task RequestRegister(string email, string password, string authNumber, string registerToken)
    {
        var requestAuthNumber = new RequestRegisterAuthNumberCheck()
        {
            registerToken = registerToken,
            authNumber = authNumber
        };

<<<<<<< HEAD
        var responseAuthNumber =
            await APIManager.SendAPIRequestAsync(API.auth_join_sendauthnumber, requestAuthNumber, failureCallback) as
                Response_Auth_Join_SendAuthNumber;
=======
        var responseAuthNumber = await APIManager.SendAPIRequestAsync(API.RegisterAuthNumberCheck, requestAuthNumber, ServerManager.Instance.FailureCallback) as ResponseRegisterAuthNumberCheck;
>>>>>>> 029fd61... ë¦¬íŒ©í† ë§ 1ì°¨ ì¬ì‘ì—…

        if (responseAuthNumber.result.Equals("ok"))
        {
            var request = new RequestRegister()
            {
                authType = "account",
                accountEmail = email,
                accountPassword = password,
                registerToken = registerToken
            };

            await Task.Delay(333);
<<<<<<< HEAD
            var response = await APIManager.SendAPIRequestAsync(API.auth_join, request, failureCallback);

=======
            var response = await APIManager.SendAPIRequestAsync(API.Register, request, ServerManager.Instance.FailureCallback);
            
>>>>>>> 029fd61... ë¦¬íŒ©í† ë§ 1ì°¨ ì¬ì‘ì—…
            if (response != null)
            {
                ResponseRegister result = response as ResponseRegister;

                var text = result.result;

                if (text.Equals("ok"))
                {
                    var jwtAccess = result.jwtAccess;
                    var jwtRefresh = result.jwtRefresh;

                    SecurityPlayerPrefs.SetString("JWTAccess", jwtAccess);
                    SecurityPlayerPrefs.SetString("JWTRefresh", jwtRefresh);
                    SecurityPlayerPrefs.Save();

                    SceneManager.LoadScene("BattleScene");
                }
                else
                    registerResult.text = "ÀÌ¹Ì °¡ÀÔµÈ °èÁ¤ Á¤º¸ÀÔ´Ï´Ù.";
            }
        }
        else
            registerResult.text = "ÀÎÁõ¹øÈ£¸¦ È®ÀÎÇØ ÁÖ¼¼¿ä.";
    }
}