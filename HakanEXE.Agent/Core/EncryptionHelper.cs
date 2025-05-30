using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HakanEXE.Agent.Core // Namespace'in bu olduğundan emin ol
{
    public static class EncryptionHelper
    {
        // ANAHTAR KESİNLİKLE 32 KARAKTER / 32 BYTE OLMALI
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("BuBirGizliAnahtar123456789012345"); // SONDAKİ '6' YOK, ARTIK 32 KARAKTER
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("BuBirIlklendirme"); // 16 karakter / 16 byte

        public static byte[] Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256; // Anahtar boyutunu açıkça belirtelim (opsiyonel ama iyi bir pratik)
                aesAlg.BlockSize = 128; // Blok boyutunu açıkça belirtelim (opsiyonel)
                aesAlg.Key = Key;       // BU SATIRDA HATA ALIYORDUN
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

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
                aesAlg.KeySize = 256; // Anahtar boyutunu açıkça belirtelim
                aesAlg.BlockSize = 128; // Blok boyutunu açıkça belirtelim
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

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