using System;
using System.Text;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class APIManager
{
    //서버 주소.
    public static string serverAddress = "https://api.wizard87.com";

    
    //API서버와 통신 하는 함수.
    public static async Task<ResponseJsonModel> SendAPIRequestAsync
        (API apiType, RequestJsonModel request, UnityAction<string, int, string> failureAction)
    {
        ResponseJsonModel response = null;

        string jsonString = null;

        try
        {
            jsonString = JObject.FromObject(request).ToString();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{e.Message}\n{e.StackTrace}");
            
            if(failureAction != null)
                failureAction("JsonSerializeError", 0, $"{e.Message}\n{e.StackTrace}");
            
            return null;
        }
        
        using (UnityWebRequest unityWebRequest = new UnityWebRequest(ServerAPI.getURL(apiType), "POST"))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");
            unityWebRequest.timeout = 15;
            
            byte[] jsonStringBinary = Encoding.UTF8.GetBytes(jsonString);
            unityWebRequest.uploadHandler = new UploadHandlerRaw(jsonStringBinary);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                //http 송신 및 응답 대기.
                var operation = await unityWebRequest.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e.Message}\n{e.StackTrace}");
                
                if(failureAction != null)
                    failureAction("SendWebRequestError", 0, $"{e.Message}\n{e.StackTrace}");
            }
            
            //서버 관련 에러
            if (unityWebRequest.isHttpError)
            {
                Debug.LogWarning($"HttpError:{unityWebRequest.error}");
                
                if(failureAction != null)
                    failureAction("HttpError", (int)unityWebRequest.responseCode, unityWebRequest.error);
            }
            //네트워크 환경 관련 에러
            else if (unityWebRequest.isNetworkError)
            {
                Debug.LogWarning($"NetworkError:{unityWebRequest.error}");
                
                if(failureAction != null)
                    failureAction("NetworkError", (int)unityWebRequest.responseCode, unityWebRequest.error);
            }
            //정상적인 경우.
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(unityWebRequest.downloadHandler.text))
                    {
                        JObject result = JObject.Parse(unityWebRequest.downloadHandler.text);
                        
                        response = ServerAPI.parseJsonModel(apiType, result);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{e.Message}\n{e.StackTrace}");
                    
                    if(failureAction != null)
                        failureAction("JsonParseError", 0, $"{e.Message}\n{e.StackTrace}");
                    return null;
                }
            }
        }

        return response;
    }
}

public class ServerAPI
{
    public static string getURL(API apiType)
    {
        switch (apiType)
        {
<<<<<<< HEAD
            case API.versioncheck:
                return $"{APIManager.serverAddress}/version-check";
            
            case API.auth_login:
=======
            case API.Login:
>>>>>>> 029fd61... 리팩토링 1차 재작업
                return $"{APIManager.serverAddress}/auth/login";
            
            case API.RegisterAuthNumber:
                return $"{APIManager.serverAddress}/auth/join/send-request";
            
            case API.RegisterAuthNumberCheck:
                return $"{APIManager.serverAddress}/auth/join/send-auth-number";
            
            case API.Register:
                return $"{APIManager.serverAddress}/auth/join";
            
            case API.PasswordFindAuthNumber:
                return $"{APIManager.serverAddress}/auth/findpassword/send-request";
            
            case API.PasswordFindAuthNumberCheck:
                return $"{APIManager.serverAddress}/auth/findpassword/send-auth-number";
            
            case API.PasswordChange:
                return $"{APIManager.serverAddress}/auth/findpassword/update-account-password";
            
            case API.UserData:
                return $"{APIManager.serverAddress}/user/gamedata";
            
            case API.UserNicknameUpdate:
                return $"{APIManager.serverAddress}/user/gamedata/update-username";
            
<<<<<<< HEAD
            
=======
            case API.UserNicknameCheck:
                return $"{APIManager.serverAddress}/user/gamedata/check-username";

>>>>>>> 029fd61... 리팩토링 1차 재작업
            default:
                return String.Empty;
        }
    }
    
    public static ResponseJsonModel parseJsonModel(API apiType, JObject jsonObject)
    {
        switch (apiType)
        {
<<<<<<< HEAD
            case API.versioncheck:
                return jsonObject.ToObject<Response_VersionUpdate>();
            
            case API.auth_login:
                return jsonObject.ToObject<Response_Auth_Login>();
=======
            case API.Login:
                return jsonObject.ToObject<ResponseLogin>();
>>>>>>> 029fd61... 리팩토링 1차 재작업
            
            case API.RegisterAuthNumber:
                return jsonObject.ToObject<ResponseRegisterAuthNumber>();
            
            case API.RegisterAuthNumberCheck:
                return jsonObject.ToObject<ResponseRegisterAuthNumberCheck>();
            
            case API.Register:
                return jsonObject.ToObject<ResponseRegister>();

            case API.PasswordFindAuthNumber:
                return jsonObject.ToObject<ResponsePasswordFindAuthNumber>();
            
            case API.PasswordFindAuthNumberCheck:
                return jsonObject.ToObject<ResponsePasswordFindAuthNumberCheck>();
            
            case API.PasswordChange:
                return jsonObject.ToObject<ResponsePasswordChange>();
            
            case API.UserData:
                return jsonObject.ToObject<ResponseUserData>();
            
            case API.UserNicknameUpdate:
                return jsonObject.ToObject<ResponseUserNiknameUpdate>();
            
<<<<<<< HEAD
            
=======
            case API.UserNicknameCheck:
                return jsonObject.ToObject<ResponseUserNicknameCheck>();

>>>>>>> 029fd61... 리팩토링 1차 재작업
            default:
                return null;
        }
    }
}

public enum API
{
<<<<<<< HEAD
    versioncheck,
    auth_login,
    auth_join_sendrequest,
    auth_join_sendauthnumber,
    auth_join,
    auth_findpassword_sendrequest,
    auth_findpassword_sendauthnumber,
    auth_findpassword_updateaccountpassword,
    user_gamedata,
    user_gamedata_updateusername
=======
    Login,
    RegisterAuthNumber,
    RegisterAuthNumberCheck,
    Register,
    PasswordFindAuthNumber,
    PasswordFindAuthNumberCheck,
    PasswordChange,
    UserData,
    UserNicknameUpdate,
    UserNicknameCheck
>>>>>>> 029fd61... 리팩토링 1차 재작업
}