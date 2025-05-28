using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HakanEXE.Server.Core
{
    public static class EncryptionHelper
    {
        // Güvenli bir anahtar ve IV oluşturmanız önemlidir.
        // Gerçek uygulamada bunları kod içine sabit olarak gömmeyin, güvenli bir şekilde saklayın.
        // Örnek amaçlı sabit değerler kullanılıyor.
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("BuBirGizliAnahtar1234567890123456"); // 32 byte = 256 bit
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("BuBirIlklendirme"); // 16 byte = 128 bit

        public static byte[] Encrypt(string plainText)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC; // Varsayılan değer

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        public static string Decrypt(byte[] cipherText)
        {
            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}