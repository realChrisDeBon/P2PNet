using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace P2PNet.NetworkPackets { 
    public class IdentifierPacket
    {
        public string Message { get; set; }
        public int Data { get; set; }
        public string ip { get; set; }
        public IdentifierPacket() { }
        public IdentifierPacket(string message, int data, IPAddress ip_)
        {
            Message = message;
            Data = data;
            ip = ip_.ToString();
        }
    }
}