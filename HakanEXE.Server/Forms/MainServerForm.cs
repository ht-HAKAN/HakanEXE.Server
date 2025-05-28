using System;
using System.Net;
using System.Windows.Forms;
using HakanEXE.Server.Core;
using HakanEXE.Server.Models;
using System.Collections.Concurrent; // Birden fazla iş parçacığı için güvenli koleksiyon
using System.Collections.Generic;    // Dictionary için eklendi

namespace HakanEXE.Server.Forms
{
    public partial class MainServerForm : Form
    {
        private ServerManager _serverManager;
        private ConcurrentDictionary<string, ClientInfo> _connectedClients; // IP'ye göre istemcileri tutuyorduk, AgentId daha iyi olabilir.
                                                                            // Şimdilik IP ile devam edelim, ClientInfo içinde AgentId var.

        // YENİ EKLENEN ALAN: Açık olan ClientDetailForm'ları AgentId ile takip etmek için
        private Dictionary<string, ClientDetailForm> _openDetailForms = new Dictionary<string, ClientDetailForm>();

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
            listViewClients.FullRowSelect = true; // Satırın tamamını seçmeyi etkinleştir
            listViewClients.Columns.Add("Agent ID", 120);
            listViewClients.Columns.Add("Bilgisayar Adı", 150);
            listViewClients.Columns.Add("IP Adresi", 120);
            listViewClients.Columns.Add("Durum", 80);
            listViewClients.Columns.Add("Son Aktif", 150);
            listViewClients.DoubleClick += ListViewClients_DoubleClick;
        }

        private void ServerManager_ClientConnected(object sender, ClientInfo clientInfo)
        {
            if (clientInfo == null || string.IsNullOrEmpty(clientInfo.IpAddress))
            {
                Console.WriteLine("ServerManager_ClientConnected: clientInfo veya IpAddress null/boş.");
                return;
            }

            // UI güncellemesi için Invoke kullan
            this.Invoke((MethodInvoker)delegate
            {
                // AgentId'yi anahtar olarak kullanmak daha güvenilir olabilir, çünkü IP'ler değişebilir veya birden fazla agent aynı IP'den (NAT arkası) gelebilir (gerçi LAN'da bu daha az olası).
                // Şimdilik IP adresi ile devam ediyoruz ama ClientInfo içinde AgentId de var.
                // _connectedClients için de AgentId'yi anahtar yapmak daha iyi olabilir.
                if (!_connectedClients.ContainsKey(clientInfo.IpAddress)) // Veya AgentId'ye göre kontrol et
                {
                    if (_connectedClients.TryAdd(clientInfo.IpAddress, clientInfo)) // Veya AgentId'ye göre ekle
                    {
                        UpdateClientList();
                        Console.WriteLine($"Yeni İstemci Bağlandı ve Listeye Eklendi: {clientInfo.ComputerName} ({clientInfo.IpAddress}), AgentID: {clientInfo.AgentId}");
                    }
                }
                else
                {
                    // Zaten listede varsa, bilgilerini güncelle (özellikle LastActive ve IsOnline durumu)
                    _connectedClients[clientInfo.IpAddress] = clientInfo; // Bilgileri güncelle
                    UpdateClientStatus(clientInfo.IpAddress, "Çevrimiçi (Yeniden Bağlandı)");
                    Console.WriteLine($"Varolan İstemci Tekrar Bağlandı/Bilgisi Güncellendi: {clientInfo.ComputerName} ({clientInfo.IpAddress})");
                }
            });
        }

