using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HakanEXE.Server.Core
{
    public class NetworkScanner
    {
        private const int DiscoveryPort = 12345; // Agent'lar bu porttan kendini duyuracak veya dinleyecek
        private const string DiscoveryMessage = "HakanEXE_AGENT_DISCOVERY_REQUEST"; // Sunucunun gönderdiği istek mesajı
        private const string DiscoveryResponse = "HakanEXE_AGENT_DISCOVERY_RESPONSE"; // Agent'ın yanıt mesajı

        public event EventHandler<IPEndPoint> AgentDiscovered;

        public NetworkScanner()
        {
            // Agent'lardan gelen yanıtları dinlemek için bir UDP dinleyicisi başlat
            Task.Run(() => ListenForAgentResponses());
        }

        public async Task ScanNetworkAsync()
        {
            Console.WriteLine("Ağ taraması başlatılıyor...");
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort)); // Kendi portumuzu bağla

                // Broadcast adresine istek gönder
                IPEndPoint broadcastIp = new IPEndPoint(IPAddress.Parse("255.255.255.255"), DiscoveryPort);
                byte[] sendBytes = Encoding.UTF8.GetBytes(DiscoveryMessage);

                try
                {
                    // Ağdaki tüm cihazlara discovery isteği gönder
                    await udpClient.SendAsync(sendBytes, sendBytes.Length, broadcastIp);
                    Console.WriteLine($"Discovery isteği {broadcastIp} adresine gönderildi.");

                    // Kısa bir süre yanıtları bekle
                    // Yanıtlar ayrı bir thread'de (ListenForAgentResponses) dinleneceği için burada uzun bekleme yapmaya gerek yok.
                    await Task.Delay(2000); // 2 saniye bekle
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Ağ tarama hatası (Socket): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ağ tarama hatası: {ex.Message}");
                }
            }
            Console.WriteLine("Ağ taraması tamamlandı (istek gönderildi).");
        }

        private void ListenForAgentResponses()
        {
            using (UdpClient udpClient = new UdpClient(DiscoveryPort)) // Aynı porttan hem gönderip hem dinleyeceğiz
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Console.WriteLine($"Agent yanıtları için {DiscoveryPort} portu dinleniyor...");

                try
                {
                    while (true)
                    {
                        byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint); // Bloklar
                        string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

                        if (receivedMessage == DiscoveryResponse)
                        {
                            Console.WriteLine($"Agent yanıtı alındı: {remoteEndPoint.Address}");
                            AgentDiscovered?.Invoke(this, remoteEndPoint);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"UDP dinleme hatası (Socket): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UDP dinleme hatası: {ex.Message}");
                }
            }
        }
    }
}