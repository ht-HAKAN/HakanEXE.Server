using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HakanEXE.Agent.Core
{
    public static class CommandExecutor
    {
        public static string ExecuteCmdCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8, // Türkçe karakterler için
                    StandardErrorEncoding = Encoding.UTF8   // Türkçe karakterler için
                };

                using (Process process = Process.Start(psi))
                {
                    if (process == null) return "İşlem başlatılamadı.";

                    StringBuilder output = new StringBuilder();
                    // Zaman aşımı ekleyerek sonsuz döngüleri veya çok uzun süren komutları yönet
                    if (!process.WaitForExit(15000)) // 15 saniye zaman aşımı
                    {
                        try { process.Kill(); } catch { /* Kill başarısız olursa görmezden gel */ }
                        output.AppendLine("Komut zaman aşımına uğradı.");
                    }
                    else
                    {
                        output.Append(process.StandardOutput.ReadToEnd());
                        output.Append(process.StandardError.ReadToEnd());
                    }
                    return output.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Komut çalıştırma hatası: {ex.Message}";
            }
        }

        public static void Shutdown()
        {
            try { Process.Start("shutdown", "/s /f /t 0"); }
            catch (Exception ex) { Console.WriteLine("Kapatma hatası: " + ex.Message); }
        }

        public static void Restart()
        {
            try { Process.Start("shutdown", "/r /f /t 0"); }
            catch (Exception ex) { Console.WriteLine("Yeniden başlatma hatası: " + ex.Message); }
        }

        public static void OpenNotepad()
        {
            try { Process.Start("notepad.exe"); }
            catch (Exception ex) { Console.WriteLine("Notepad açma hatası: " + ex.Message); }
        }

        public static string GetFileList(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    return "Hata: Belirtilen dizin bulunamadı veya geçersiz.";
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Dizin: {path}");

                foreach (string dir in Directory.GetDirectories(path))
                {
                    sb.AppendLine($"<DIR>\t{Path.GetFileName(dir)}");
                }
                foreach (string file in Directory.GetFiles(path))
                {
                    FileInfo fi = new FileInfo(file);
                    sb.AppendLine($"<FILE>\t{Path.GetFileName(file)}\t{fi.Length} bytes");
                }
                return sb.ToString();
            }
            catch (UnauthorizedAccessException)
            {
                return $"Hata: '{path}' dizinine erişim yetkiniz yok.";
            }
            catch (Exception ex)
            {
                return $"Dosya listesi alınamadı: {ex.Message}";
            }
        }

        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                else if (Directory.Exists(filePath)) // Klasör silme için ayrı bir komut daha iyi olurdu ama basitçe ekleyelim
                {
                    Directory.Delete(filePath, true); // true ile içindekileri de siler
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silme hatası ({filePath}): {ex.Message}");
                return false;
            }
        }
    }
}