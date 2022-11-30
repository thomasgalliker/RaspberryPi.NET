using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaspberryPi.Process;

namespace RaspberryPi.Network
{

    /// TODO: Consider merging Interface with Interface class
    public class NetworkManager : INetworkManager
    {
        private readonly ILogger logger;
        private readonly IDHCP dhcp;
        private readonly IAccessPoint accessPoint;
        private readonly IWPA wpa;
        private readonly IProcessRunner processRunner;
        private readonly INetworkInterfaceService networkInterfaceService;

        public NetworkManager(
            ILogger<NetworkManager> logger,
            IDHCP dhcp,
            IAccessPoint accessPoint,
            IWPA wpa,
            IProcessRunner processRunner,
            INetworkInterfaceService networkInterfaceService)
        {
            this.logger = logger;
            this.dhcp = dhcp;
            this.accessPoint = accessPoint;
            this.wpa = wpa;
            this.processRunner = processRunner;
            this.networkInterfaceService = networkInterfaceService;
        }

        /// <inheritdoc />
        //public async Task SetupAccessPoint(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country)
        //{
        //    this.logger.LogDebug($"SetupAccessPoint: iface={iface.Name}, ssid={ssid}");

        //    await this.dhcp.SetIPAddressAsync(iface, ipAddress, null, null, null, forAP: true);

        //    await this.accessPoint.ConfigureAsync(iface, ssid, psk, ipAddress, null, channel, country);

        //    //this.wpa.Stop();

        //    //this.networkInterfaceService.SetLinkDown(iface); // TODO: REALLY???

        //    await this.accessPoint.RestartAsync();
        //}

        public async Task SetupAccessPoint2(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country)
        {
            if (iface == null)
            {
                throw new ArgumentNullException(nameof(iface));
            }

            var ifaceAP = new NetworkInterface($"ap@{iface.Name}");
            this.logger.LogDebug($"SetupAccessPoint: iface={ifaceAP}, ssid={ssid}");

            await this.dhcp.SetIPAddressAsync(ifaceAP, ipAddress, null, null, null, forAP: true);

            await this.accessPoint.ConfigureAsync(ifaceAP, ssid, psk, ipAddress, null, channel, country);

            //this.wpa.Stop();

            //this.networkInterfaceService.SetLinkDown(iface); // TODO: REALLY???

            //await this.accessPoint.RestartAsync();
            this.logger.LogDebug($"SetupAccessPoint: Finished successfully. Please reboot now!");
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
        public async Task SetupStationMode(INetworkInterface iface, WPASupplicantNetwork network, Country country = null)
        {
            // https://raspberrypi.stackexchange.com/questions/117819/configure-back-to-normal-wifi-station-after-access-point-mode-hostapd

            this.logger.LogDebug($"SetupStationMode");

            // No longer in AP mode
            this.accessPoint.Stop();

            await this.dhcp.SetIPAddressAsync(iface, null, null, null, null, null);

            await this.wpa.AddOrUpdateNetworkAsync(network);

            if (country != null)
            {
                var config = await this.wpa.GetConfigAsync();
                if (config.Country != country)
                {
                    config.Country = country;
                    await this.wpa.SetConfigAsync(config);
                }
            }

            await Task.Delay(1000);

            this.processRunner.TryExecuteCommand($"sudo wpa_cli -i {iface.Name} reconfigure");

            await Task.Delay(10000);

            // Disable the adapter
            this.networkInterfaceService.SetLinkDown(iface);

            await Task.Delay(2000);

            // Start station mode
            this.wpa.Start();

            // Enable the adapter again
            this.networkInterfaceService.SetLinkUp(iface);

            // TODO: See line 88 in Interface class
            // wpa_cli list_networks ...
        }
    }
}