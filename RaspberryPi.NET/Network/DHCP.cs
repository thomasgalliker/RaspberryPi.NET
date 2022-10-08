﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaspberryPi.Process;
using RaspberryPi.Services;
using RaspberryPi.Storage;

namespace RaspberryPi.Network
{
    /// <summary>
    /// Functions for IP address management via dhcpcd
    /// </summary>
    public class DHCP : IDHCP
    {
        internal const string DhcpdService = "dhcpcd.service";
        internal const string DhcpdConfFilePath = "/etc/dhcpcd.conf";

        /// <summary>
        /// Regex to capture the interface name
        /// </summary>
        private static readonly Regex IfaceRegex = new(@"^\s*interface\s+(\w+)");

        /// <summary>
        /// Regex to capture the configured IP address
        /// </summary>
        private static readonly Regex IpRegex = new(@"^\s*static\s+ip_address=(\d+\.\d+\.\d+\.\d+)(?:/(\d+))?");

        /// <summary>
        /// Regex to capture the configured gateway
        /// </summary>
        private static readonly Regex RoutersRegex = new(@"^static\s+routers=(?:.*\s+)?(\d+\.\d+\.\d+\.\d+)");

        /// <summary>
        /// Regex to capture the configured DNS server
        /// </summary>
        private static readonly Regex DnsServerRegex = new(@"^static\s+domain_name_servers=(?:.*\s+)?(\d+\.\d+\.\d+\.\d+)");

        /// <summary>
        /// Regex to detect if the interface config is intended for AP mode
        /// </summary>
        private static readonly Regex ApModeRegex = new(@"^\s*nohook\s+wpa_supplicant");

        private readonly ILogger<DHCP> logger;
        private readonly IProcessRunner processRunner;
        private readonly ISystemCtl systemCtl;
        private readonly IFileSystem fileSystem;
        private readonly INetworkInterfaceService networkInterface;

        public DHCP(
            ILogger<DHCP> logger,
            IProcessRunner processRunner,
            ISystemCtl systemCtl,
            IFileSystem fileSystem,
            INetworkInterfaceService networkInterface)
        {
            this.logger = logger;
            this.processRunner = processRunner;
            this.systemCtl = systemCtl;
            this.fileSystem = fileSystem;
            this.networkInterface = networkInterface;
        }

        /// <summary>
        /// Private class representing the IP address configuration as saved in the dhcpcd config
        /// </summary>
        private sealed class IPConfig
        {
            /// <summary>
            /// Name of the interface
            /// </summary>
            public string Interface { get; set; }

            /// <summary>
            /// IP address
            /// </summary>
            public IPAddress IP { get; set; }

            /// <summary>
            /// Gateway
            /// </summary>
            public IPAddress Gateway { get; set; }

            /// <summary>
            /// Subnet mask
            /// </summary>
            public IPAddress Subnet { get; set; }

