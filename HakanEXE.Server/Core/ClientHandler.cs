using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HakanEXE.Server.Models;
using Newtonsoft.Json;
using System.IO;

namespace HakanEXE.Server.Core
{
    public class ClientHandler : IDisposable
    {
        private TcpClient _clientSocket;
        private NetworkStream _networkStream;
        private Thread _clientThread;
        private bool _isRunning = false;
        private ClientInfo _clientInfo; // Bu istemcinin bilgileri

        public event EventHandler<Packet> PacketReceived;
        public event EventHandler<ClientInfo> ClientDisconnected;

        public ClientHandler(TcpClient clientSocket)
        {
            _clientSocket = clientSocket;
            _networkStream = _clientSocket.GetStream();
        }

        public void SetClientInfo(ClientInfo info)
        {
            _clientInfo = info;
        }

        public void StartHandlingClient()
        {
            _isRunning = true;
            _clientThread = new Thread(ReceiveData);
            _clientThread.IsBackground = true;
            _clientThread.Start();
        }

        private void ReceiveData()
        {
            byte[] headerBuffer = new byte[4]; // Paket boyutunu tutacak (int)
            byte[] dataBuffer;
            string decryptedData;

            try
            {
                while (_isRunning && _clientSocket.Connected)
                {
                    // Paket boyutunu oku
                    int bytesRead = _networkStream.Read(headerBuffer, 0, headerBuffer.Length);
                    if (bytesRead == 0) throw new SocketException(); // Bağlantı kapandı

                    int dataLength = BitConverter.ToInt32(headerBuffer, 0);
                    if (dataLength <= 0) continue;

                    dataBuffer = new byte[dataLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < dataLength)
                    {
                        bytesRead = _networkStream.Read(dataBuffer, totalBytesRead, dataLength - totalBytesRead);
                        if (bytesRead == 0) throw new SocketException();
                        totalBytesRead += bytesRead;
                    }

                    // Şifre çözme
                    decryptedData = EncryptionHelper.Decrypt(dataBuffer);

                    Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(decryptedData);
                    if (receivedPacket != null)
                    {
                        // İlk bağlantıda ClientInfo'yu alırız, sonraki paketlerde SenderIp'yi handler'a atarız.
                        if (_clientInfo == null && receivedPacket.PacketType == PacketType.ClientInfo)
                        {
                            _clientInfo = JsonConvert.DeserializeObject<ClientInfo>(receivedPacket.Data);
                            receivedPacket.SenderIp = _clientInfo.IpAddress; // Packet'a da IP'yi set et
                        }
                        else if (_clientInfo != null)
                        {
                            receivedPacket.SenderIp = _clientInfo.IpAddress;
                        }
                        PacketReceived?.Invoke(this, receivedPacket);
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine($"İstemci ayrıldı: {_clientInfo?.IpAddress ?? "Bilinmiyor"}");
                ClientDisconnected?.Invoke(this, _clientInfo);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ağ akışı hatası (ReceiveData): {ex.Message}");
                ClientDisconnected?.Invoke(this, _clientInfo); // Bağlantı koptu varsay
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İstemci veri alma hatası: {ex.Message}");
                ClientDisconnected?.Invoke(this, _clientInfo);
            }
            finally
            {
                Dispose();
            }
        }

        public void SendPacket(Packet packet)
        {
            try
            {
                string jsonPacket = JsonConvert.SerializeObject(packet);
                byte[] encryptedData = EncryptionHelper.Encrypt(jsonPacket);
                byte[] dataLength = BitConverter.GetBytes(encryptedData.Length);

                _networkStream.Write(dataLength, 0, dataLength.Length); // Boyutu gönder
                _networkStream.Write(encryptedData, 0, encryptedData.Length); // Veriyi gönder
                _networkStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Paket gönderme hatası: {ex.Message}");
                // Bağlantı kopmuş olabilir, yeniden bağlanmayı tetikle (server tarafında değil)
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            if (_networkStream != null)
            {
                _networkStream.Close();
                _networkStream.Dispose();
            }
            if (_clientSocket != null)
            {
                _clientSocket.Close();
                _clientSocket.Dispose();
            }
            if (_clientThread != null && _clientThread.IsAlive)
            {
                _clientThread.Join(100); // Küçük bir bekleme süresi ver
                if (_clientThread.IsAlive) _clientThread.Abort(); // Abort riskli
            }
            GC.SuppressFinalize(this);
        }
    }
}