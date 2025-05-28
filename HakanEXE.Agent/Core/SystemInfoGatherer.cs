using System;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace HakanEXE.Agent.Core
{
    public static class SystemInfoGatherer
    {
        // Bu metot artık doğrudan HakanEXE.Server.Models.ClientInfo döndürmeyecek,
        // AgentClient içinde bu bilgiler kullanılarak ClientInfo nesnesi oluşturulacak.
        // Şimdilik temel bilgileri toplayan ayrı metotlar yapalım veya ClientInfo'yu burada oluşturalım
        // ama ek alanlar olmadan.
        // Basitlik adına, ClientInfo nesnesini burada oluşturalım ama ekstradan Cpu, Ram vs. eklemeyelim.
        public static HakanEXE.Server.Models.ClientInfo GetBasicSystemInfo()
        {
            // Not: AgentId ve diğer bazı alanlar AgentClient içinde atanacak.
            // Burada sadece OS, ComputerName, UserName gibi temel bilgileri set edelim.
            return new HakanEXE.Server.Models.ClientInfo
            {
                // AgentId, IpAddress, MacAddress AgentClient.GetInitialClientInfo() içinde set ediliyor.
                // LastActive ve IsOnline da AgentClient içinde yönetiliyor.
                ComputerName = Environment.MachineName,
                UserName = Environment.UserName,
                OperatingSystem = Environment.OSVersion.VersionString,
                // CpuUsage = "N/A", // Şimdilik bunları eklemiyoruz
                // RamAvailable = "N/A",
                // ActiveWindowTitle = "N/A"
            };
        }

        // AgentClient içinde kullanılmak üzere ayrı metotlar da bırakabiliriz.
        public static string GetCpuUsageForAgent()
        {
            PerformanceCounter cpuCounter = null;
            string cpuUsage = "N/A";
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(250);
                cpuUsage = cpuCounter.NextValue().ToString("F0") + "%";
            }
            catch (Exception ex) { Console.WriteLine($"CPU Sayaç Hatası: {ex.Message}"); }
            finally { cpuCounter?.Dispose(); }
            return cpuUsage;
        }

        public static string GetRamAvailableForAgent()
        {
            PerformanceCounter ramCounter = null;
            string ramAvailable = "N/A";
            try
            {
                ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
                ramAvailable = ramCounter.NextValue().ToString("F0") + " MB";
            }
            catch (Exception ex) { Console.WriteLine($"RAM Sayaç Hatası: {ex.Message}"); }
            finally { ramCounter?.Dispose(); }
            return ramAvailable;
        }

        public static string GetActiveWindowNameForAgent()
        {
            // P/Invoke gerektirir, şimdilik basit yer tutucu
            return "Aktif Pencere (WinAPI Gerekli)";
        }

        // GetLocalIPAddress ve GetMacAddress gibi metotlar AgentClient içinde olduğu için
        // SystemInfoGatherer'dan çıkarılabilir veya private static olarak kalabilir.
        // Şimdilik AgentClient'taki versiyonları kullanılacak.
    }
}