using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace RaspberryPi.Network
{
    public interface INetworkInterfaceService
    {
        /// <summary>
        /// Returns all available network interfaces.
        /// </summary>
        /// <returns>The enumeration of network interfaces.</returns>
        IEnumerable<INetworkInterface> GetAllNetworkInterfaces();

        /// <summary>
        /// Gets a network interface by <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the network interfaces enumeration.</param>
        /// <returns>The network interface.</returns>
        INetworkInterface GetByIndex(int index);

        /// <summary>
        /// Gets a network interface by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the network interface.</param>
        /// <returns>The network interface.</returns>
        INetworkInterface GetByName(string name);

        /// <summary>
        /// Bring the network interface <paramref name="iface"/> up.
        /// </summary>
        /// <param name="iface">The network interface.</param>
        void SetLinkUp(INetworkInterface iface);

        /// <summary>
        /// Bring the network interface <paramref name="iface"/> down.
        /// </summary>
        /// <param name="iface">The network interface.</param>
        void SetLinkDown(INetworkInterface iface);

        /// <summary>
        /// Assigns a new MAC address to the network interface <paramref name="iface"/>.
        /// </summary>
        /// <param name="iface">The network interface.</param>
        /// <param name="macAddress">The MAC address.</param>
        /// <returns>Command result</returns>
        void SetMacAddress(INetworkInterface iface, PhysicalAddress macAddress);
    }
}