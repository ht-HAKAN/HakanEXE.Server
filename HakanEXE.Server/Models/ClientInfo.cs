using System;

namespace HakanEXE.Server.Models
{
    public class ClientInfo
    {
        public string AgentId { get; set; } // Benzersiz ID
        public string ComputerName { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string OperatingSystem { get; set; }
        public DateTime LastActive { get; set; }
        public bool IsOnline { get; set; } // Bu, aslında LastActive ile belirlenebilir

        public ClientInfo()
        {
            LastActive = DateTime.Now;
            IsOnline = true;
        }
    }
}