        private void ServerManager_ClientDisconnected(object sender, ClientInfo clientInfo)
        {
            if (clientInfo == null || string.IsNullOrEmpty(clientInfo.IpAddress))
            {
                // Eğer clientInfo null ise, _openDetailForms'u temizleyemeyiz.
                // Bu durum ServerManager'daki null check ile çözülmeye çalışılmıştı.
                Console.WriteLine("ServerManager_ClientDisconnected: clientInfo veya IpAddress null/boş.");
                // Belki tüm bağlantısı kopmuş formları kontrol edip kapatmak gerekebilir.
                return;
            }

            this.Invoke((MethodInvoker)delegate
            {
                ClientInfo removedClient;
                if (_connectedClients.TryRemove(clientInfo.IpAddress, out removedClient)) // Veya AgentId'ye göre
                {
                    UpdateClientList();
                    Console.WriteLine($"İstemci Ayrıldı ve Listeden Silindi: {clientInfo.ComputerName} ({clientInfo.IpAddress})");
                }

                // İlgili istemcinin detay formu açıksa onu da kapat veya listeden çıkar
                if (!string.IsNullOrEmpty(clientInfo.AgentId) && _openDetailForms.ContainsKey(clientInfo.AgentId))
                {
                    if (!_openDetailForms[clientInfo.AgentId].IsDisposed)
                    {
                        // Formu kapatmak yerine, belki kullanıcıya bilgi verip pasif hale getirebiliriz.
                        // Şimdilik sadece listeden çıkaralım, form zaten kapanmış olabilir.
                        // _openDetailForms[clientInfo.AgentId].Close(); // Otomatik kapatma
                    }
                    _openDetailForms.Remove(clientInfo.AgentId);
                    Console.WriteLine($"İlgili detay formu ({clientInfo.AgentId}) kapatılanlar listesinden çıkarıldı.");
                }
            });
        }

        private void ServerManager_PacketReceived(object sender, Packet packet)
        {
            if (packet == null || string.IsNullOrEmpty(packet.SenderIp)) return;

            this.Invoke((MethodInvoker)delegate
            {
                if (_connectedClients.TryGetValue(packet.SenderIp, out ClientInfo client)) // Veya AgentId ile eşleştir
                {
                    client.LastActive = DateTime.Now;
                    UpdateClientStatus(packet.SenderIp, "Çevrimiçi");

                    if (packet.PacketType == PacketType.Heartbeat)
                    {
                        // Heartbeat geldiyse sadece son aktif zamanını güncellemek yeterli olabilir.
                        // Console.WriteLine($"Heartbeat alındı: {packet.SenderIp}");
                    }
                    else if (packet.PacketType == PacketType.SystemInfo)
                    {
                        // Bu paket ClientDetailForm tarafından istenir ve orada işlenir.
                        // MainServerForm'un bu paketi doğrudan işlemesine gerek yok gibi.
                        // Ama eğer ClientInfo'yu güncellemek istersek:
                        // try {
                        //    ClientInfo updatedInfo = JsonConvert.DeserializeObject<ClientInfo>(packet.Data);
                        //    if(updatedInfo != null) {
                        //        client.ComputerName = updatedInfo.ComputerName; // vb.
                        //        // UpdateClientList(); // Liste görünümünü güncelle
                        //    }
                        // } catch (Exception ex) { Console.WriteLine($"SystemInfo paketi işlenirken hata: {ex.Message}");}
                    }
                    // Diğer paket türleri genellikle ClientDetailForm tarafından ele alınacak.
                    // Bu olay ClientDetailForm'a da yönlendiriliyor.
                }
            });
        }

        private void UpdateClientList()
        {
            listViewClients.Items.Clear();
            foreach (var client in _connectedClients.Values.ToList()) // ToList() ile koleksiyon değişirken hata almayı engelle
            {
                if (client == null) continue; // Ekstra güvenlik

                var item = new ListViewItem(client.AgentId ?? "N/A");
                item.SubItems.Add(client.ComputerName ?? "N/A");
                item.SubItems.Add(client.IpAddress ?? "N/A");
                item.SubItems.Add(client.IsOnline ? "Çevrimiçi" : "Çevrimdışı");
                item.SubItems.Add(client.LastActive.ToString("G")); // Genel tarih/saat formatı
                item.Tag = client;
                listViewClients.Items.Add(item);
            }
        }

