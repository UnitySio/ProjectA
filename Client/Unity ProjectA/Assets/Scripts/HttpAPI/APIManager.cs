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

            if (failureAction != null)
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
            case API.SendSignInAuthNumber:
                return $"{APIManager.serverAddress}/signin/authnumber";
            
            case API.VerifySignInAuthNumber:
                return $"{APIManager.serverAddress}/signin/authnumber/verify";
            
            case API.SignIn:
                return $"{APIManager.serverAddress}/signin";

            case API.UserData:
                return $"{APIManager.serverAddress}/userdata";
            
            case API.UpdateUserNickname:
                return $"{APIManager.serverAddress}/userdata/nickname/update";
            
            case API.AddCharacter:
                return $"{APIManager.serverAddress}/userdata/character/add";
            
            case API.GetCharacter:
                return $"{APIManager.serverAddress}/userdata/character";

            default:
                return String.Empty;
        }
    }
    
    public static ResponseJsonModel parseJsonModel(API apiType, JObject jsonObject)
    {
        switch (apiType)
        {
            case API.SendSignInAuthNumber:
                return jsonObject.ToObject<ResponseSendSignInAuthNumber>();
            
            case API.VerifySignInAuthNumber:
                return jsonObject.ToObject<ResponseVerifySignInAuthNumber>();
            
            case API.SignIn:
                return jsonObject.ToObject<ResponseSignIn>();
            
            case API.UserData:
                return jsonObject.ToObject<ResponseUserData>();
            
            case API.UpdateUserNickname:
                return jsonObject.ToObject<ResponseUpdateUserNickname>();
            
            case API.AddCharacter:
                return jsonObject.ToObject<ResponseAddCharacter>();
            
            case API.GetCharacter:
                return jsonObject.ToObject<ResponseGetCharacter>();

            default:
                return null;
        }
    }
}

public enum API
{
    SendSignInAuthNumber,
    VerifySignInAuthNumber,
    SignIn,
    UserData,
    UpdateUserNickname,
    AddCharacter,
    GetCharacter
}