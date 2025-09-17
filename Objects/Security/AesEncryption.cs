using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.Security
{
    public static class AesEncryption
    {
        public static string EncryptSimple(string plainText, string key16Characters)
        {
            if (key16Characters.Length != 16)
            {
                throw new ArgumentException("Key must be 16 characters long", nameof(key16Characters));
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key16Characters);
                aes.IV = new byte[16]; // Zero IV

                using (MemoryStream memoryStream = new MemoryStream())
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public static string DecryptSimple(string cipherTextInBase64, string key16Characters)
        {
            if (key16Characters.Length != 16)
            {
                throw new ArgumentException("Key must be 16 characters long", nameof(key16Characters));
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key16Characters);
                aes.IV = new byte[16]; // Zero IV

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipherTextInBase64)))
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }


        public static string Encrypt2(string plainText, string key, out string iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            using (Aes aes = Aes.Create())
            {
                // Set key and generate a new IV
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key.PadRight(32, '0').Substring(0, 32));
                aes.GenerateIV();

                // Convert IV to hexadecimal string
                iv = BitConverter.ToString(aes.IV).Replace("-", "");

                // Encrypt the string to a byte array
                byte[] encrypted;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        encrypted = memoryStream.ToArray();
                    }
                }

                // Convert encrypted data to Base64 string
                return Convert.ToBase64String(encrypted);
            }
        }

        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (key.Length != 16)
                throw new ArgumentException("Key length must be 16 characters");

            byte[] iv;
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Prepend the IV
                    memoryStream.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string Encrypt4(string stringToEncrypt, byte[] Key, byte[] IV)
        {
            byte[] Results = null;
            try
            {
                using (Aes AESProvider = Aes.Create())
                {
                    UTF8Encoding UTF8 = new UTF8Encoding();
                    AESProvider.Key = Key;
                    AESProvider.IV = IV;
                    AESProvider.Padding = PaddingMode.Zeros;
                    byte[] DataToEncryptAES = UTF8.GetBytes(stringToEncrypt);
                    ICryptoTransform Encryptor = AESProvider.CreateEncryptor(AESProvider.Key, AESProvider.IV);
                    Results = Encryptor.TransformFinalBlock(DataToEncryptAES, 0, DataToEncryptAES.Length);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Convert.ToBase64String(Results);
        }

        public static string Decrypt4(string stringToDecrypt, byte[] Key, byte[] IV)
        {
            byte[] Results = null;
            UTF8Encoding UTF8 = new UTF8Encoding();
            try
            {
                using (Aes AESProvider = Aes.Create())
                {
                    AESProvider.Key = Key;
                    AESProvider.IV = IV;
                    byte[] DataToDecryptAES = Convert.FromBase64String(stringToDecrypt);
                    ICryptoTransform Decryptor = AESProvider.CreateDecryptor(AESProvider.Key, AESProvider.IV);
                    Results = Decryptor.TransformFinalBlock(DataToDecryptAES, 0, DataToDecryptAES.Length);
                }
            }
            catch (Exception ex)
            {
                throw new System.Security.Cryptography.CryptographicException();
            }
            return System.Text.Encoding.UTF8.GetString(Results);
        }

        public static string EncryptPrependIV(string stringToEncrypt, byte[] Key)
        {
            byte[] Results = null;
            try
            {
                using (Aes AESProvider = Aes.Create())
                {
                    AESProvider.GenerateIV();
                    byte[] IV = AESProvider.IV;
                    UTF8Encoding UTF8 = new UTF8Encoding();
                    AESProvider.Key = Key;
                    AESProvider.Padding = PaddingMode.PKCS7;
                    byte[] DataToEncryptAES = UTF8.GetBytes(stringToEncrypt);
                    ICryptoTransform Encryptor = AESProvider.CreateEncryptor(AESProvider.Key, AESProvider.IV);
                    byte[] cipherText = Encryptor.TransformFinalBlock(DataToEncryptAES, 0, DataToEncryptAES.Length);
                    Results = new byte[AESProvider.IV.Length + cipherText.Length];
                    Buffer.BlockCopy(AESProvider.IV, 0, Results, 0, AESProvider.IV.Length);
                    Buffer.BlockCopy(cipherText, 0, Results, AESProvider.IV.Length, cipherText.Length);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Convert.ToBase64String(Results);
        }

        public static string DecryptWithPrependedIV(string stringToDecrypt, byte[] Key)
        {
            byte[] Results = null;
            UTF8Encoding UTF8 = new UTF8Encoding();
            try
            {
                using (Aes AESProvider = Aes.Create())
                {
                    byte[] DataToDecryptAES = Convert.FromBase64String(stringToDecrypt);
                    byte[] IV = new byte[AESProvider.IV.Length];
                    Buffer.BlockCopy(DataToDecryptAES, 0, IV, 0, IV.Length);
                    AESProvider.Key = Key;
                    AESProvider.IV = IV;
                    AESProvider.Padding = PaddingMode.PKCS7;
                    ICryptoTransform Decryptor = AESProvider.CreateDecryptor(AESProvider.Key, AESProvider.IV);
                    byte[] cipherText = new byte[DataToDecryptAES.Length - IV.Length];
                    Buffer.BlockCopy(DataToDecryptAES, IV.Length, cipherText, 0, cipherText.Length);
                    Results = Decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
            }
            catch (Exception ex)
            {
                throw new System.Security.Cryptography.CryptographicException();
            }
            return System.Text.Encoding.UTF8.GetString(Results);
        }

    }
}

