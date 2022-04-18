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
    public static async Task<Response_JsonModel> SendAPIRequestAsync
        (API apiType, Request_JsonModel request, UnityAction<string, int, string> failureAction)
    {
        Response_JsonModel response = null;

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
            case API.auth_login:
                return $"{APIManager.serverAddress}/auth/login";
            
            case API.auth_join_sendrequest:
                return $"{APIManager.serverAddress}/auth/join/send-request";
            
            case API.auth_join_sendauthnumber:
                return $"{APIManager.serverAddress}/auth/join/send-auth-number";
            
            case API.auth_join:
                return $"{APIManager.serverAddress}/auth/join";
            
            case API.auth_findpassword_sendrequest:
                return $"{APIManager.serverAddress}/auth/findpassword/send-request";
            
            case API.auth_findpassword_sendauthnumber:
                return $"{APIManager.serverAddress}/auth/findpassword/send-auth-number";
            
            case API.auth_findpassword_updateaccountpassword:
                return $"{APIManager.serverAddress}/auth/findpassword/update-account-password";
            
            case API.user_gamedata:
                return $"{APIManager.serverAddress}/user/gamedata";
            
            case API.user_gamedata_updateusername:
                return $"{APIManager.serverAddress}/user/gamedata/update-username";

            default:
                return String.Empty;
        }
    }
    
    public static Response_JsonModel parseJsonModel(API apiType, JObject jsonObject)
    {
        switch (apiType)
        {
            case API.auth_login:
                return jsonObject.ToObject<Response_Auth_Login>();
            
            case API.auth_join_sendrequest:
                return jsonObject.ToObject<Response_Auth_Join_SendRequest>();
            
            case API.auth_join_sendauthnumber:
                return jsonObject.ToObject<Response_Auth_Join_SendAuthNumber>();
            
            case API.auth_join:
                return jsonObject.ToObject<Response_Auth_Join>();

            case API.auth_findpassword_sendrequest:
                return jsonObject.ToObject<Response_Auth_FindPassword_SendRequest>();
            
            case API.auth_findpassword_sendauthnumber:
                return jsonObject.ToObject<Response_Auth_FindPassword_SendAuthNumber>();
            
            case API.auth_findpassword_updateaccountpassword:
                return jsonObject.ToObject<Response_Auth_FindPassword_UpdateAccountPassword>();
            
            case API.user_gamedata:
                return jsonObject.ToObject<Response_User_Gamedata>();
            
            case API.user_gamedata_updateusername:
                return jsonObject.ToObject<Response_User_Gamedata_UpdateUserName>();

            default:
                return null;
        }
    }
}

public enum API
{
    auth_login,
    auth_join_sendrequest,
    auth_join_sendauthnumber,
    auth_join,
    auth_findpassword_sendrequest,
    auth_findpassword_sendauthnumber,
    auth_findpassword_updateaccountpassword,
    user_gamedata,
    user_gamedata_updateusername
}