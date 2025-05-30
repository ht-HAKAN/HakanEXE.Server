using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HakanEXE.Agent.Core
{
    public static class CommandExecutor
    {
        public static string ExecuteCmdCommand(string command) // BU METODU GÜNCELLİYORUZ
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return "Hata: Boş komut gönderildi.";
            }

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", $"/c {command}") // /c komutu çalıştırıp sonlanır
                {
                    RedirectStandardOutput = true,  // Çıktıyı yakalamak için
                    RedirectStandardError = true,   // Hataları yakalamak için
                    UseShellExecute = false,        // Yeni bir pencere açma
                    CreateNoWindow = true,          // Görünür bir pencere oluşturma
                    StandardOutputEncoding = Encoding.UTF8, // Türkçe karakterler için
                    StandardErrorEncoding = Encoding.UTF8   // Türkçe karakterler için
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    // Çıktıyı ve hatayı oku
                    // WaitForExit'ten önce OutputDataReceived ve ErrorDataReceived event'lerini kullanmak
                    // büyük çıktılarda takılmaları önleyebilir ama basitlik için ReadToEnd kullanıyoruz.
                    // Uzun süren komutlar için timeout eklemek iyi bir pratik olur.
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit(10000); // Maksimum 10 saniye bekle

                    if (!string.IsNullOrEmpty(error))
                    {
                        return $"HATA:\n{error}\nÇIKTI (eğer varsa):\n{output}";
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CMD komutu çalıştırma hatası ({command}): {ex.ToString()}");
                return $"Komut '{command}' çalıştırılırken istisna oluştu: {ex.Message}";
            }
        }

        public static void Shutdown()
        {
            try
            {
                Process.Start("shutdown", "/s /f /t 0");
                Console.WriteLine("Kapatma komutu gönderildi.");
            }
            catch (Exception ex) { Console.WriteLine($"Kapatma hatası: {ex.Message}"); }
        }

        public static void Restart()
        {
            try
            {
                Process.Start("shutdown", "/r /f /t 0");
                Console.WriteLine("Yeniden başlatma komutu gönderildi.");
            }
            catch (Exception ex) { Console.WriteLine($"Yeniden başlatma hatası: {ex.Message}"); }
        }

        public static void OpenNotepad()
        {
            try
            {
                Process.Start("notepad.exe");
                Console.WriteLine("Notepad.exe başarıyla başlatıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notepad açma hatası: {ex.Message}");
            }
        }

        public static string GetFileList(string path)
        {
            Console.WriteLine($"GetFileList çağrıldı: {path} (implementasyon eklenecek).");
            return $"'{path}' için dosya listesi özelliği eklenecek.";
        }

        public static bool DeleteFile(string filePath)
        {
            Console.WriteLine($"DeleteFile çağrıldı: {filePath} (implementasyon eklenecek).");
            return false;
        }
    }
}