using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent; // Birden fazla iş parçacığı için güvenli koleksiyon
using HakanEXE.Server.Models;
using Newtonsoft.Json; // NuGet: Install-Package Newtonsoft.Json

namespace HakanEXE.Server.Core
{
    public class ServerManager
    {
        private TcpListener _listener;
        private int _port;
        private Thread _listenThread;
        private ConcurrentDictionary<string, ClientHandler> _connectedClients;

        public event EventHandler<ClientInfo> ClientConnected;
        public event EventHandler<ClientInfo> ClientDisconnected;
        public event EventHandler<Packet> PacketReceived;

        public ServerManager(int port)
        {
            _port = port;
            _connectedClients = new ConcurrentDictionary<string, ClientHandler>();
        }

        public void StartListening()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listenThread = new Thread(new ThreadStart(ListenForClients));
            _listenThread.IsBackground = true;
            _listenThread.Start();
            Console.WriteLine($"Sunucu {_port} portunda dinlemeye başladı...");
        }

        private void ListenForClients()
        {
            try
            {
                _listener.Start();
                while (true)
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine($"Yeni bağlantı: {client.Client.RemoteEndPoint}");

                    ClientHandler handler = new ClientHandler(client);
                    handler.PacketReceived += ClientHandler_PacketReceived;
                    handler.ClientDisconnected += ClientHandler_ClientDisconnected;

                    // İlk bağlantıda istemci bilgisi beklenir
                    handler.StartHandlingClient();
                }
            }
            catch (SocketException ex)
            {
                // Listener durdurulduğunda (StopListening çağrıldığında) buraya düşebilir.
                // Bu durumda hata mesajı göstermek yerine sessizce çıkabiliriz.
                if (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    Console.WriteLine("Dinleyici thread'i sonlandırıldı (SocketError.Interrupted).");
                }
                else
                {
                    Console.WriteLine($"Soket hatası (ListenForClients): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sunucu hatası (ListenForClients): {ex.Message}");
            }
        }

