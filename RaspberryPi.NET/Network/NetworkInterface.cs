using System.Diagnostics;
using System.Net.NetworkInformation;
using SystemNetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace RaspberryPi.Network
{
    [DebuggerDisplay("{this.Name}")]
    internal class NetworkInterface : INetworkInterface
    {
        private readonly SystemNetworkInterface networkInterface;

        public NetworkInterface(SystemNetworkInterface networkInterface)
        {
            this.networkInterface = networkInterface;
        }

        public string Name => this.networkInterface.Name;

        public OperationalStatus OperationalStatus => this.networkInterface.OperationalStatus;

        public NetworkInterfaceType NetworkInterfaceType => this.networkInterface.NetworkInterfaceType;

        public IPInterfaceProperties GetIPProperties()
        {
            return this.networkInterface.GetIPProperties();
        }
    }
}