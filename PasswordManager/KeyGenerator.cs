using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.Xml;

namespace PasswordManager.Security
{
    internal class KeyGenerator
    {
        private const int SaltSize = 16; // Size of the salt in bytes, 128 bits
        private const int KeySize = 32; // Size of the key in bytes, 256 bits
        private const int IvSize = 16; // Size of the IV in bytes, 128 bits
        private const int Iterations = 100000; // Number of iterations for PBKDF2, 100000 is a common choice

        private readonly string _keyStorePath;

        public KeyGenerator(string applicationDataPath)
        {
            _keyStorePath = Path.Combine(applicationDataPath, "keystore.bin");
            Directory.CreateDirectory(applicationDataPath);
        }

        public bool InitializeKeyStore(string masterPassword)
        {
            try
            {
                // Generate a random salt
                byte[] salt = GenerateRandomBytes(SaltSize);

                // Derive keys from the master password
                (byte[] key, byte[] iv) = DeriveKeyAndIv(masterPassword, salt);

                // Create a test value to verify master password later
                byte[] testValue = Encoding.UTF8.GetBytes("VERIFY_PASSWORD_MANAGER");
                byte[] encryptedTest = EncryptData(testValue, key, iv);

                // Store salt and encrypted test value in the keystore
                using (var fs = new FileStream(_keyStorePath, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(salt.Length);
                    bw.Write(salt);
                    bw.Write(iv.Length);
                    bw.Write(iv);
                    bw.Write(encryptedTest.Length);
                    bw.Write(encryptedTest);
                }

                return true;
            }

            catch(Exception ex)
            {
                Console.WriteLine($"Error initializing keystore: {ex.Message}");
                return false;
            }
        }

        // Method to verify the master password
        public (bool Success, byte[] Key, byte[] IV) GetDerivedKey(string MasterPassword)
        {
            try
            {

                if (!File.Exists(_keyStorePath))
                {
                    return (false, null, null);
                }

                byte[] salt;
                byte[] storedIv;
                byte[] encryptedTest;

                // Read the salt and encrypted test value from the keystore
                using (var fs = new FileStream(_keyStorePath, FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    int saltLength = br.ReadInt32();
                    salt = br.ReadBytes(saltLength);
                    int ivLength = br.ReadInt32();
                    storedIv = br.ReadBytes(ivLength);
                    int encryptedTestLength = br.ReadInt32();
                    encryptedTest = br.ReadBytes(encryptedTestLength);
                }

                // Derive the key and IV from the master password
                (byte[] key, byte[] iv) = DeriveKeyAndIv(MasterPassword, salt);

                // Decrypt the test value
                try
                {
                    byte[] decryptedTest = DecryptData(encryptedTest, key, storedIv);
                    string testString = Encoding.UTF8.GetString(decryptedTest);

                    if (testString == "VERIFY_PASSWORD_MANAGER")
                    {
                        return (true, key, storedIv);
                    }
                }

                catch (CryptographicException)
                {
                    // Handle decryption failure
                }

                return (false, null, null);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error getting derived key: {ex.Message}");
                return (false, null, null);
            }
        }

        // Check to see if master password exists
        public bool IsKeyStoreInitialized()
        {
            return File.Exists(_keyStorePath);
        }

        // Generate a new IV for each encryption operation
        public byte[] GenerateNewIV()
        {
            return GenerateRandomBytes(IvSize);
        }

        // Helper method to derive a key and IV from the master password and salt
        public (byte[] Key, byte[] IV) DeriveKeyAndIv(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(KeySize);
                byte[] iv = deriveBytes.GetBytes(IvSize);
                return (key, iv);
            }
        }

        // Helper method to derive key and IV from the master password
        private byte[] GenerateRandomBytes(int size)
        {
            byte[] bytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        // Encrypt data using AES
        private byte[] EncryptData(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())

            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        // Decrypt data using AES
        private byte[] DecryptData(byte[] encryptedData, byte[] key, byte[] iv)
        {

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, 0, encryptedData.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }
    }
}
