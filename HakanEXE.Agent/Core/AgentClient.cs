using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HakanEXE.Server.Models; // Bu satır için Server'a proje referansı ŞART!
using Newtonsoft.Json;
using System.IO;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net;
// System.Management, SystemInfoGatherer içinde kullanılıyor.

namespace HakanEXE.Agent.Core
{
    public class AgentClient : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _serverIp;
        private int _serverPort;
        private bool _isConnected = false;
        private Thread _receiveThread;
        private Thread _heartbeatThread;
        private ClientInfo _myClientInfo;

        public AgentClient(string serverIp, int serverPort) // Program.cs'in çağırdığı constructor
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        public void Start() // Program.cs'in çağırdığı Start metodu
        {
            Console.WriteLine("Agent başlatılıyor...");
            _myClientInfo = GetInitialClientInfo();

            Task.Run(() => ConnectToServer());
            ListenForDiscoveryRequests();
        }

        private ClientInfo GetInitialClientInfo()
        {
            // Şimdilik SystemInfoGatherer'ı çağırmadan temel bilgileri set edelim
            // Hataları en aza indirmek için.
            return new ClientInfo
            {
                AgentId = Guid.NewGuid().ToString(),
                ComputerName = Environment.MachineName,
                UserName = Environment.UserName,
                IpAddress = GetLocalIPAddress(),
                MacAddress = GetMacAddress(),
                OperatingSystem = Environment.OSVersion.VersionString,
                LastActive = DateTime.Now,
                IsOnline = true
            };
        }

        private void ConnectToServer()
        {
            while (!_isConnected)
            {
                try
                {
                    _client = new TcpClient();
                    Console.WriteLine($"Sunucuya bağlanılıyor: {_serverIp}:{_serverPort}");
                    _client.Connect(_serverIp, _serverPort);
                    _stream = _client.GetStream();
                    _isConnected = true;
                    Console.WriteLine("Sunucuya bağlandı.");

                    SendPacketInternal(new Packet { PacketType = PacketType.ClientInfo, Data = JsonConvert.SerializeObject(_myClientInfo), SenderIp = _myClientInfo.IpAddress });

                    _receiveThread = new Thread(ReceiveData);
                    _receiveThread.IsBackground = true;
                    _receiveThread.Start();

                    if (_heartbeatThread == null || !_heartbeatThread.IsAlive)
                    {
                        _heartbeatThread = new Thread(SendHeartbeat);
                        _heartbeatThread.IsBackground = true;
                        _heartbeatThread.Start();
                    }
                }
                catch (SocketException) { Thread.Sleep(5000); } // Kısaca geçelim
                catch (Exception) { Thread.Sleep(5000); }  // Kısaca geçelim
            }
        }

        private void ReceiveData()
        {
            byte[] headerBuffer = new byte[4];
            try
            {
                while (_isConnected && _client != null && _client.Connected && _stream != null && _stream.CanRead)
                {
                    int bytesRead = _stream.Read(headerBuffer, 0, headerBuffer.Length);
                    if (bytesRead == 0) break; // Bağlantı kapandı

                    int dataLength = BitConverter.ToInt32(headerBuffer, 0);
                    if (dataLength <= 0) continue;

                    byte[] dataBuffer = new byte[dataLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < dataLength)
                    {
                        bytesRead = _stream.Read(dataBuffer, totalBytesRead, dataLength - totalBytesRead);
                        if (bytesRead == 0) break;
                        totalBytesRead += bytesRead;
                    }
                    if (totalBytesRead < dataLength) break; // Tam paket okunamadı

                    string decryptedData = EncryptionHelper.Decrypt(dataBuffer); // EncryptionHelper.cs doğru olmalı
                    Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(decryptedData);
                    if (receivedPacket != null) HandleCommand(receivedPacket);
                }
            }
            catch { /* Şimdilik hataları yoksayalım, bağlantı kopunca finally çalışsın */ }
            finally
            {
                _isConnected = false;
                CleanupNetworkResources();
                if (Thread.CurrentThread.IsBackground) Task.Run(() => ConnectToServer());
            }
        }

        private void SendHeartbeat()
        {
            try
            {
                while (true)
                {
                    if (!_isConnected) break;
                    if (_client != null && _client.Connected && _stream != null && _stream.CanWrite)
                    {
                        SendPacketInternal(new Packet { PacketType = PacketType.Heartbeat, Data = "alive", SenderIp = _myClientInfo.IpAddress });
                    }
                    Thread.Sleep(5000);
                }
            }
            catch { /* Hataları yoksayalım */ }
        }

        private void SendPacketInternal(Packet packet)
        {
            if (!_isConnected || _stream == null || !_stream.CanWrite) return;
            try
            {
                string jsonPacket = JsonConvert.SerializeObject(packet);
                byte[] encryptedData = EncryptionHelper.Encrypt(jsonPacket); // EncryptionHelper.cs doğru olmalı
                byte[] dataLength = BitConverter.GetBytes(encryptedData.Length);
                lock (_stream)
                {
                    _stream.Write(dataLength, 0, dataLength.Length);
                    _stream.Write(encryptedData, 0, encryptedData.Length);
                    _stream.Flush();
                }
            }
            catch { _isConnected = false; } // Hata durumunda bağlantıyı kopmuş say
        }

        private void HandleCommand(Packet commandPacket)
        {
            // Şimdilik burayı boş bırakalım, sadece temel bağlantıyı test ediyoruz.
            // Komut işleme mantığı daha sonra eklenecek.
            // Console.WriteLine($"Komut Alındı: {commandPacket.PacketType}");
        }

        private string GetLocalIPAddress()
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
            catch { return "127.0.0.1"; }
        }

        private string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Select(nic => nic.GetPhysicalAddress()?.ToString())
                .FirstOrDefault(mac => !string.IsNullOrEmpty(mac)) ?? "N/A";
        }

        private void ListenForDiscoveryRequests()
        {
            Task.Run(async () => {
                try
                {
                    using (var udpClient = new UdpClient(12345))
                    {
                        while (true)
                        {
                            UdpReceiveResult result = await udpClient.ReceiveAsync();
                            if (Encoding.UTF8.GetString(result.Buffer) == "HakanEXE_AGENT_DISCOVERY_REQUEST")
                            {
                                byte[] responseBytes = Encoding.UTF8.GetBytes("HakanEXE_AGENT_DISCOVERY_RESPONSE");
                                await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                            }
                        }
                    }
                }
                catch { /* UDP hatalarını şimdilik yoksayalım */ }
            });
        }

        private void CleanupNetworkResources()
        {
            try { _stream?.Close(); _stream = null; } catch { }
            try { _client?.Close(); _client = null; } catch { }
        }

        public void Dispose()
        {
            _isConnected = false;
            CleanupNetworkResources();
            GC.SuppressFinalize(this);
        }
    }
}