        private void ClientHandler_PacketReceived(object sender, Packet packet)
        {
            if (packet == null)
            {
                Console.WriteLine("ClientHandler_PacketReceived: Alınan paket null.");
                return;
            }

            // İlk paket genellikle ClientInfo bilgisi olacaktır
            if (packet.PacketType == PacketType.ClientInfo)
            {
                ClientInfo clientInfo = null;
                try
                {
                    clientInfo = JsonConvert.DeserializeObject<ClientInfo>(packet.Data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ClientInfo deserialize hatası: {ex.Message}. Gelen Data: {packet.Data}");
                    return; // Hatalı ClientInfo paketiyle işlem yapma
                }

                if (clientInfo == null || string.IsNullOrEmpty(clientInfo.IpAddress))
                {
                    Console.WriteLine("ClientHandler_PacketReceived: Deserialize edilen clientInfo veya IpAddress null/boş.");
                    // Belki sender'dan IP alınabilir ama ClientInfo paketi bozuksa client'ı yönetmek zorlaşır.
                    return;
                }

                ClientHandler handler = sender as ClientHandler;
                if (handler == null)
                {
                    Console.WriteLine("ClientHandler_PacketReceived: sender ClientHandler tipinde değil.");
                    return;
                }

                if (_connectedClients.TryAdd(clientInfo.IpAddress, handler))
                {
                    handler.SetClientInfo(clientInfo); // Handler'a ClientInfo'yu ata
                    ClientConnected?.Invoke(this, clientInfo);
                    Console.WriteLine($"Yeni istemci eklendi ve ClientConnected olayı tetiklendi: {clientInfo.ComputerName} ({clientInfo.IpAddress})");
                }
                else
                {
                    // Zaten bağlı, belki bilgi güncellemesi veya bir hata durumu
                    if (_connectedClients.TryGetValue(clientInfo.IpAddress, out ClientHandler existingHandler))
                    {
                        existingHandler.SetClientInfo(clientInfo); // Bilgiyi güncelle
                        Console.WriteLine($"Varolan istemci bilgisi güncellendi: {clientInfo.ComputerName} ({clientInfo.IpAddress})");
                    }
                    else
                    {
                        // Bu durum normalde TryAdd false döndüğünde olmamalı ama bir log bırakalım.
                        Console.WriteLine($"İstemci ({clientInfo.IpAddress}) eklenemedi ama listede de bulunamadı.");
                    }
                }
            }
            PacketReceived?.Invoke(this, packet); // Diğer paket türleri için ana olayı tetikle
        }

        private void ClientHandler_ClientDisconnected(object sender, ClientInfo clientInfo)
        {
            // ---- GÜNCELLENMİŞ KISIM BURADA BAŞLIYOR ----
            ClientHandler handlerToDispose = sender as ClientHandler;

            if (clientInfo == null)
            {
                Console.WriteLine("ClientHandler_ClientDisconnected: clientInfo is null.");
                // clientInfo null ise, sender'ı kullanarak listeden bulmayı deneyebiliriz
                // Bu senaryo, ClientHandler'da _clientInfo set edilemeden bağlantı koparsa olabilir.
                if (handlerToDispose != null)
                {
                    string keyToRemove = null;
                    foreach (var pair in _connectedClients)
                    {
                        if (pair.Value == handlerToDispose)
                        {
                            keyToRemove = pair.Key;
                            break;
                        }
                    }
                    if (keyToRemove != null)
                    {
                        ClientHandler removedHandler;
                        if (_connectedClients.TryRemove(keyToRemove, out removedHandler))
                        {
                            Console.WriteLine($"İstemci (IP: {keyToRemove}, ClientInfo null) listeden kaldırıldı.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("clientInfo null ve handler listede bulunamadı.");
                    }
                }
                // ClientDisconnected olayını null clientInfo ile tetiklemeyelim veya farklı bir olay tanımlayalım.
                // Şimdilik, eğer clientInfo null ise bu olayı tetiklemiyoruz.
            }
            else
            {
                ClientHandler removedHandler;
                if (_connectedClients.TryRemove(clientInfo.IpAddress, out removedHandler))
                {
                    Console.WriteLine($"İstemci ({clientInfo.ComputerName} - {clientInfo.IpAddress}) listeden kaldırıldı.");
                }
                else
                {
                    Console.WriteLine($"İstemci ({clientInfo.ComputerName} - {clientInfo.IpAddress}) listede bulunamadı ama bağlantısı kesildi.");
                }
                ClientDisconnected?.Invoke(this, clientInfo); // clientInfo null değilse bu satır güvenli
            }

            handlerToDispose?.Dispose(); // Kaynakları serbest bırak
            // ---- GÜNCELLENMİŞ KISIM BURADA BİTİYOR ----
        }

        public void SendCommand(string targetIp, PacketType type, string data)
        {
            if (string.IsNullOrEmpty(targetIp))
            {
                Console.WriteLine("SendCommand: targetIp null veya boş.");
                return;
            }
            if (_connectedClients.TryGetValue(targetIp, out ClientHandler handler))
            {
                Packet packet = new Packet
                {
                    PacketType = type,
                    Data = data,
                    SenderIp = GetLocalIPAddress() // Sunucunun IP'si
                };
                handler.SendPacket(packet);
            }
            else
            {
                Console.WriteLine($"Hedef IP ({targetIp}) bağlı değil.");
            }
        }

        public void StopListening()
        {
            Console.WriteLine("Sunucu durduruluyor...");
            try
            {
                if (_listener != null)
                {
                    _listener.Stop(); // Bu, ListenForClients'teki AcceptTcpClient'ta bir SocketException (Interrupted) fırlatır.
                }

                // Bağlı tüm istemcilerin bağlantısını kapat ve kaynaklarını serbest bırak
                foreach (var clientId in _connectedClients.Keys)
                {
                    ClientHandler handler;
                    if (_connectedClients.TryRemove(clientId, out handler))
                    {
                        handler.Dispose();
                    }
                }
                _connectedClients.Clear(); // Koleksiyonu temizle

                // Dinleyici thread'in sonlanmasını bekle (isteğe bağlı, IsBackground=true olduğu için uygulama kapanırken kendi de sonlanır)
                // if (_listenThread != null && _listenThread.IsAlive)
                // {
                //    _listenThread.Join(TimeSpan.FromSeconds(1)); // Kısa bir süre bekle
                //    if(_listenThread.IsAlive) _listenThread.Abort(); // Hala çalışıyorsa sonlandır (riskli)
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StopListening sırasında hata: {ex.Message}");
            }
            finally
            {
                _listenThread = null; // Thread nesnesini null yap
                _listener = null; // Listener nesnesini null yap
                Console.WriteLine("Sunucu durduruldu.");
            }
        }

        private string GetLocalIPAddress()
        {
            // Bu metot, sunucunun spesifik bir ağ arayüzüne bağlı olmadığı (IPAddress.Any) durumlarda
            // dış dünyaya görünecek IP'yi bulmak için kullanılır, ancak her zaman doğru olmayabilir.
            // Yerel testler için genellikle sorun olmaz.
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    // Harici bir adrese bağlanmaya çalışarak hangi yerel IP'nin kullanılacağını öğreniriz.
                    // Bu adresin erişilebilir olması gerekmez.
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address?.ToString() ?? "127.0.0.1";
                }
            }
            catch
            {
                return "127.0.0.1"; // Hata durumunda localhost döndür
            }
        }
    }
}