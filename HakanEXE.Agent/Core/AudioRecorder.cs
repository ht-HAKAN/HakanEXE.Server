using System;

namespace HakanEXE.Agent.Core
{
    public static class AudioRecorder
    {
        public static void StartRecording()
        {
            Console.WriteLine("AudioRecorder.StartRecording() çağrıldı (implementasyon gerekli).");
            // Gerçek ses kayıt implementasyonu (NAudio gibi bir kütüphane ile) buraya gelecek.
        }

        public static void StopRecording()
        {
            Console.WriteLine("AudioRecorder.StopRecording() çağrıldı (implementasyon gerekli).");
            // Ses kaydını durdurma ve belki dosyayı kaydetme kodları buraya gelecek.
        }
    }
}