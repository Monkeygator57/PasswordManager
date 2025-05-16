using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Security
{
    public class PasswordCrypto
    {
        private readonly byte[] _key;

        public PasswordCrypto(byte[] key)
        {
            _key = key;
        }

        public string EncryptPassword(string plainText)
        {
            // Generate a new random IV for each encryption operation
            byte[] iv;
            using (var rng = RandomNumberGenerator.Create())
            {
                iv = new byte[16]; // AES block size is 16 bytes
                rng.GetBytes(iv);
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream ms = new MemoryStream();


                ms.Write(iv, 0, iv.Length); // Prepend IV to the encrypted data

                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public string DecryptPassword(string encryptedText)
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedText);

            // Extract the IV from the beginning of the fullCipher
            byte[] iv = new byte[16];
            byte[] cipherText = new byte[fullCipher.Length - 16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream ms = new MemoryStream(cipherText);
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
