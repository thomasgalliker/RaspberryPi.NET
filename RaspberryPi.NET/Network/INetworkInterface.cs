using System.Net.NetworkInformation;

namespace RaspberryPi.Network
{
    public interface INetworkInterface
    {
        public string Name { get; }

        public string GetPhysicalName();
        
        public string GetVirtualName();
        
        public OperationalStatus OperationalStatus { get; }

        NetworkInterfaceType NetworkInterfaceType { get; }

        IPInterfaceProperties GetIPProperties();
    }
}