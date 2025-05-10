using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


public class PasswordDecryption
{
    private readonly byte[] AesKey;
    private readonly byte[] AesIV;

    public PasswordDecryption(byte[] key, byte[] iv)
    {
        AesKey = key;
        AesIV = iv;
    }

    public string EncryptString(string plainText)
    {
        using System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create();
        aes.Key = AesKey;
        aes.IV = AesIV;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        using (StreamWriter sw = new(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string DecryptString(string encryptedText)
    {
        using System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create();
        aes.Key = AesKey;
        aes.IV = AesIV;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] buffer = Convert.FromBase64String(encryptedText);

        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        {
            return sr.ReadToEnd();
        }
    }
}