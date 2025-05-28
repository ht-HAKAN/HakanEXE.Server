using System;
using System.Management; // System.Management referansı eklenmiş olmalı
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets; // Socket sınıfı için
using System.Text;
using System.Linq; // FirstOrDefault için
// using System.Windows.Forms; // GetActiveWindowName için gerekebilir, şimdilik devre dışı

namespace HakanEXE.Agent.Core
{
    public static class SystemInfoGatherer
    {
        public static Models.ClientInfo GetDetailedSystemInfo() // ClientInfo modelini döndürecek şekilde güncellendi
        {
            PerformanceCounter cpuCounter = null;
            PerformanceCounter ramCounter = null;
            string cpuUsage = "N/A";
            string ramAvailable = "N/A";

            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);

                cpuCounter.NextValue(); // İlk çağrı genellikle 0 döner
                System.Threading.Thread.Sleep(250); // Kısa bir bekleme
                cpuUsage = cpuCounter.NextValue().ToString("F0") + "%";
                ramAvailable = ramCounter.NextValue().ToString("F0") + " MB";
            }
            catch (Exception ex) { Console.WriteLine($"Performans sayacı hatası: {ex.Message}"); }
            finally
            {
                cpuCounter?.Dispose();
                ramCounter?.Dispose();
            }

            return new Models.ClientInfo // Sunucudaki ClientInfo ile aynı olmalı
            {
                AgentId = "AgentID_Burada_Olusturulacak_Veya_AgentClient_Tarafindan_Atanacak", // AgentClient'ta atanıyor
                ComputerName = Environment.MachineName,
                UserName = Environment.UserName,
                IpAddress = GetLocalIPAddress(),
                MacAddress = GetMacAddress(),
                OperatingSystem = Environment.OSVersion.VersionString,
                // LastActive ve IsOnline AgentClient'ta yönetiliyor.
                // Ek bilgiler:
                CpuUsage = cpuUsage,
                RamAvailable = ramAvailable,
                ActiveWindowTitle = GetActiveWindowName() // Şimdilik basit bir implementasyon
            };
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address?.ToString() ?? "127.0.0.1";
                }
            }
            catch
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)?
                    .GetIPProperties().UnicastAddresses
                    .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)?
                    .Address.ToString() ?? "127.0.0.1";
            }
        }

        private static string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
               .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
               .Select(nic => nic.GetPhysicalAddress().ToString())
               .FirstOrDefault(mac => !string.IsNullOrEmpty(mac)) ?? "N/A";
        }

        public static string GetActiveWindowName()
        {
            // Bu kısım P/Invoke (WinAPI) gerektirir ve daha karmaşıktır.
            // Şimdilik basit bir yer tutucu:
            try
            {
                // IntPtr handle = NativeMethods.WinAPI.GetForegroundWindow();
                // int capacity = NativeMethods.WinAPI.GetWindowTextLength(handle) * 2;
                // StringBuilder stringBuilder = new StringBuilder(capacity);
                // NativeMethods.WinAPI.GetWindowText(handle, stringBuilder, stringBuilder.Capacity);
                // return stringBuilder.ToString();
                return "Aktif Pencere (WinAPI Gerekli)";
            }
            catch { return "N/A"; }
        }
    }
}