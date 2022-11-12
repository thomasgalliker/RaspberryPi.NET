using System.Net.NetworkInformation;

namespace RaspberryPi.Network
{
    public interface INetworkInterface
    {
        public string Name { get; }

        public OperationalStatus OperationalStatus { get; }
        NetworkInterfaceType NetworkInterfaceType { get; }

        IPInterfaceProperties GetIPProperties();
    }
}