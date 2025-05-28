using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms; // Screen sınıfı için

namespace HakanEXE.Agent.Core
{
    public static class ScreenCapture
    {
        public static byte[] CaptureScreen()
        {
            try
            {
                // Ana ekranın sınırlarını al
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                // Belirtilen boyutlarda bir bitmap oluştur
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    // Bitmap üzerinden bir grafik nesnesi oluştur
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Ekran görüntüsünü bitmap'e kopyala
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    // Bitmap'i bir memory stream'e JPEG formatında kaydet
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Kalite ayarı için EncoderParameters kullanılabilir
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L); // 0-100L arası kalite
                        myEncoderParameters.Param[0] = myEncoderParameter;

                        bitmap.Save(ms, jpgEncoder, myEncoderParameters);
                        //bitmap.Save(ms, ImageFormat.Jpeg); // Daha basit versiyon
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ekran görüntüsü alma hatası: " + ex.Message);
                return null;
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}