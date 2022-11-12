using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RaspberryPi.Process;

namespace RaspberryPi.Network
{
    public interface IAccessPoint
    {
        /// <summary>
        /// Configures an access point using hostapd service.
        /// </summary>
        /// <param name="ssid">SSID to be used in IEEE 802.11 management frames.</param>
        /// <param name="psk">WPA pre-shared keys for WPA-PSK.</param>
        /// <param name="ipAddress">The IP address to be used for this access point.</param>
        /// <param name="channel">Channel number (IEEE 802.11).</param>
        Task ConfigureAsync(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel = null, Country country = null);

        /// <summary>
        /// Removes the access point configuration.
        /// </summary>
        /// <param name="iface"></param>
        /// <returns></returns>
        Task DeleteConfigurationAsync(INetworkInterface iface);

        /// <summary>
        /// Checks if the access point is running.
        /// </summary>
        /// <returns></returns>
        bool IsEnabled();

        CommandLineResult TestDnsmasq();

        /// <summary>
        /// Starts the access point.
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// Restarts the access point.
        /// </summary>
        Task RestartAsync();

        /// <summary>
        /// Stops the access point.
        /// </summary>
        void Stop();
        IEnumerable<ConnectedAccessPointClient> GetConnectedClients(INetworkInterface iface);
    }
}