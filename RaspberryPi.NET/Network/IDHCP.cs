using System.Net;
using System.Threading.Tasks;

namespace RaspberryPi.Network
{
    public interface IDHCP
    {
        /// <summary>
        /// Get the configured DNS server of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured DNS server or 0.0.0.0 if not configured</returns>
        Task<IPAddress> GetConfiguredDNSServer(string iface); // TODO Replace string with INetworkInterface

        /// <summary>
        /// Get the configured gateway of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured gateway or 0.0.0.0 if not configured</returns>
        Task<IPAddress> GetConfiguredGateway(string iface); // TODO Replace string with INetworkInterface

        /// <summary>
        /// Get the configured IP address of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured IP address or 0.0.0.0 if not configured</returns>
        Task<IPAddress> GetConfiguredIPAddress(string iface); // TODO Replace string with INetworkInterface

        /// <summary>
        /// Get the configured netmask of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured netmask or 0.0.0.0 if not configured</returns>
        Task<IPAddress> GetConfiguredNetmask(string iface); // TODO Replace string with INetworkInterface

        /// <summary>
        /// Checks if an access point is currently configured.
        /// </summary>
        /// <returns><code>true</code> if an access point is configured.</returns>
        Task<bool> IsAPConfiguredAsync();

        /// <summary>
        /// Update the IP address of the given network interface.
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <param name="ip">IP address or null if unchanged</param>
        /// <param name="netmask">Subnet mask or null if unchanged</param>
        /// <param name="gateway">Gateway or null if unchanged</param>
        /// <param name="netmask">Subnet mask or null if unchanged</param>
        /// <param name="dnsServer">Set IP address for AP mode</param>
        Task SetIPAddressAsync(INetworkInterface iface, IPAddress ip, IPAddress netmask, IPAddress gateway, IPAddress dnsServer, bool? forAP = null);
    }
}