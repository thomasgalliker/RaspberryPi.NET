using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using RaspberryPi.Process;
using RaspberryPi.Storage;

namespace RaspberryPi.Network
{
    /// <summary>
    /// Manages network interfaces.
    /// TODO: This class requires heavy refactoring!!
    /// TODO: Consider merging Interface with NetworkManager class
    /// </summary>
    public class Interface : IInterface
    {
        private readonly ILogger logger;
        private readonly IProcessRunner processRunner;
        private readonly INetworkInterfaceService networkInterfaceService;
        private readonly IAccessPoint accessPoint;
        private readonly IDHCP dhcp;
        private readonly IWPA wpa;
        private readonly IFileSystem fileSystem;

        public Interface(
            ILogger<Interface> logger,
            IProcessRunner processRunner,
            INetworkInterfaceService networkInterfaceService,
            IAccessPoint accessPoint,
            IDHCP dhcp,
            IWPA wpa,
            IFileSystem fileSystem)
        {
            this.logger = logger;
            this.processRunner = processRunner;
            this.networkInterfaceService = networkInterfaceService;
            this.accessPoint = accessPoint;
            this.dhcp = dhcp;
            this.wpa = wpa;
            this.fileSystem = fileSystem;
        }

        public async Task SetConfig(INetworkInterface iface, object pParam, object sParam)
        {
            if (iface.Name.StartsWith("w"))
            {
                // WiFi interface
                if (sParam == null)
                {
                }
                else if (sParam is <= 0 or > 2)
                {
                   // await this.StopAccessPointAsync(iface);
                }
                else if (sParam is int s && s == 1)
                {
                    // Is there a wpa_supplicant.conf?
                    if (!this.fileSystem.File.Exists("/etc/wpa_supplicant/wpa_supplicant.conf"))
                    {
                        throw new InvalidOperationException("No WiFi configuration found, use M587 to configure at least one SSID");
                    }

                    // No longer in AP mode
                    this.accessPoint.Stop();

                    // Disable the adapter
                    this.networkInterfaceService.SetLinkDown(iface);

                    // Start station mode
                    this.wpa.Start();

                    // Enable the adapter again
                    this.networkInterfaceService.SetLinkUp(iface);

                    // Connect to the given SSID (if applicable)
                    if (pParam != null)
                    {
                        // Find the network index
                        var networkListResult = this.processRunner.ExecuteCommand("wpa_cli list_networks");
                        var networkList = networkListResult.OutputData;
                        var regexString = Regex.Escape($"{sParam}");
                        var ssidRegex = new Regex($"^(\\d+)\\s+{regexString}\\W", RegexOptions.IgnoreCase);

                        var networkIndex = -1;
                        using (var reader = new StringReader(networkList))
                        {
                            do
                            {
                                var line = await reader.ReadLineAsync();
                                if (line == null)
                                {
                                    break;
                                }

                                var match = ssidRegex.Match(line);
                                if (match.Success)
                                {
                                    networkIndex = int.Parse(match.Groups[1].Value);
                                    break;
                                }
                            }
                            while (true); // TODO: Use timeout here!
                        }

                        if (networkIndex == -1)
                        {
                            throw new InvalidOperationException("SSID could not be found, use M587 to configure it first");
                        }

                        // Select it
                        var selectResult = this.processRunner.ExecuteCommand($"wpa_cli -i {iface.Name} select_network {networkIndex}");
                        if (selectResult.OutputData.Trim() != "OK")
                        {
                            //result.AppendLine(selectResult);
                        }
                    }
                    // else wpa_supplicant will connect to the next available network
                }
                else if (sParam is int s2 && s2 == 2)
                {
                    // Are the required config files present?
                    if (!this.fileSystem.File.Exists("/etc/hostapd/wlan0.conf"))
                    {
                        throw new InvalidOperationException("No hostapd configuration found, use M589 to configure the access point first");
                    }

                    if (!this.fileSystem.File.Exists("/etc/dnsmasq.conf"))
                    {
                        throw new InvalidOperationException("No dnsmasq configuration found, use M589 to configure the access point first");
                    }

                    // Is there at least one DHCP profile for AP mode?
                    if (!await this.dhcp.IsAPConfiguredAsync())
                    {
                        throw new InvalidOperationException("No access point configuration found. Use M587 to configure it first");
                    }

                    // No longer in station mode
                    this.wpa.Stop();

                    // Disable the adapter
                    this.networkInterfaceService.SetLinkDown(iface);

                    // Start AP mode. This will enable the adapter too
                    await this.accessPoint.StartAsync();
                }
            }
            else
            {
                // Ethernet interface
                if (pParam != null)
                {
                    // Set IP address
                    var ip = IPAddress.Parse($"{pParam}");
                    await this.dhcp.SetIPAddressAsync(iface, ip, null, null, null);
                }

                if (sParam is bool sbool && iface.OperationalStatus != OperationalStatus.Up != sbool)
                {
                    // Enable or disable the adapter if required
                    if ((bool)sParam)
                    {
                        this.networkInterfaceService.SetLinkUp(iface);
                    }
                    else
                    {
                        this.networkInterfaceService.SetLinkDown(iface);
                    }
                }
            }
        }

