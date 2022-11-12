using System.Net.NetworkInformation;

namespace RaspberryPi.Network
{
    public class ConnectedAccessPointClient
    {
        public PhysicalAddress MacAddress { get; set; }
        
        public string TxBitrate { get; set; }
        
        public string RxBitrate { get; set; }

        public bool Authorized { get; set; }

        public bool Authenticated { get; set; }
    }
}