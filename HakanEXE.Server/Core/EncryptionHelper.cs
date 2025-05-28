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
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7; // Padding modu açıkça belirtildi

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
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));

            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7; // Padding modu açıkça belirtildi

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