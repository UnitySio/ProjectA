using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class SecurityPlayerPrefs
{
    #region InLogic

    private static string _saltForKey;

    private static byte[] _keys;
    private static byte[] _iv;
    private static int keySize = 256;
    private static int blockSize = 128;
    private static int _hashLen = 32;

    static SecurityPlayerPrefs()
    {
        // 8 바이트로 하고, 변경해서 쓸것
        var saltBytes = new byte[] { 125, 16, 177, 58, 23, 121, 75, 9 };

        // 길이 상관 없고, 키를 만들기 위한 용도로 씀
        var randomSeedForKey = "5bd456fcb4aaa0!adfga=e649eba45adfs^3sf3506e4dc";

        // 길이 상관 없고, aes에 쓸 key 와 iv 를 만들 용도
        var randomSeedForValue = "2e323d+sa3d772sd578=9df84sdf1v34b5^bb5c706sd7saw";

        {
            var key = new Rfc2898DeriveBytes(randomSeedForKey, saltBytes, 1000);
            _saltForKey = System.Convert.ToBase64String(key.GetBytes(blockSize / 8));
        }

        {
            var key = new Rfc2898DeriveBytes(randomSeedForValue, saltBytes, 1000);
            _keys = key.GetBytes(keySize / 8);
            _iv = key.GetBytes(blockSize / 8);
        }
    }

    private static string MakeHash(string original)
    {
        using (var md5 = new MD5CryptoServiceProvider())
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(original);
            var hashBytes = md5.ComputeHash(bytes);

            var hashToString = "";
            for (int i = 0; i < hashBytes.Length; ++i)
                hashToString += hashBytes[i].ToString("x2");

            return hashToString;
        }
    }

    private static byte[] Encrypt(byte[] bytesToBeEncrypted)
    {
        using (var aes = new RijndaelManaged())
        {
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;

            aes.Key = _keys;
            aes.IV = _iv;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform ct = aes.CreateEncryptor())
            {
                return ct.TransformFinalBlock(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            }
        }
    }

    private static byte[] Decrypt(byte[] bytesToBeDecrypted)
    {
        using (var aes = new RijndaelManaged())
        {
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;

            aes.Key = _keys;
            aes.IV = _iv;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform ct = aes.CreateDecryptor())
            {
                return ct.TransformFinalBlock(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
            }
        }
    }

    private static string Encrypt(string input)
    {
        var bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
        var bytesEncrypted = Encrypt(bytesToBeEncrypted);

        return System.Convert.ToBase64String(bytesEncrypted);
    }

    private static string Decrypt(string input)
    {
        var bytesToBeDecrypted = System.Convert.FromBase64String(input);
        var bytesDecrypted = Decrypt(bytesToBeDecrypted);

        return Encoding.UTF8.GetString(bytesDecrypted);
    }

    private static void SetSecurityValue(string key, string value)
    {
        var hideKey = MakeHash(key + _saltForKey);
        var encryptValue = Encrypt(value + MakeHash(value));

        PlayerPrefs.SetString(hideKey, encryptValue);
    }

    private static string GetSecurityValue(string key)
    {
        var hideKey = MakeHash(key + _saltForKey);

        var encryptValue = PlayerPrefs.GetString(hideKey);
        if (true == string.IsNullOrEmpty(encryptValue))
            return string.Empty;

        var valueAndHash = Decrypt(encryptValue);
        if (_hashLen > valueAndHash.Length)
            return string.Empty;

        var savedValue = valueAndHash.Substring(0, valueAndHash.Length - _hashLen);
        var savedHash = valueAndHash.Substring(valueAndHash.Length - _hashLen);

        if (MakeHash(savedValue) != savedHash)
            return string.Empty;

        return savedValue;
    }

    #endregion

    #region SecurityPlayerPrefsMethods

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(MakeHash(key + _saltForKey));
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void SetInt(string key, int value)
    {
        SetSecurityValue(key, value.ToString());
    }

    public static void SetLong(string key, long value)
    {
        SetSecurityValue(key, value.ToString());
    }

    public static void SetFloat(string key, float value)
    {
        SetSecurityValue(key, value.ToString());
    }

    public static void SetString(string key, string value)
    {
        SetSecurityValue(key, value);
    }

    public static int GetInt(string key, int defaultValue)
    {
        var originalValue = GetSecurityValue(key);
        if (string.IsNullOrEmpty(originalValue))
            return defaultValue;

        var result = defaultValue;
        if (!int.TryParse(originalValue, out result))
            return defaultValue;

        return result;
    }

    public static long GetLong(string key, long defaultValue)
    {
        var originalValue = GetSecurityValue(key);
        if (string.IsNullOrEmpty(originalValue))
            return defaultValue;

        var result = defaultValue;
        if (!long.TryParse(originalValue, out result))
            return defaultValue;

        return result;
    }

    public static float GetFloat(string key, float defaultValue)
    {
        var originalValue = GetSecurityValue(key);
        if (string.IsNullOrEmpty(originalValue))
            return defaultValue;

        var result = defaultValue;
        if (!float.TryParse(originalValue, out result))
            return defaultValue;

        return result;
    }

    public static string GetString(string key, string defaultValue)
    {
        var originalValue = GetSecurityValue(key);
        if (string.IsNullOrEmpty(originalValue))
            return defaultValue;

        return originalValue;
    }

    #endregion
}