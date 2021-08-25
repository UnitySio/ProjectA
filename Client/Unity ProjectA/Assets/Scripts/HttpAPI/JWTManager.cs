using System;
using System.IdentityModel.Tokens.Jwt;
using UnityEngine;

public class JWTManager : MonoBehaviour
{
    public static JwtSecurityToken DecryptJWT(string jsonWebTokenStr)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            JwtSecurityToken jwt = tokenHandler.ReadJwtToken(jsonWebTokenStr);
            return jwt;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            tokenHandler = null;
        }
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
        object value;
        if (jwt.Payload.TryGetValue("exp", out value))
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
}
