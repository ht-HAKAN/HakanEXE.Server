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
                Console.WriteLine($"Soket hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sunucu hatası: {ex.Message}");
            }
        }

        private void ClientHandler_PacketReceived(object sender, Packet packet)
        {
            // İlk paket genellikle ClientInfo bilgisi olacaktır
            if (packet.PacketType == PacketType.ClientInfo)
            {
                ClientInfo clientInfo = JsonConvert.DeserializeObject<ClientInfo>(packet.Data);
                if (_connectedClients.TryAdd(clientInfo.IpAddress, (ClientHandler)sender))
                {
                    ((ClientHandler)sender).SetClientInfo(clientInfo); // Handler'a ClientInfo'yu ata
                    ClientConnected?.Invoke(this, clientInfo);
                }
                else
                {
                    // Zaten bağlı, belki bilgi güncellemesi
                    // Mevcut clientInfo'yu güncelle
                    ClientHandler existingHandler;
                    if (_connectedClients.TryGetValue(clientInfo.IpAddress, out existingHandler))
                    {
                        existingHandler.SetClientInfo(clientInfo);
                    }
                }
            }
            PacketReceived?.Invoke(this, packet);
        }

        private void ClientHandler_ClientDisconnected(object sender, ClientInfo clientInfo)
        {
            ClientHandler removedHandler;
            _connectedClients.TryRemove(clientInfo.IpAddress, out removedHandler);
            ClientDisconnected?.Invoke(this, clientInfo);
            ((ClientHandler)sender).Dispose(); // Kaynakları serbest bırak
        }

        public void SendCommand(string targetIp, PacketType type, string data)
        {
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
            if (_listener != null)
            {
                _listener.Stop();
            }
            if (_listenThread != null && _listenThread.IsAlive)
            {
                _listenThread.Abort(); // Dikkat: Abort zararlı olabilir, düzgün kapatma mekanizması daha iyi
            }
            foreach (var handler in _connectedClients.Values)
            {
                handler.Dispose();
            }
            _connectedClients.Clear();
            Console.WriteLine("Sunucu durduruldu.");
        }

        private string GetLocalIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530); // Google DNS'e bağlanarak yerel IP'yi al
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }
    }
}