        private void UpdateClientStatus(string ipAddressOrAgentId, string status)
        {
            // Anahtar olarak IP yerine AgentId kullanmak daha iyi olurdu.
            // Şimdilik IP ile devam.
            foreach (ListViewItem item in listViewClients.Items)
            {
                ClientInfo client = item.Tag as ClientInfo;
                if (client != null && client.IpAddress == ipAddressOrAgentId) // Veya client.AgentId == ipAddressOrAgentId
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
                    // Eğer bu istemci için zaten bir detay formu açıksa, onu öne getir.
                    if (_openDetailForms.TryGetValue(selectedClient.AgentId, out ClientDetailForm existingForm))
                    {
                        if (existingForm != null && !existingForm.IsDisposed)
                        {
                            existingForm.WindowState = FormWindowState.Minimized;
                            existingForm.Show(); // Formu göster (eğer gizlenmişse)
                            existingForm.WindowState = FormWindowState.Normal;
                            existingForm.Activate(); // Formu aktif et ve öne getir
                            Console.WriteLine($"Varolan ClientDetailForm öne getirildi: {selectedClient.AgentId}");
                            return; // Yeni form oluşturma
                        }
                        else
                        {
                            // Eğer form dispose edilmişse listeden kaldır
                            _openDetailForms.Remove(selectedClient.AgentId);
                            Console.WriteLine($"Dispose edilmiş ClientDetailForm listeden kaldırıldı: {selectedClient.AgentId}");
                        }
                    }

                    // Yeni bir ClientDetailForm oluştur ve göster
                    Console.WriteLine($"Yeni ClientDetailForm oluşturuluyor: {selectedClient.AgentId}");
                    ClientDetailForm detailForm = new ClientDetailForm(selectedClient, _serverManager);

                    detailForm.FormClosed += (s, ev) =>
                    {
                        // Form kapatıldığında _openDetailForms listesinden kaldır
                        if (_openDetailForms.ContainsKey(selectedClient.AgentId))
                        {
                            _openDetailForms.Remove(selectedClient.AgentId);
                            Console.WriteLine($"ClientDetailForm kapatıldı ve listeden kaldırıldı: {selectedClient.AgentId}");
                        }
                    };

                    _openDetailForms[selectedClient.AgentId] = detailForm; // Yeni formu listeye ekle
                    detailForm.Show(); // Modeless olarak aç
                }
                else if (selectedClient != null && string.IsNullOrEmpty(selectedClient.AgentId))
                {
                    Console.WriteLine("ListViewClients_DoubleClick: Seçilen istemcinin AgentId'si boş veya null.");
                }
            }
        }

        private async void BtnScanNetwork_Click(object sender, EventArgs e) // async void eklendi
        {
            // Bu özellik için NetworkScanner.cs'in doğru implemente edilmiş olması lazım.
            if (_serverManager == null) return;

            // NetworkScanner sınıfı ServerManager içinde oluşturulabilir veya buradan direkt çağrılabilir.
            // Şimdilik ServerManager'a bir metot eklediğimizi varsayalım:
            // _serverManager.ScanForAgentsOnNetwork(); 

            // Veya NetworkScanner'ı burada direkt kullanalım:
            NetworkScanner scanner = new NetworkScanner();
            scanner.AgentDiscovered += (s, agentEndPoint) =>
            {
                // Bu olay farklı bir thread'den gelebilir, UI güncellemesi için Invoke gerekli.
                this.Invoke((MethodInvoker)delegate
                {
                    // Keşfedilen agent için ne yapılacağına karar verilmeli.
                    // Belki bir "Keşfedilenler" listesine eklenir veya direkt bağlanmaya çalışılır.
                    // Şimdilik sadece bir mesaj gösterelim.
                    MessageBox.Show($"Yeni Agent keşfedildi: {agentEndPoint.Address}:{agentEndPoint.Port}", "Ağ Taraması");
                    // Burada agent'a direkt bağlanma veya listeye ekleme mantığı eklenebilir.
                    // Örneğin, _serverManager.ConnectToDiscoveredAgent(agentEndPoint);
                });
            };

            try
            {
                await scanner.ScanNetworkAsync(); // Asenkron olarak tarama yap
                MessageBox.Show("Ağ tarama isteği gönderildi. Yanıtlar bekleniyor...", "Ağ Taraması");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ağ taraması sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}