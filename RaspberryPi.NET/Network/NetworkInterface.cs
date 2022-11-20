using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using SystemNetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace RaspberryPi.Network
{
    [DebuggerDisplay("NetworkInterface: {this.Name}")]
    public class NetworkInterface : INetworkInterface
    {
        private SystemNetworkInterface networkInterface;
        private readonly string name;

        public NetworkInterface(SystemNetworkInterface networkInterface)
        {
            if (networkInterface == null)
            {
                throw new ArgumentNullException(nameof(networkInterface), $"Parameter '{nameof(networkInterface)}' must not be null or empty");
            }

            this.networkInterface = networkInterface;
        }

        public NetworkInterface(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"Parameter '{nameof(name)}' must not be null or empty", nameof(name));
            }

            this.name = name;
        }

        public string Name => this.networkInterface?.Name ?? this.name;

        public string GetPhysicalName()
        {
            var split = this.Name.Split(new[] { '@' }, StringSplitOptions.None);
            var physicalName = split.Length == 2 ? split[1] : this.Name;
            return physicalName;
        }
        
        public string GetVirtualName()
        {
            var split = this.Name.Split(new[] { '@' }, StringSplitOptions.None);
            var physicalName = split.Length == 2 ? split[0] : null;
            return physicalName;
        }

        public OperationalStatus OperationalStatus
        {
            get
            {
                return this.networkInterface?.OperationalStatus ?? OperationalStatus.Unknown;
            }
        }

        public NetworkInterfaceType NetworkInterfaceType
        {
            get
            {
                return this.networkInterface?.NetworkInterfaceType ?? NetworkInterfaceType.Unknown;
            }
        }

        public IPInterfaceProperties GetIPProperties()
        {
            return this.networkInterface?.GetIPProperties() ?? throw new NotSupportedException();
        }

        public void Refresh()
        {
            this.networkInterface = SystemNetworkInterface.GetAllNetworkInterfaces().SingleOrDefault(i => i.Name == this.Name);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}