        /// <summary>
        /// Set and/or report the network mask via M553
        /// </summary>
        /// <param name="index">Index of the network interface</param>
        /// <param name="netmask">Subnet mask</param>
        /// <returns>Configuration result</returns>
        public async Task ManageNetmask(INetworkInterface iface, IPAddress netmask)
        {
            if (netmask != null)
            {
                await this.dhcp.SetIPAddressAsync(iface, null, netmask, null, null);
            }

            if (iface.OperationalStatus == OperationalStatus.Up)
            {
                var ipInfo = (from unicastAddress in iface.GetIPProperties().UnicastAddresses
                              where unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork
                              select unicastAddress).FirstOrDefault();
                netmask = ipInfo != null ? ipInfo.IPv4Mask : IPAddress.Any;
            }
            else
            {
                netmask = await this.dhcp.GetConfiguredNetmask(iface.Name);
            }
            //return $"Net mask: {netmask}";
        }

        /// <summary>
        /// Set the network mask via M553
        /// </summary>
        /// <param name="index">Index of the network interface</param>
        /// <param name="netmask">Subnet mask</param>
        /// <returns>Configuration result</returns>
        public async Task ManageGateway(INetworkInterface iface, IPAddress gateway, IPAddress dnsServer)
        {
            if (gateway != null || dnsServer != null)
            {
                await this.dhcp.SetIPAddressAsync(iface, null, null, gateway, dnsServer);
            }

            if (iface.OperationalStatus == OperationalStatus.Up)
            {
                gateway = (from item in iface.GetIPProperties().GatewayAddresses
                           where item.Address.AddressFamily == AddressFamily.InterNetwork
                           select item.Address).FirstOrDefault() ?? IPAddress.Any;
                dnsServer = (from item in iface.GetIPProperties().DnsAddresses
                             where item.AddressFamily == AddressFamily.InterNetwork
                             select item).FirstOrDefault() ?? IPAddress.Any;
            }
            else
            {
                gateway = await this.dhcp.GetConfiguredGateway(iface.Name);
                dnsServer = await this.dhcp.GetConfiguredDNSServer(iface.Name);
            }

            //StringBuilder builder = new();
            //builder.AppendLine($"Gateway: {gateway}");
            //builder.Append($"DNS server: {dnsServer}");
            //return builder.ToString();
        }

        /// <summary>
        /// Report the IP address of the network interface(s)
        /// </summary>
        /// <param name="builder">String builder to write to</param>
        /// <param name="iface">Optional network interface</param>
        /// <param name="index">Index of the network interface</param>
        public async Task<string> ReportAsync(INetworkInterface iface)
        {
            var stringBuilder = new StringBuilder();
            this.logger.LogInformation("ReportAsync");
            if (this.networkInterfaceService.GetAll().Count(item => item.NetworkInterfaceType != NetworkInterfaceType.Loopback) > 1)
            {
                // Add labels if there is more than one available network interface
                stringBuilder.Append($"Interface {iface.Name}: ");
            }

            if (iface.Name.StartsWith("w"))
            {
                // WiFi interface
                if (iface.OperationalStatus != OperationalStatus.Down)
                {
                    var ipInfo = (from unicastAddress in iface.GetIPProperties().UnicastAddresses
                                  where unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork
                                  select unicastAddress).FirstOrDefault();
                    if (ipInfo != null)
                    {
                        var isAccessPoint = this.accessPoint.IsEnabled();
                        stringBuilder.AppendLine($"WiFi module is {(isAccessPoint ? "providing access point" : "connected to access point")}, IP address {ipInfo.Address}");
                    }
                    else
                    {
                        stringBuilder.AppendLine("WiFi module is idle");
                    }
                }
                else
                {
                    stringBuilder.AppendLine("WiFi module is disabled");
                }
            }
            else
            {
                // Ethernet interface
                var configuredIP = await this.dhcp.GetConfiguredIPAddress(iface.Name);
                if (iface.OperationalStatus == OperationalStatus.Up)
                {
                    var actualIP = (from unicastAddress in iface.GetIPProperties().UnicastAddresses
                                    where unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork
                                    select unicastAddress.Address).FirstOrDefault() ?? IPAddress.Any;
                    stringBuilder.AppendLine($"Ethernet is enabled, configured IP address: {configuredIP}, actual IP address: {actualIP}");
                }
                else
                {
                    stringBuilder.AppendLine($"Ethernet is disabled, configured IP address: {configuredIP}, actual IP address: 0.0.0.0");
                }
            }

            return stringBuilder.ToString();
        }
    }
}