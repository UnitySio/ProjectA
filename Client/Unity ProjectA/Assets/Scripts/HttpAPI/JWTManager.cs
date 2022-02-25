using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JWTManager : MonoBehaviour
{
    public static JwtSecurityToken DecryptJWT(string jsonWebTokenStr)
    {
        JwtSecurityToken jwt;
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            jwt = tokenHandler.ReadJwtToken(jsonWebTokenStr);
            return jwt;
        }
        catch (Exception)
        {
            
        }
        finally
        {
            tokenHandler = null;
        }
        
        string[] jwts = jsonWebTokenStr?.Split('.');

        if (jwts.Length != 3)
        {
            Debug.LogWarning($"DecryptJWT() jwts.Length :{jwts.Length}");
            return null;
        }
            
        string header = Encoding.UTF8.GetString(Convert.FromBase64String(checkBase64(jwts[0])));
        string payload = Encoding.UTF8.GetString(Convert.FromBase64String(checkBase64(jwts[1])));
        //string sign = Encoding.UTF8.GetString(Convert.FromBase64String(checkBase64(jwts[2])));
            
        JObject resultheader = JObject.Parse(header);
        JObject resultpayload = JObject.Parse(payload);
        
        //Debug.LogWarning($"1 -> {resultheader}\n2 -> {resultpayload}");

        jwt = new JwtSecurityToken();
        
        jwt.Header.Add("alg", resultheader["alg"]);
        jwt.Header.Add("typ", resultheader["typ"]);
        
        jwt.Header.Add("JWTType", resultheader["JWTType"]);
        jwt.Header.Add("AccountUniqueId", resultheader["AccountUniqueId"]);
        jwt.Header.Add("AuthLv", resultheader["AuthLv"]);
        jwt.Header.Add("nbf", resultheader["nbf"]);
        jwt.Header.Add("exp", resultheader["exp"]);
        jwt.Header.Add("iat", resultheader["iat"]);
        jwt.Header.Add("iss", resultheader["iss"]);
        jwt.Header.Add("aud", resultheader["aud"]);
        
        return jwt;
    }
    
    //keys
    //JWTType, AccountUniqueId, AuthLv, nbf(not before), exp(expiraton), iat(issued at), iss(issuer), aud(audience)
    //jwt타입, 계정유니크id, 계정권한레벨, 효력발휘날짜, 효력만료날짜, 토큰 발급날짜, 토큰 발급자, 토큰 대상자.

    /*
    예시)
    {
        "JWTType": "AccessToken",
        "AccountUniqueId": "1",
        "AuthLv": "1",
        "nbf": 1629809208,
        "exp": 1629812808,
        "iat": 1629809208,
        "iss": "siogames_authManager",
        "aud": "https://api.siogames.com"
    }    
    */
    public static string parsingJWT(JwtSecurityToken jwt, string key)
    {
        object value;
        if (jwt.Payload.TryGetValue(key, out value))
        {
            return value.ToString();
        }
        else
        {
            return string.Empty;
        }
    }
    
    
    //유효기간 체크용 jwt
    public static bool checkValidateJWT(JwtSecurityToken jwt)
    {
        if (jwt == null)
        {
            Debug.LogWarning($"JWTManager.checkValidateJWT jwt == null");
            return false;
        }
            
        if (jwt.Payload == null)
        {
            Debug.LogWarning($"JWTManager.checkValidateJWT jwt.Payload == null");
            return false;
        }
        
        if (jwt.Payload.TryGetValue("exp", out object value))
        {
            long expTime = long.Parse(value.ToString());

            if ((expTime - 300) > getUnixTimeNowSeconds())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    
    static long getUnixTimeNowSeconds() //1 Sec단위.
    {
        var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return (long)timeSpan.TotalSeconds;
    }
    
    static string checkBase64(string inputBase64)
    {
        int checkLength = inputBase64.Length % 4;
        if (checkLength != 0)
        {
            int padding = 4 - checkLength;

            if (padding == 1)
                inputBase64 += "=";
            else if (padding == 2)
                inputBase64 += "==";
        }
        return inputBase64;
    }
}
