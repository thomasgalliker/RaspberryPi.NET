using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaspberryPi.Network
{
    public interface IWPA
    {
        /// <summary>
        /// Reports the current configuration of the network.
        /// </summary>
        [Obsolete]
        Task<string> GetReportAsync();

        /// <summary>
        /// Starts the wpa_supplicant service.
        /// </summary>
        void Start();

        /// <summary>
        /// Restarts the wpa_supplicant service.
        /// </summary>
        void Restart();

        /// <summary>
        /// Stops the wpa_supplicant service.
        /// </summary>
        void Stop();

        /// <summary>
        /// Returns the SSIDs of all interfaces (if connected to any wireless network).
        /// </summary>
        /// <returns>The list of SSIDs.</returns>
        IEnumerable<string> GetConnectedSSIDs();

        /// <summary>
        /// Returns the SSIDs of interface '<paramref name="iface"/>' (if connected to any wireless network).
        /// </summary>
        /// <returns>The list of SSIDs.</returns>
        IEnumerable<string> GetConnectedSSIDs(INetworkInterface iface);

        /// <summary>
        /// Scans the area for available wireless networks.
        /// </summary>
        /// <returns>List of SSIDs available to connect.</returns>
        IEnumerable<string> ScanSSIDs(INetworkInterface iface);

        /// <summary>
        /// Gets the current wpa_supplicant.conf.
        /// </summary>
        /// <returns>The wpa_supplicant.conf.</returns>
        Task<WPASupplicantConf> GetWPASupplicantConfAsync();

        /// <summary>
        /// Sets the wpa_supplicant.conf with the given <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The updated wpa_supplicant.conf.</param>
        Task SetWPASupplicantConfAsync(WPASupplicantConf config);

        /// <summary>
        /// Returns the network with <paramref name="ssid"/> from wpa_supplicant.conf.
        /// </summary>
        /// <param name="ssid">The SSID.</param>
        /// <returns>The network section with given SSID.</returns>
        Task<WPASupplicantNetwork> GetNetworkAsync(string ssid);

        /// <summary>
        /// Adds or updates the network configuration given in <paramref name="network"/> which matches the SSID.
        /// </summary>
        /// <param name="network">The network configuration.</param>
        Task AddOrUpdateNetworkAsync(WPASupplicantNetwork network);

        /// <summary>
        /// Removes the network configuration <paramref name="network"/> which matches the SSID.
        /// </summary>
        /// <param name="network">The network configuration.</param>
        Task RemoveNetworkAsync(WPASupplicantNetwork network);
    }
}