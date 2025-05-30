using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HakanEXE.Server.Models; // Server'a proje referansı ŞART!
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

        public AgentClient(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        public void Start()
        {
            Console.WriteLine("Agent başlatılıyor...");
            _myClientInfo = GetInitialClientInfo(); // Bu metot AgentClient içinde tanımlı olmalı

            Task.Run(() => ConnectToServer());
            ListenForDiscoveryRequests(); // Ağda bulunabilirlik için
        }

        private ClientInfo GetInitialClientInfo()
        {
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
                    Console.WriteLine("ClientInfo paketi gönderildi.");

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
                catch (SocketException sex)
                {
                    Console.WriteLine($"Bağlantı hatası (SocketException): {sex.Message}. 5sn sonra tekrar denenecek.");
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Genel bağlantı hatası: {ex.Message}. 5sn sonra tekrar denenecek.");
                    Thread.Sleep(5000);
                }
            }
        }

        private void ReceiveData()
        {
            byte[] headerBuffer = new byte[4];
            byte[] dataBuffer;
            string decryptedData;
            try
            {
                while (_isConnected && _client != null && _client.Connected && _stream != null && _stream.CanRead)
                {
                    int bytesRead = _stream.Read(headerBuffer, 0, headerBuffer.Length);
                    if (bytesRead == 0) { Console.WriteLine("ReceiveData: Stream 0 byte döndü, bağlantı kapandı."); break; }

                    int dataLength = BitConverter.ToInt32(headerBuffer, 0);
                    if (dataLength <= 0) { Console.WriteLine($"ReceiveData: Geçersiz veri uzunluğu: {dataLength}"); continue; }
                    if (dataLength > 10 * 1024 * 1024) { Console.WriteLine($"ReceiveData: Çok büyük veri uzunluğu: {dataLength}. Atlanıyor."); _stream.Flush(); continue; }


                    dataBuffer = new byte[dataLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < dataLength)
                    {
                        bytesRead = _stream.Read(dataBuffer, totalBytesRead, dataLength - totalBytesRead);
                        if (bytesRead == 0) break;
                        totalBytesRead += bytesRead;
                    }
                    if (totalBytesRead < dataLength) { Console.WriteLine("ReceiveData: Paket tam okunamadı."); break; }

                    decryptedData = EncryptionHelper.Decrypt(dataBuffer);
                    Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(decryptedData);

                    if (receivedPacket != null)
                    {
                        HandleCommand(receivedPacket);
                    }
                }
            }
            catch (ObjectDisposedException) { Console.WriteLine("ReceiveData: Nesne (stream/client) dispose edilmiş."); }
            catch (IOException ioEx) { Console.WriteLine($"ReceiveData IOException: {ioEx.Message}"); }
            catch (Exception ex) { Console.WriteLine($"ReceiveData içinde beklenmedik hata: {ex.ToString()}"); }
            finally
            {
                _isConnected = false;
                CleanupNetworkResources();
                Console.WriteLine("ReceiveData sonlandı, yeniden bağlanma tetikleniyor.");
                if (Thread.CurrentThread.IsBackground) Task.Run(() => ConnectToServer());
            }
        }

        private void SendHeartbeat()
        {
            try
            {
                while (true)
                {
                    if (!_isConnected) { Console.WriteLine("SendHeartbeat: Bağlantı yok, thread duruyor."); break; }
                    if (_client != null && _client.Connected && _stream != null && _stream.CanWrite)
                    {
                        SendPacketInternal(new Packet { PacketType = PacketType.Heartbeat, Data = "alive", SenderIp = _myClientInfo?.IpAddress ?? "UNKNOWN_IP_HB" });
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (ThreadAbortException) { Console.WriteLine("Heartbeat thread sonlandırıldı (Abort)."); }
            catch (Exception ex) { Console.WriteLine($"SendHeartbeat içinde hata: {ex.ToString()}"); }
        }

        private void SendPacketInternal(Packet packet)
        {
            if (!_isConnected || _stream == null || !_stream.CanWrite) return;
            try
            {
                string jsonPacket = JsonConvert.SerializeObject(packet);
                byte[] encryptedData = EncryptionHelper.Encrypt(jsonPacket);
                byte[] dataLength = BitConverter.GetBytes(encryptedData.Length);
                lock (_stream)
                {
                    _stream.Write(dataLength, 0, dataLength.Length);
                    _stream.Write(encryptedData, 0, encryptedData.Length);
                    _stream.Flush();
                }
            }
            catch (ObjectDisposedException) { Console.WriteLine("SendPacketInternal: Nesne (stream) dispose edilmiş."); _isConnected = false; }
            catch (IOException ioEx) { Console.WriteLine($"SendPacketInternal IOException: {ioEx.Message}"); _isConnected = false; }
            catch (Exception ex) { Console.WriteLine($"SendPacketInternal içinde hata: {ex.ToString()}"); _isConnected = false; }
        }

        // ---- BU METODU GÜNCELLİYORUZ ----
        private void HandleCommand(Packet commandPacket)
        {
            Console.WriteLine($"Komut Alındı: {commandPacket.PacketType}, Data: '{commandPacket.Data}'");

            switch (commandPacket.PacketType)
            {
                case PacketType.OpenNotepad:
                    Console.WriteLine("OpenNotepad komutu işleniyor...");
                    CommandExecutor.OpenNotepad(); // CommandExecutor.cs'teki metodu çağır
                    break;

                // Diğer komutlar için case'ler buraya eklenecek
                // Örnek:
                // case PacketType.ExecuteCommand:
                //    Console.WriteLine($"ExecuteCommand komutu işleniyor: {commandPacket.Data}");
                //    string output = CommandExecutor.ExecuteCmdCommand(commandPacket.Data);
                //    // Çıktıyı sunucuya geri gönderme mantığı eklenecek
                //    break;

                default:
                    Console.WriteLine($"Bilinmeyen veya henüz işlenmeyen komut: {commandPacket.PacketType}");
                    break;
            }
        }
        // ---- GÜNCELLEME BİTTİ ----

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
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .Select(nic => nic.GetPhysicalAddress()?.ToString())
                    .FirstOrDefault(mac => !string.IsNullOrEmpty(mac)) ?? "N/A_MAC";
            }
            catch { return "N/A_MAC_EX"; }
        }

        private void ListenForDiscoveryRequests()
        {
            Task.Run(async () => {
                try
                {
                    using (var udpClient = new UdpClient(12345))
                    {
                        Console.WriteLine("UDP Discovery dinlemesi başlatıldı (Port 12345).");
                        while (true)
                        {
                            UdpReceiveResult result = await udpClient.ReceiveAsync();
                            if (Encoding.UTF8.GetString(result.Buffer) == "HakanEXE_AGENT_DISCOVERY_REQUEST")
                            {
                                Console.WriteLine($"UDP Discovery isteği alındı: {result.RemoteEndPoint}");
                                byte[] responseBytes = Encoding.UTF8.GetBytes("HakanEXE_AGENT_DISCOVERY_RESPONSE");
                                await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                                Console.WriteLine($"UDP Discovery yanıtı gönderildi: {result.RemoteEndPoint}");
                            }
                        }
                    }
                }
                catch (ObjectDisposedException) { Console.WriteLine("UDP Discovery client dispose edildi."); }
                catch (SocketException se) when (se.SocketErrorCode == SocketError.AddressAlreadyInUse) { Console.WriteLine("UDP Discovery HATA: Port 12345 zaten kullanılıyor."); }
                catch (Exception ex) { Console.WriteLine($"UDP Discovery içinde hata: {ex.ToString()}"); }
            });
        }

        private void CleanupNetworkResources()
        {
            try { _stream?.Close(); _stream = null; } catch { /* Hataları yoksay */ }
            try { _client?.Close(); _client = null; } catch { /* Hataları yoksay */ }
        }

        public void Dispose()
        {
            _isConnected = false;
            CleanupNetworkResources();
            GC.SuppressFinalize(this);
            Console.WriteLine("AgentClient Dispose çağrıldı.");
        }
    }
}