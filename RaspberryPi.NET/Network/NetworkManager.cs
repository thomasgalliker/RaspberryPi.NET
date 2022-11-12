using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RaspberryPi.Network
{

    /// TODO: Consider merging Interface with Interface class
    public class NetworkManager : INetworkManager
    {
        private readonly ILogger logger;
        private readonly IDHCP dhcp;
        private readonly IAccessPoint accessPoint;
        private readonly IWPA wpa;
        private readonly INetworkInterfaceService networkInterfaceService;

        public NetworkManager(
            ILogger<NetworkManager> logger,
            IDHCP dhcp,
            IAccessPoint accessPoint,
            IWPA wpa,
            INetworkInterfaceService networkInterfaceService)
        {
            this.logger = logger;
            this.dhcp = dhcp;
            this.accessPoint = accessPoint;
            this.wpa = wpa;
            this.networkInterfaceService = networkInterfaceService;
        }

        /// <inheritdoc />
        public async Task SetupAccessPoint(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country)
        {
            this.logger.LogDebug($"SetupAccessPoint: iface={iface.Name}, ssid={ssid}");

            await this.dhcp.SetIPAddressAsync(iface, ipAddress, null, null, null, forAP: true);

            await this.accessPoint.ConfigureAsync(iface, ssid, psk, ipAddress, channel, country);

            //this.wpa.Stop();

            //this.networkInterfaceService.SetLinkDown(iface); // TODO: REALLY???

            await this.accessPoint.RestartAsync();
        }

        /// <inheritdoc />
        public Task StopAccessPointAsync(INetworkInterface iface)
        {
            // Disable WiFi services
            this.accessPoint.Stop();
            this.wpa.Stop();

            // Disable WiFi adapter
            this.networkInterfaceService.SetLinkDown(iface);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task SetupStationMode(INetworkInterface iface, WPASupplicantNetwork network)
        {
            // https://raspberrypi.stackexchange.com/questions/117819/configure-back-to-normal-wifi-station-after-access-point-mode-hostapd

            this.logger.LogDebug($"SetupStationMode");

            await this.wpa.AddOrUpdateNetworkAsync(network);

            await this.dhcp.SetIPAddressAsync(iface, null, null, null, null, null);

            // No longer in AP mode
            this.accessPoint.Stop();

            // Disable the adapter
            //this.networkInterfaceService.SetLinkDown(iface);

            // Start station mode
            this.wpa.Start();

            // Enable the adapter again
            //this.networkInterfaceService.SetLinkUp(iface);

            // TODO: See line 88 in Interface class
            // wpa_cli list_networks ...
        }
    }
}