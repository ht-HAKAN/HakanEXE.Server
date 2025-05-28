using System;
using System.Net;
using System.Windows.Forms;
using HakanEXE.Server.Core;
using HakanEXE.Server.Models;
using System.Collections.Concurrent; // Birden fazla iş parçacığı için güvenli koleksiyon
using System.Collections.Generic;    // Dictionary için eklendi
using System.Linq; // ToList() için eklendi

namespace HakanEXE.Server.Forms
{
    public partial class MainServerForm : Form
    {
        private ServerManager _serverManager;
        private ConcurrentDictionary<string, ClientInfo> _connectedClients;

        // YENİ EKLENEN ALAN: Açık olan ClientDetailForm'ları AgentId ile takip etmek için
        private Dictionary<string, ClientDetailForm> _openDetailForms = new Dictionary<string, ClientDetailForm>();

        public MainServerForm()
        {
            InitializeComponent();
            _connectedClients = new ConcurrentDictionary<string, ClientInfo>();
            InitializeServer();
            SetupListView();
        }

        private void InitializeServer()
        {
            _serverManager = new ServerManager(8888);
            _serverManager.ClientConnected += ServerManager_ClientConnected;
            _serverManager.ClientDisconnected += ServerManager_ClientDisconnected;
            _serverManager.PacketReceived += ServerManager_PacketReceived;
            _serverManager.StartListening();
            this.Text = "HakanEXE Sunucu - Dinleniyor...";
        }

        private void SetupListView()
        {
            listViewClients.View = View.Details;
            listViewClients.FullRowSelect = true;
            listViewClients.Columns.Add("Agent ID", 120);
            listViewClients.Columns.Add("Bilgisayar Adı", 150);
            listViewClients.Columns.Add("IP Adresi", 120);
            listViewClients.Columns.Add("Durum", 80);
            listViewClients.Columns.Add("Son Aktif", 150);
            listViewClients.DoubleClick += ListViewClients_DoubleClick;
        }

        private void ServerManager_ClientConnected(object sender, ClientInfo clientInfo)
        {
            if (clientInfo == null || string.IsNullOrEmpty(clientInfo.AgentId)) // AgentId üzerinden kontrol daha iyi
            {
                Console.WriteLine("ServerManager_ClientConnected: clientInfo veya AgentId null/boş.");
                return;
            }

            this.Invoke((MethodInvoker)delegate
            {
                if (!_connectedClients.ContainsKey(clientInfo.AgentId))
                {
                    if (_connectedClients.TryAdd(clientInfo.AgentId, clientInfo))
                    {
                        UpdateClientList();
                        Console.WriteLine($"Yeni İstemci Bağlandı ve Listeye Eklendi: {clientInfo.ComputerName} ({clientInfo.IpAddress}), AgentID: {clientInfo.AgentId}");
                    }
                }
                else
                {
                    _connectedClients[clientInfo.AgentId] = clientInfo;
                    UpdateClientStatus(clientInfo.AgentId, "Çevrimiçi (Yeniden Bağlandı)");
                    Console.WriteLine($"Varolan İstemci Tekrar Bağlandı/Bilgisi Güncellendi: {clientInfo.ComputerName} ({clientInfo.AgentId})");
                }
            });
        }

        private void ServerManager_ClientDisconnected(object sender, ClientInfo clientInfo)
        {
            if (clientInfo == null || string.IsNullOrEmpty(clientInfo.AgentId))
            {
                Console.WriteLine("ServerManager_ClientDisconnected: clientInfo veya AgentId null/boş.");
                return;
            }

            this.Invoke((MethodInvoker)delegate
            {
                ClientInfo removedClient;
                if (_connectedClients.TryRemove(clientInfo.AgentId, out removedClient))
                {
                    UpdateClientList();
                    Console.WriteLine($"İstemci Ayrıldı ve Listeden Silindi: {clientInfo.ComputerName} ({clientInfo.AgentId})");
                }

                if (_openDetailForms.TryGetValue(clientInfo.AgentId, out ClientDetailForm openForm))
                {
                    if (openForm != null && !openForm.IsDisposed)
                    {
                        // Formu UI thread'inde kapatmak daha güvenli olabilir.
                        // openForm.Close(); // Otomatik kapatma için
                        Console.WriteLine($"İlgili detay formu ({clientInfo.AgentId}) bağlantı kesildiği için kapatılacak veya pasif hale getirilecek.");
                    }
                    _openDetailForms.Remove(clientInfo.AgentId);
                }
            });
        }