            /// <summary>
            /// Get or set the subnet mask in CIDR notation
            /// </summary>
            public int CIDR
            {
                get
                {
                    var cidr = 0;
                    var subnetMask = BitConverter.ToUInt32(this.Subnet.GetAddressBytes(), 0);
                    for (var i = 0; i < 32; i++)
                    {
                        if ((subnetMask & (1u << i)) != 0)
                        {
                            cidr++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return cidr;
                }
                set
                {
                    if (value > 32)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    uint subnetMask = 0;
                    for (var i = 0; i < value; i++)
                    {
                        subnetMask <<= 1;
                        subnetMask |= 1u;
                    }
                    this.Subnet = new IPAddress(subnetMask);
                }
            }

            /// <summary>
            /// DNS server
            /// </summary>
            public IPAddress DNSServer { get; set; }

            /// <summary>
            /// Set to true if the configuration is intended for AP mode
            /// </summary>
            public bool ForAP { get; set; }
        }

        /// <summary>
        /// Update the IP address of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <param name="ip">IP address or null if unchanged</param>
        /// <param name="netmask">Subnet mask or null if unchanged</param>
        /// <param name="gateway">Gateway or null if unchanged</param>
        /// <param name="netmask">Subnet mask or null if unchanged</param>
        /// <param name="dnsServer">Set IP address for AP mode</param>
        /// <returns>Asynchronous task</returns>
        public async Task SetIPAddressAsync(string iface, IPAddress ip, IPAddress netmask, IPAddress gateway, IPAddress dnsServer, bool? forAP = null)
        {
            this.logger.LogDebug($"SetIPAddressAsync: iface={iface}, ip={ip}, netmask={netmask}, gateway={gateway}, dnsServer={dnsServer}, forAP={forAP}");

            // Check if the profile already exists and if anything is supposed to change
            var profiles = await this.ReadProfiles();
            IPConfig existingProfile = null;
            foreach (var profile in profiles)
            {
                if (profile.Interface == iface)
                {
                    if ((ip == null || ip == profile.IP) &&
                        (netmask == null || netmask == profile.Subnet) &&
                        (gateway == null || gateway == profile.Gateway) &&
                        (dnsServer == null || dnsServer == profile.DNSServer) &&
                        (forAP == null || forAP == profile.ForAP))
                    {
                        // Config remains unchanged; no need to rewrite the config
                    }
                    existingProfile = profile;
                }
            }

            if (IPAddress.Any.Equals(ip))
            {
                // DHCP config
                if (existingProfile == null)
                {
                    // It is and will remain enabled; no need to rewrite the config
                }
            }
            else if (existingProfile != null && !existingProfile.ForAP && forAP != true)
            {
                // Static config - replace missing settings with parsed settings from the old config
                ip ??= existingProfile.IP;
                netmask ??= existingProfile.Subnet;
                gateway ??= existingProfile.Gateway;
                dnsServer ??= existingProfile.DNSServer;
            }

            // Rewrite the network config
            await this.UpdateProfile(iface, ip, netmask, gateway, dnsServer, forAP ?? false);

            // Restart dhcpcd if the AP config may have changed
            if (forAP != null)
            {
                this.systemCtl.RestartService(DhcpdService);
            }

            // Restart Ethernet adapter if it is up to apply the new configuration
            var networkInterfaces = this.networkInterface.GetAllNetworkInterfaces();
            if (networkInterfaces.Any(item => item.Name == iface && item.OperationalStatus == OperationalStatus.Up) &&
                forAP == null)
            {
                this.processRunner.ExecuteCommand($"ip link set {iface} down");
                this.processRunner.ExecuteCommand($"ip link set {iface} up");
            }
        }

        /// <summary>
        /// Read the current network profiles
        /// </summary>
        /// <returns>List of configured profiles</returns>
        private async Task<List<IPConfig>> ReadProfiles()
        {
            List<IPConfig> result = new();

            if (this.fileSystem.File.Exists(DhcpdConfFilePath))
            {
                using var reader = this.fileSystem.FileStreamFactory.CreateStreamReader(DhcpdConfFilePath, FileMode.Open, FileAccess.Read);

                IPConfig item = null;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var match = IfaceRegex.Match(line);
                    if (match.Success)
                    {
                        if (item != null)
                        {
                            result.Add(item);
                        }

                        // Interface name
                        item = new IPConfig() { Interface = match.Groups[1].Value };
                    }
                    else if (item != null)
                    {
                        match = IpRegex.Match(line);
                        if (match.Success)
                        {
                            // IP address
                            item.IP = IPAddress.Parse(match.Groups[1].Value);
                            if (match.Groups.Count == 3)
                            {
                                // Subnet mask (CIDR)
                                item.CIDR = int.Parse(match.Groups[2].Value);
                            }
                        }
                        else
                        {
                            // Gateway
                            match = RoutersRegex.Match(line);
                            if (match.Success)
                            {
                                item.Gateway = IPAddress.Parse(match.Groups[1].Value);
                            }
                            else
                            {
                                // DNS server
                                match = DnsServerRegex.Match(line);
                                if (match.Success)
                                {
                                    item.DNSServer = IPAddress.Parse(match.Groups[1].Value);
                                }
                                else
                                {
                                    // AP mode
                                    if (ApModeRegex.IsMatch(line))
                                    {
                                        item.ForAP = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (item != null)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Update a network profile
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <param name="ip">IP address or null if unset</param>
        /// <param name="subnet">Subnet mask or null if unset</param>
        /// <param name="gateway">Gateway or null if unset</param>
        /// <param name="subnet">Subnet mask or null if unset</param>
        /// <param name="dnsServer">DNS server or null if unset</param>
        /// <param name="forAP">Add extra option for AP mode</param>
        /// <returns>Asynchronous task</returns>
        private async Task UpdateProfile(string iface, IPAddress ip, IPAddress subnet, IPAddress gateway, IPAddress dnsServer, bool forAP)
        {
            if (iface == null)
            {
                throw new ArgumentNullException(nameof(iface));
            }

            using var configStream = this.fileSystem.FileStreamFactory.Create(DhcpdConfFilePath, FileMode.Open, FileAccess.ReadWrite);
            //using FileStream configStream = new(DhcpdConfFilePath, FileMode.Open, FileAccess.ReadWrite);
            using MemoryStream newConfigStream = new();
            using (var reader = new StreamReader(configStream, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                using var writer = new StreamWriter(newConfigStream, Encoding.UTF8, 1024, leaveOpen: true);
                async Task WriteProfile(bool writeEmptyLine)
                {
                    // Write the interface section only if it isn't meant to be configured by DHCP
                    if (!IPAddress.Any.Equals(ip) && (ip != null || gateway != null || dnsServer != null))
                    {
                        if (writeEmptyLine)
                        {
                            // Write empty line between the last line and the following profile
                            await writer.WriteLineAsync();
                        }

                        await writer.WriteLineAsync($"interface {iface}");
                        if (ip != null)
                        {
                            int? cidr = null;
                            if (subnet != null)
                            {
                                var subnetMask = BitConverter.ToUInt32(subnet.GetAddressBytes(), 0);
                                for (var i = 0; i < 32; i++)
                                {
                                    if ((subnetMask & (1u << i)) != 0)
                                    {
                                        cidr ??= 0;
                                        cidr++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            await writer.WriteLineAsync($"static ip_address={ip}/{cidr ?? 24}");
                            if (forAP)
                            {
                                await writer.WriteLineAsync("nohook wpa_supplicant");
                            }
                        }
                        if (gateway != null && !IPAddress.Any.Equals(gateway))
                        {
                            await writer.WriteLineAsync($"static routers={gateway}");
                        }
                        if (dnsServer != null && !IPAddress.Any.Equals(dnsServer))
                        {
                            await writer.WriteLineAsync($"static domain_name_servers={dnsServer}");
                        }
                    }
                }

                // Rewrite the config line by line
                bool lastLineEmpty = true, profileWritten = false;
                string line = null, currentInterface = null;
                while (!reader.EndOfStream)
                {
                    line = await reader.ReadLineAsync();

                    // Is this the first line of a new profile?
                    var match = IfaceRegex.Match(line);
                    if (match.Success)
                    {
                        if (currentInterface == iface)
                        {
                            // Profile is being changed from the one we want to modify, write the updated profile now
                            await WriteProfile(!lastLineEmpty);
                            profileWritten = true;
                        }
                        currentInterface = match.Groups[1].Value;
                    }

                    // Write empty lines, comments, and sections which don't belong to the profile that is supposed to be updated
                    if (currentInterface != iface || line.TrimStart().StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    {
                        await writer.WriteLineAsync(line);
                    }

                    // Only write empty line delimiters if the last line before the profile to be written was not empty
                    if (currentInterface != iface)
                    {
                        lastLineEmpty = string.IsNullOrWhiteSpace(line);
                    }
                }

                if (!profileWritten)
                {
                    // Write profile now, it hasn't been written yet
                    await WriteProfile(!lastLineEmpty);
                }
            }

            // Overwrite the previous config
            configStream.Seek(0, SeekOrigin.Begin);
            configStream.SetLength(newConfigStream.Length);
            newConfigStream.Seek(0, SeekOrigin.Begin);
            await newConfigStream.CopyToAsync(configStream);
        }

        /// <summary>
        /// Get the configured IP address of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured IP address or 0.0.0.0 if not configured</returns>
        public async Task<IPAddress> GetConfiguredIPAddress(string iface)
        {
            var profiles = await this.ReadProfiles();
            var ifaceProfile = profiles.FirstOrDefault(profile => profile.Interface == iface);
            return ifaceProfile == null ? IPAddress.Any : ifaceProfile.IP;
        }

        /// <summary>
        /// Get the configured netmask of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured netmask or 0.0.0.0 if not configured</returns>
        public async Task<IPAddress> GetConfiguredNetmask(string iface)
        {
            var profiles = await this.ReadProfiles();
            var ifaceProfile = profiles.FirstOrDefault(profile => profile.Interface == iface);
            return ifaceProfile == null ? IPAddress.Any : ifaceProfile.Subnet;
        }

        /// <summary>
        /// Get the configured gateway of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured gateway or 0.0.0.0 if not configured</returns>
        public async Task<IPAddress> GetConfiguredGateway(string iface)
        {
            var profiles = await this.ReadProfiles();
            var ifaceProfile = profiles.FirstOrDefault(profile => profile.Interface == iface);
            return ifaceProfile == null ? IPAddress.Any : ifaceProfile.Gateway;
        }

        /// <summary>
        /// Get the configured DNS server of the given network interface
        /// </summary>
        /// <param name="iface">Name of the network interface</param>
        /// <returns>Configured DNS server or 0.0.0.0 if not configured</returns>
        public async Task<IPAddress> GetConfiguredDNSServer(string iface)
        {
            var profiles = await this.ReadProfiles();
            var ifaceProfile = profiles.FirstOrDefault(profile => profile.Interface == iface);
            return ifaceProfile == null ? IPAddress.Any : ifaceProfile.DNSServer;
        }

        /// <summary>
        /// Check if there is at least one DHCP profile for AP mode
        /// </summary>
        /// <returns>Whether there is a DHCP profile for AP mode</returns>
        public async Task<bool> IsAPConfigured()
        {
            var profiles = await this.ReadProfiles();
            return profiles.Any(profile => profile.ForAP);
        }
    }
}