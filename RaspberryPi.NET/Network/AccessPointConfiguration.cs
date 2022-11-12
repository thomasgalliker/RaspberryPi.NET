using System.Net;

namespace RaspberryPi.Network
{
    public class AccessPointConfiguration
    {
        public IPAddress IPAddress { get; set; }

        public IPAddress SubnetMask { get; set; }

        public IPAddress DefaultGateway { get; set; }

        public IPAddress DNSServerAddress { get; set; }
        
        public (IPAddress Start, IPAddress End) DHCPRange { get; set; }
    }
}