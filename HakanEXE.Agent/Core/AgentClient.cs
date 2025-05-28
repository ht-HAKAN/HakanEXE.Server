using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HakanEXE.Server.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net;

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
            try
            {
                _myClientInfo = GetInitialClientInfo();
                if (_myClientInfo == null)
                {
                    Console.WriteLine("HATA: _myClientInfo başlatılamadı (GetInitialClientInfo null döndü). Agent durduruluyor.");
                    return; // _myClientInfo null ise devam etme
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA: GetInitialClientInfo sırasında bir istisna oluştu: {ex.Message}. Agent durduruluyor.");
                return; // İstisna durumunda devam etme
            }

            Task.Run(() => ConnectToServer());
            ListenForDiscoveryRequests();
        }

        private ClientInfo GetInitialClientInfo()
        {
            try
            {
                Console.WriteLine("GetInitialClientInfo() çağrıldı.");
                ClientInfo info = new ClientInfo
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
                Console.WriteLine($"ClientInfo oluşturuldu: ID={info.AgentId}, IP={info.IpAddress}");
                return info;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetInitialClientInfo içinde HATA: {ex.ToString()}");
                return null; // Hata durumunda null döndür, Start() metodu bunu yakalasın
            }
        }

        private void ConnectToServer()
        {
            Console.WriteLine("ConnectToServer() thread'i başlatıldı.");
            if (_myClientInfo == null)
            {
                Console.WriteLine("HATA: ConnectToServer içinde _myClientInfo null. Bu durum beklenmiyor. Thread sonlandırılıyor.");
                return;
            }

            while (!_isConnected)
            {
                try
                {
                    _client = new TcpClient();
                    Console.WriteLine($"Sunucuya bağlanılıyor: {_serverIp}:{_serverPort}");
                    _client.Connect(_serverIp, _serverPort);
                    _stream = _client.GetStream();
                    _isConnected = true;
                    Console.WriteLine("Sunucuya bağlandı."); // BURASI ÖNCEKİ KODDA YAKLAŞIK 68. SATIRDI

                    // ClientInfo paketini göndermeden önce _myClientInfo'nun null olmadığını tekrar kontrol edelim (paranoya için)
                    if (_myClientInfo == null)
                    {
                        Console.WriteLine("KRİTİK HATA: _myClientInfo, ClientInfo paketi gönderilmeden hemen önce null oldu!");
                        // Bağlantıyı sonlandırabilir veya hata yönetimi yapabiliriz.
                        _isConnected = false; // Döngüden çıkmayı tetikle
                        CleanupNetworkResources();
                        continue; // veya return;
                    }
                    string clientInfoJson = JsonConvert.SerializeObject(_myClientInfo);
                    string clientIpForPacket = _myClientInfo.IpAddress; // NullReferenceException burada olabilir eğer _myClientInfo null ise

                    SendPacketInternal(new Packet { PacketType = PacketType.ClientInfo, Data = clientInfoJson, SenderIp = clientIpForPacket });
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
                catch (NullReferenceException nre)
                {
                    Console.WriteLine($"ConnectToServer içinde NullReferenceException: {nre.ToString()}");
                    Thread.Sleep(5000);
                }
                catch (SocketException sex)
                {
                    Console.WriteLine($"ConnectToServer içinde SocketException: {sex.Message}");
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ConnectToServer içinde genel Exception: {ex.ToString()}");
                    Thread.Sleep(5000);
                }
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
                    if (bytesRead == 0) { Console.WriteLine("ReceiveData: Stream 0 byte döndü, bağlantı kapandı varsayılıyor."); break; }

                    int dataLength = BitConverter.ToInt32(headerBuffer, 0);
                    if (dataLength <= 0) { Console.WriteLine($"ReceiveData: Geçersiz veri uzunluğu alındı: {dataLength}"); continue; }
                    if (dataLength > 10 * 1024 * 1024) { Console.WriteLine($"ReceiveData: Çok büyük veri uzunluğu: {dataLength}. Paket atlanıyor."); _stream.Flush(); continue; } // Güvenlik önlemi

                    byte[] dataBuffer = new byte[dataLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < dataLength)
                    {
                        bytesRead = _stream.Read(dataBuffer, totalBytesRead, dataLength - totalBytesRead);
                        if (bytesRead == 0) break;
                        totalBytesRead += bytesRead;
                    }
                    if (totalBytesRead < dataLength) { Console.WriteLine("ReceiveData: Paket tam okunamadı."); break; }

                    string decryptedData = EncryptionHelper.Decrypt(dataBuffer);
                    Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(decryptedData);
                    if (receivedPacket != null) HandleCommand(receivedPacket);
                }
            }
            catch (ObjectDisposedException) { Console.WriteLine("ReceiveData: Stream veya Client dispose edilmiş."); }
            catch (IOException ioEx) { Console.WriteLine($"ReceiveData IOException: {ioEx.Message}"); }
            catch (Exception ex) { Console.WriteLine($"ReceiveData'da HATA: {ex.ToString()}"); }
            finally
            {
                Console.WriteLine("ReceiveData finally bloğu çalıştı.");
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
                    if (!_isConnected) { Console.WriteLine("SendHeartbeat: Bağlantı yok, thread durduruluyor."); break; }
                    if (_client != null && _client.Connected && _stream != null && _stream.CanWrite)
                    {
                        //Console.WriteLine("Heartbeat gönderiliyor...");
                        SendPacketInternal(new Packet { PacketType = PacketType.Heartbeat, Data = "alive", SenderIp = _myClientInfo?.IpAddress ?? "UNKNOWN_IP" });
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (ThreadAbortException) { Console.WriteLine("Heartbeat thread'i sonlandırıldı."); }
            catch (Exception ex) { Console.WriteLine($"SendHeartbeat'ta HATA: {ex.ToString()}"); }
        }

        private void SendPacketInternal(Packet packet)
        {
            if (!_isConnected || _stream == null || !_stream.CanWrite) { /*Console.WriteLine("SendPacketInternal: Bağlantı yok veya stream yazılamaz.");*/ return; }
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
            catch (ObjectDisposedException) { Console.WriteLine("SendPacketInternal: Stream dispose edilmiş."); _isConnected = false; }
            catch (IOException ioEx) { Console.WriteLine($"SendPacketInternal IOException: {ioEx.Message}"); _isConnected = false; }
            catch (Exception ex) { Console.WriteLine($"SendPacketInternal'da HATA: {ex.ToString()}"); _isConnected = false; }
        }

        private void HandleCommand(Packet commandPacket)
        {
            //Console.WriteLine($"Komut Alındı: {commandPacket.PacketType}");
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
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .Select(nic => nic.GetPhysicalAddress()?.ToString())
                    .FirstOrDefault(mac => !string.IsNullOrEmpty(mac)) ?? "N/A_MAC";
            }
            catch (Exception ex) { Console.WriteLine($"GetMacAddress HATA: {ex.Message}"); return "N/A_MAC_EX"; }
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
                                byte[] responseBytes = Encoding.UTF8.GetBytes("HakanEXE_AGENT_DISCOVERY_RESPONSE");
                                await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                            }
                        }
                    }
                }
                catch (ObjectDisposedException) { Console.WriteLine("UDP Discovery client dispose edildi."); }
                catch (SocketException se) when (se.SocketErrorCode == SocketError.AddressAlreadyInUse) { Console.WriteLine("UDP Discovery HATA: Port 12345 zaten kullanılıyor."); }
                catch (Exception ex) { Console.WriteLine($"UDP Discovery'de HATA: {ex.ToString()}"); }
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
            Console.WriteLine("AgentClient Dispose çağrıldı.");
        }
    }
}