using System;

namespace HakanEXE.Agent.Core
{
    public static class Keylogger
    {
        public static void Start()
        {
            Console.WriteLine("Keylogger.Start() çağrıldı (implementasyon gerekli).");
            // Gerçek keylogger implementasyonu (low-level keyboard hooks) buraya gelecek.
        }

        public static void Stop()
        {
            Console.WriteLine("Keylogger.Stop() çağrıldı (implementasyon gerekli).");
            // Keylogger'ı durdurma kodları buraya gelecek.
        }
    }
}