        private void ServerManager_PacketReceived(object sender, Packet packet)
        {
            if (packet == null || string.IsNullOrEmpty(packet.SenderIp)) return; // SenderIp yerine AgentId kullanılmalı paketlerde de

            // AgentId'yi paketten almak daha doğru olurdu, şimdilik SenderIp ile eşleşen client'ı bulalım.
            // Bu kısım ClientDetailForm'a daha çok veri gönderecek.
            // Ana formda sadece heartbeat ile son aktifliği güncellemek yeterli olabilir.
            var clientEntry = _connectedClients.FirstOrDefault(kvp => kvp.Value.IpAddress == packet.SenderIp);
            if (!string.IsNullOrEmpty(clientEntry.Key)) // Eğer client bulunduysa (AgentId'si boş değilse)
            {
                ClientInfo client = clientEntry.Value;
                if (client != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        client.LastActive = DateTime.Now;
                        UpdateClientStatus(client.AgentId, "Çevrimiçi");

                        if (packet.PacketType == PacketType.Heartbeat)
                        {
                            // Console.WriteLine($"Heartbeat alındı: {client.AgentId}");
                        }
                        // Diğer paket türleri ClientDetailForm tarafından işlenecek,
                        // ServerManager'daki PacketReceived olayı ClientDetailForm'a da abone edilecek.
                    });
                }
            }
        }

        private void UpdateClientList()
        {
            listViewClients.Items.Clear();
            foreach (var client in _connectedClients.Values.ToList())
            {
                if (client == null) continue;

                var item = new ListViewItem(client.AgentId ?? "N/A");
                item.SubItems.Add(client.ComputerName ?? "N/A");
                item.SubItems.Add(client.IpAddress ?? "N/A");
                item.SubItems.Add(client.IsOnline ? "Çevrimiçi" : "Çevrimdışı"); // IsOnline property'si ClientInfo'da olmalı
                item.SubItems.Add(client.LastActive.ToString("G"));
                item.Tag = client;
                listViewClients.Items.Add(item);
            }
        }

        private void UpdateClientStatus(string agentId, string status)
        {
            foreach (ListViewItem item in listViewClients.Items)
            {
                ClientInfo client = item.Tag as ClientInfo;
                if (client != null && client.AgentId == agentId)
                {
                    item.SubItems[3].Text = status;
                    item.SubItems[4].Text = DateTime.Now.ToString("G");
                    break;
                }
            }
        }

        private void ListViewClients_DoubleClick(object sender, EventArgs e)
        {
            if (listViewClients.SelectedItems.Count > 0)
            {
                ClientInfo selectedClient = listViewClients.SelectedItems[0].Tag as ClientInfo;
                if (selectedClient != null && !string.IsNullOrEmpty(selectedClient.AgentId))
                {
                    if (_openDetailForms.TryGetValue(selectedClient.AgentId, out ClientDetailForm existingForm))
                    {
                        if (existingForm != null && !existingForm.IsDisposed)
                        {
                            existingForm.WindowState = FormWindowState.Minimized;
                            existingForm.Show();
                            existingForm.WindowState = FormWindowState.Normal;
                            existingForm.Activate();
                            Console.WriteLine($"Varolan ClientDetailForm öne getirildi: {selectedClient.AgentId}");
                            return;
                        }
                        else
                        {
                            _openDetailForms.Remove(selectedClient.AgentId);
                            Console.WriteLine($"Dispose edilmiş ClientDetailForm listeden kaldırıldı: {selectedClient.AgentId}");
                        }
                    }

                    Console.WriteLine($"Yeni ClientDetailForm oluşturuluyor: {selectedClient.AgentId}");
                    ClientDetailForm detailForm = new ClientDetailForm(selectedClient, _serverManager);

                    detailForm.FormClosed += (s, ev) =>
                    {
                        if (_openDetailForms.ContainsKey(selectedClient.AgentId))
                        {
                            _openDetailForms.Remove(selectedClient.AgentId);
                            Console.WriteLine($"ClientDetailForm kapatıldı ve listeden kaldırıldı: {selectedClient.AgentId}");
                        }
                    };

                    _openDetailForms[selectedClient.AgentId] = detailForm;
                    detailForm.Show();
                }
                else if (selectedClient != null && string.IsNullOrEmpty(selectedClient.AgentId))
                {
                    Console.WriteLine("ListViewClients_DoubleClick: Seçilen istemcinin AgentId'si boş veya null.");
                }
            }
        }

        private async void BtnScanNetwork_Click(object sender, EventArgs e)
        {
            if (_serverManager == null) return;

            NetworkScanner scanner = new NetworkScanner();
            scanner.AgentDiscovered += (s, agentEndPoint) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Yeni Agent keşfedildi: {agentEndPoint.Address}:{agentEndPoint.Port}", "Ağ Taraması");
                });
            };

            try
            {
                await scanner.ScanNetworkAsync();
                MessageBox.Show("Ağ tarama isteği gönderildi. Yanıtlar bekleniyor...", "Ağ Taraması");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ağ taraması sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}