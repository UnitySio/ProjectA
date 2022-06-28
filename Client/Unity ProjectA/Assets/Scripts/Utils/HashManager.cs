using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class HashManager
{
    public static string HashPassword(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;
        
        using (var sha512 = new SHA512CryptoServiceProvider())
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            
            var hashBytes = sha512.ComputeHash(bytes);

            var hashToString = "";
            for (int i = 0; i < hashBytes.Length; ++i)
                hashToString += hashBytes[i].ToString("x2");

            return hashToString;
        }
    }
}
