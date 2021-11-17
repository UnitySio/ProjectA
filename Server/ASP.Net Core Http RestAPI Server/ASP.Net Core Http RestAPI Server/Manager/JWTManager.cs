using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;


namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class JWTManager
    {
        //JsonWebToken 검증용 비밀 서명 키 (서버에만 보관)
        static string plainTextKey = "s54fdfsd5f56!4df3ef54s=f6ds!f456s4f65sd4!f65s4df564e53s4f56!=";
        //액세스 토큰 유효기간 (minute)
        static int AccessTokenLifetimeMinute = 60;
        //리프레시 토큰 유효기간 (day)
        static int RefreshTokenLifetimeDay = 15;

        //JsonWebToken 발급자명 (도메인)
        private static string IssuerStr = "siogames_authManager";
        //JsonWebToken 대상자명 (도메인)
        private static string AudienceStr = "https://api.siogames.com";
        //JsonWebToken 서명 키 기반 Credential 객체
        private static SigningCredentials jwt_signingCredentials;
        //JsonWebToken 유효성 체크용 파라메터
        static TokenValidationParameters tokenValidationParam;

        public static void Initialize()
        {
            jwt_signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(plainTextKey)),
                SecurityAlgorithms.HmacSha256Signature,
                SecurityAlgorithms.Sha256Digest
                );

            tokenValidationParam = new TokenValidationParameters()
            {
                RequireAudience = true,
                RequireSignedTokens = true,
                RequireExpirationTime = true,

                ValidateLifetime = true,

                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,

                ValidAudience = AudienceStr,
                ValidIssuer = IssuerStr,
                IssuerSigningKey = jwt_signingCredentials.Key,

                ClockSkew = TimeSpan.Zero
            };
        }



        #region JWT_Logic
        
        public static string createNewJWT(UserData user, JWTType type)
        {
            if (user == null || type == null)
                throw new NullReferenceException("createNewJWT()에는 null 인자값이 들어올 수 없습니다.");

            DateTime utcNow = DateTime.UtcNow;

            var securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new List<Claim>()
                {
                    new Claim("JWTType", type.ToString()),
                    new Claim("AccountUniqueId", user.AccountUniqueId.ToString()),
                    new Claim("AuthLv", user.AuthLv.ToString())
                }),
                Audience = AudienceStr, //대상자
                Issuer = IssuerStr, //발급자
                IssuedAt = utcNow, //발급일시
                NotBefore = utcNow, //토큰 효력발휘 시작 일시 
                SigningCredentials = jwt_signingCredentials, //토큰 암호화 알고리즘
            };

            //타입에 따라 토큰 효력만료일시 지정.
            switch (type)
            {
                case JWTType.AccessToken:
                    securityTokenDescriptor.Expires = utcNow.AddMinutes(AccessTokenLifetimeMinute);
                    break;
                case JWTType.RefreshToken:
                    securityTokenDescriptor.Expires = utcNow.AddDays(RefreshTokenLifetimeDay);
                    break;
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
            string signedAndEncodedToken = tokenHandler.WriteToken(plainToken);

            return signedAndEncodedToken;
        }


        public static bool checkValidationJWT(string tokenStr, out SecurityToken tokenInfo)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(tokenStr, tokenValidationParam, out tokenInfo);

                /*JwtSecurityToken jwt = tokenInfo as JwtSecurityToken;

                object AccountUniqueId, AuthLv;

                if (jwt.Payload.TryGetValue("AccountUniqueId", out AccountUniqueId))
                {
                    Console.WriteLine($"AccountUniqueId = \n{AccountUniqueId.ToString()}");
                }

                if (jwt.Payload.TryGetValue("AuthLv", out AuthLv))
                {
                    Console.WriteLine($"AuthLv = \n{AuthLv.ToString()}");
                }*/

                return true;
            }
            catch (Exception)
            {
                tokenInfo = null;
                return false;
            }
            finally
            {
                tokenHandler = null;
            }
        }

        public enum JWTType
        {
            AccessToken,
            RefreshToken
        }

        #endregion
    }
}
