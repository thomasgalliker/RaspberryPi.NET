using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using RaspberryPi.Process;
using SystemNetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace RaspberryPi.Network
{
    /// <inheritdoc/>
    public class NetworkInterfaceService : INetworkInterfaceService
    {
        private readonly IProcessRunner processRunner;

        public NetworkInterfaceService(IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        /// <inheritdoc/>
        public IEnumerable<INetworkInterface> GetAll()
        {
            return SystemNetworkInterface.GetAllNetworkInterfaces().Select(i => new NetworkInterface(i));
        }

        /// <inheritdoc/>
        public INetworkInterface GetByIndex(int index)
        {
            var i = 0;
            foreach (var iface in this.GetAll())
            {
                if (iface.NetworkInterfaceType != NetworkInterfaceType.Loopback && i++ == index)
                {
                    return iface;
                }
            }
            throw new ArgumentException($"Network connection with index \"{index}\" could not be found.", nameof(index));
        }

        /// <inheritdoc/>
        public INetworkInterface GetByName(string name)
        {
            var iface = this.GetAll().SingleOrDefault(i => i.Name == name);
            if (iface == null)
            {
                throw new ArgumentException($"Network connection with name \"{name}\" could not be found.", nameof(name));
            }

            return iface;
        }

        /// <inheritdoc/>
        public void SetLinkUp(INetworkInterface iface)
        {
            if (iface == null)
            {
                throw new ArgumentNullException(nameof(iface), $"Parameter '{nameof(iface)}' must not be null");
            }

            this.processRunner.ExecuteCommand($"sudo ip link set {iface.Name} up");
        }

        /// <inheritdoc/>
        public void SetLinkDown(INetworkInterface iface)
        {
            if (iface == null)
            {
                throw new ArgumentNullException(nameof(iface), $"Parameter '{nameof(iface)}' must not be null");
            }

            this.processRunner.ExecuteCommand($"sudo ip link set {iface.Name} down");
        }

        /// <inheritdoc/>
        public void SetMacAddress(INetworkInterface iface, PhysicalAddress macAddress)
        {
            if (iface == null)
            {
                throw new ArgumentNullException(nameof(iface), $"Parameter '{nameof(iface)}' must not be null");
            }

            if (macAddress == null)
            {
                throw new ArgumentNullException(nameof(macAddress), $"Parameter '{nameof(macAddress)}' must not be null");
            }

            var isUp = iface.OperationalStatus == OperationalStatus.Up;

            // Set link down (if needed)
            if (isUp)
            {
                this.SetLinkDown(iface);
                //this.processRunner.ExecuteCommand($"sudo ip link set dev {iface.Name} down");
            }

            // Update MAC address
            var newMacAddress = BitConverter.ToString(macAddress.GetAddressBytes()).Replace('-', ':');
            this.processRunner.ExecuteCommand($"sudo ip link set dev {iface.Name} address {newMacAddress}");

            // Set link up again (if needed)
            if (isUp)
            {
                this.SetLinkUp(iface);
                //this.processRunner.ExecuteCommand($"sudo ip link set dev {iface.Name} up");
            }
        }
    }
}
