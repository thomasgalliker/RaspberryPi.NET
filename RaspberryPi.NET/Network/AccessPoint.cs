using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaspberryPi.Extensions;
using RaspberryPi.Process;
using RaspberryPi.Services;
using RaspberryPi.Storage;

namespace RaspberryPi.Network
{
    /// <summary>
    /// Functions for access point mode
    /// </summary>
    public class AccessPoint : IAccessPoint
    {
        private static readonly string[] NewLineChars = new string[] { "\n", "\r\n" };
        private const int PskMinLength = 8;
        private const int PskMaxLength = 64;
        internal const string AccessPointServiceName = "accesspoint@.service";
        internal const string HostapdServiceName = "hostapd";
        internal const string HostapdConfFilePath = "/etc/hostapd/hostapd.conf";
        internal const string DnsmasqServiceName = "dnsmasq";
        internal const string DnsmasqConfFilePath = "/etc/dnsmasq.conf";
        private const string DefaultChannel = "acs_survey";

        private readonly ILogger logger;
        private readonly IProcessRunner processRunner;
        private readonly ISystemCtl systemCtl;
        private readonly IServiceConfigurator serviceConfigurator;
        private readonly IWPA wpa;
        private readonly IDHCP dhcp;
        private readonly IFileSystem fileSystem;
        private readonly INetworkInterfaceService networkInterfaceService;

        public AccessPoint(
            ILogger<AccessPoint> logger,
            IProcessRunner processRunner,
            ISystemCtl systemCtl,
            IServiceConfigurator serviceConfigurator,
            IWPA wpa,
            IDHCP dhcp,
            IFileSystem fileSystem,
            INetworkInterfaceService networkInterfaceService)
        {
            this.logger = logger;
            this.processRunner = processRunner;
            this.systemCtl = systemCtl;
            this.serviceConfigurator = serviceConfigurator;
            this.wpa = wpa;
            this.dhcp = dhcp;
            this.fileSystem = fileSystem;
            this.networkInterfaceService = networkInterfaceService;
        }

        /// <inheritdoc/>
        public bool IsEnabled()
        {
            return
                this.fileSystem.File.Exists(HostapdConfFilePath) &&
                this.fileSystem.File.Exists(DnsmasqConfFilePath) &&
                this.systemCtl.IsActive(HostapdServiceName) &&
                this.systemCtl.IsActive(DnsmasqServiceName);
        }

        public CommandLineResult TestDnsmasq()
        {
            var testResult = this.processRunner.TryExecuteCommand($"dnsmasq --test -C {DnsmasqConfFilePath}");
            return testResult;
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            this.logger.LogDebug("StartAsync");

            this.EnsureHostapdConfigurationExists();
            this.EnsureDnsmaqConfigurationExists();
            await this.EnsureDhcpIsConfigured();

            if (!this.systemCtl.IsEnabled(HostapdServiceName))
            {
                this.systemCtl.UnmaskService(HostapdServiceName);
                this.systemCtl.EnableService(HostapdServiceName);
            }

            if (!this.systemCtl.IsEnabled(DnsmasqServiceName))
            {
                this.systemCtl.UnmaskService(DnsmasqServiceName);
                this.systemCtl.EnableService(DnsmasqServiceName);
            }

            if (!this.systemCtl.IsActive(HostapdServiceName))
            {
                this.systemCtl.StartService(HostapdServiceName);
            }

            if (!this.systemCtl.IsActive(DnsmasqServiceName))
            {
                this.systemCtl.StartService(DnsmasqServiceName);
            }
        }

        /// <inheritdoc/>
        public async Task RestartAsync()
        {
            this.logger.LogDebug("RestartAsync");

            this.EnsureHostapdConfigurationExists();
            this.EnsureDnsmaqConfigurationExists();
            await this.EnsureDhcpIsConfigured();

            if (!this.systemCtl.IsEnabled(HostapdServiceName))
            {
                this.systemCtl.UnmaskService(HostapdServiceName);
                this.systemCtl.EnableService(HostapdServiceName);
            }

            if (!this.systemCtl.IsEnabled(DnsmasqServiceName))
            {
                this.systemCtl.UnmaskService(DnsmasqServiceName);
                this.systemCtl.EnableService(DnsmasqServiceName);
            }

            this.systemCtl.RestartService(HostapdServiceName);
            this.systemCtl.RestartService(DnsmasqServiceName);
        }

        /// <inheritdoc/>
        public void Stop()
        {
            this.logger.LogDebug("Stop");

            if (this.systemCtl.IsActive(HostapdServiceName))
            {
                this.systemCtl.StopService(HostapdServiceName);
            }

            if (this.systemCtl.IsActive(DnsmasqServiceName))
            {
                this.systemCtl.StopService(DnsmasqServiceName);
            }

            if (this.systemCtl.IsEnabled(HostapdServiceName))
            {
                this.systemCtl.DisableService(HostapdServiceName);
            }

            if (this.systemCtl.IsEnabled(DnsmasqServiceName))
            {
                this.systemCtl.DisableService(DnsmasqServiceName);
            }
        }

        private void EnsureHostapdConfigurationExists()
        {
            if (!this.fileSystem.File.Exists(HostapdConfFilePath))
            {
                throw new InvalidOperationException($"No hostapd configuration found, use {nameof(ConfigureAsync)} method to configure the access point first"); // M589
            }
        }

        private void EnsureDnsmaqConfigurationExists()
        {
            if (!this.fileSystem.File.Exists(DnsmasqConfFilePath))
            {
                throw new InvalidOperationException($"No dnsmasq configuration found, use {nameof(ConfigureAsync)} method to configure the access point first"); // M589
            }
        }

        private async Task EnsureDhcpIsConfigured()
        {
            if (!await this.dhcp.IsAPConfiguredAsync())
            {
                throw new InvalidOperationException($"No access point configuration found. Use {nameof(ConfigureAsync)} method to configure the access point first."); // M587
            }
        }

        /// <summary>
        /// Configure this devices as access point.
        /// </summary>
        /// <param name="ssid">SSID to use.</param>
        /// <param name="psk">Password to use.</param>
        /// <param name="ipAddress">IP address.</param>
        /// <param name="channel">The wifi channel number. Automatically selected if null.</param>
        /// <param name="country">The country in which this access point operates. If null, country code is read from wifi configuration.</param>
        /// <returns></returns>
        public async Task ConfigureAsync(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, INetworkInterface[] noDhcpInterfaces = null, int? channel = null, Country country = null)
        {
            if (iface == null)
            {
                throw new ArgumentNullException(nameof(iface), $"Parameter {nameof(iface)} must not be null.");
            }

            if (string.IsNullOrEmpty(ssid))
            {
                throw new ArgumentNullException(nameof(ssid), $"Parameter {nameof(ssid)} must not be null or empty.");
            }

            if (string.IsNullOrEmpty(psk))
            {
                throw new ArgumentNullException(nameof(psk), $"Parameter {nameof(psk)} must not be null or empty.");
            }

            if (psk.Length is < PskMinLength or > PskMaxLength)
            {
                throw new ArgumentNullException(nameof(psk), $"Parameter {nameof(psk)} must be between {PskMinLength} and {PskMaxLength} characters.");
            }

            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress), $"Parameter {nameof(ipAddress)} must not be null.");
            }

            if (ipAddress == IPAddress.Any || ipAddress == IPAddress.Broadcast || ipAddress == IPAddress.None || ipAddress == IPAddress.Loopback)
            {
                throw new ArgumentNullException(nameof(ipAddress), $"Parameter {nameof(ipAddress)} is not valid.");
            }

            string countryCode;

            if (country != null)
            {
                countryCode = country.Alpha2;
            }
            else
            {
                var wpaSupplicantConf = await this.wpa.GetWPASupplicantConfAsync();
                countryCode = wpaSupplicantConf.Country.Alpha2;
            }

            if (string.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentNullException(nameof(country), "Cannot configure access point because no country code has been set.");
            }

            var channelString = $"{channel}";
            if (channel == null)
            {
                channelString = DefaultChannel;
            }

            this.logger.LogDebug($"ConfigureAsync: ssid={ssid}, psk={{suppressed}}, ipAddress={ipAddress}, channel={channelString}, country={countryCode}");

            this.logger.LogDebug($"ConfigureAsync: Writing dnsmasq config --> {DnsmasqConfFilePath}...");

            var dnsmasqConfDir = Path.GetDirectoryName(DnsmasqConfFilePath);
            if (!this.fileSystem.Directory.Exists(dnsmasqConfDir))
            {
                this.fileSystem.Directory.CreateDirectory(dnsmasqConfDir);
            }

            using (var dnsmasqTemplateStream = Configurations.GetDnsmasqTemplateStream())
            {
                using var reader = new StreamReader(dnsmasqTemplateStream);
                using var writer = this.fileSystem.FileStreamFactory.CreateStreamWriter(DnsmasqConfFilePath, FileMode.Create, FileAccess.Write);

                // TODO: Configurable: Range size, subnet mask, dhcp leas duration

                var ipAddressBytes = ipAddress.GetAddressBytes();
                var dhcpRangeStart = $"{ipAddressBytes[0]}.{ipAddressBytes[1]}.{ipAddressBytes[2]}.{(ipAddressBytes[3] is < 100 or > 150 ? 100 : 151)}";
                var dhcpRangeEnd = $"{ipAddressBytes[0]}.{ipAddressBytes[1]}.{ipAddressBytes[2]}.{(ipAddressBytes[3] is < 100 or > 150 ? 150 : 200)}";
                var dhcpRangeSubnet = IPAddress.Parse("255.255.255.0");
                var dhcpLeaseDuration = "24h";

                if (noDhcpInterfaces == null)
                {
                    var physicalName = iface.GetPhysicalName();
                    var allNetworkInterfaces = this.networkInterfaceService.GetAll();
                    noDhcpInterfaces = allNetworkInterfaces.Where(i => i.Name != physicalName).ToArray();
                }

                var interfaceValue = $"lo,{iface.Name}";
                var noDhcpInterfaceValue = noDhcpInterfaces != null ? ConvertToCsvString(noDhcpInterfaces) : null;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    line = line.Replace("{interface}", interfaceValue);
                    line = line.Replace("{no-dhcp-interface}", noDhcpInterfaceValue);
                    line = line.Replace("{dhcpRangeStart}", dhcpRangeStart);
                    line = line.Replace("{dhcpRangeEnd}", dhcpRangeEnd);
                    line = line.Replace("{dhcpRangeSubnet}", dhcpRangeSubnet.ToString());
                    line = line.Replace("{dhcpLeaseDuration}", dhcpLeaseDuration);
                    line = line.Replace("{ipAddress}", ipAddress.ToString());
                    await writer.WriteLineAsync(line);
                }
            }

            this.logger.LogDebug($"ConfigureAsync: Writing hostapd config --> {HostapdConfFilePath}...");

            var hostapdConfDir = Path.GetDirectoryName(HostapdConfFilePath);
            if (!this.fileSystem.Directory.Exists(hostapdConfDir))
            {
                this.fileSystem.Directory.CreateDirectory(hostapdConfDir);
            }

            using (var hostapdTemplateStream = Configurations.GetHostapdTemplateStream())
            {
                using var reader = new StreamReader(hostapdTemplateStream);
                using var writer = this.fileSystem.FileStreamFactory.CreateStreamWriter(HostapdConfFilePath, FileMode.Create, FileAccess.Write);

                var hwMode = "g"; // TODO: Make hw_mode configurable

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    line = line.Replace("{interface}", iface.Name);
                    line = line.Replace("{ssid}", ssid);
                    line = line.Replace("{hw_mode}", hwMode);
                    line = line.Replace("{country_code}", countryCode);
                    line = line.Replace("{channel}", channelString);
                    line = line.Replace("{psk}", psk);
                    await writer.WriteLineAsync(line);
                }
            }

            //this.processRunner.TryExecuteCommand($"sudo chmod 600 {HostapdConfFilePath}");

            var serviceDefinition = GetServiceDefinition(iface);
            this.serviceConfigurator.ReinstallService(serviceDefinition);

            this.processRunner.TryExecuteCommand("sudo systemctl disable wpa_supplicant.service");

            this.processRunner.TryExecuteCommand("sudo systemctl unmask dnsmasq.service");
            this.processRunner.TryExecuteCommand("sudo systemctl enable dnsmasq.service");
            this.processRunner.TryExecuteCommand("sudo systemctl stop hostapd");
            this.processRunner.TryExecuteCommand("sudo systemctl disable hostapd");
            this.processRunner.TryExecuteCommand("sudo systemctl enable accesspoint@wlan0.service");
            this.processRunner.TryExecuteCommand("sudo rfkill unblock wlan");
            this.processRunner.TryExecuteCommand("sudo systemctl daemon-reload");

            this.logger.LogDebug($"ConfigureAsync for ssid={ssid} finished successfully");
        }

        private static ServiceDefinition GetServiceDefinition(INetworkInterface iface)
        {
            var virtualName = iface.GetVirtualName();
            var hostapdPID = "/run/hostapd.pid";

            return new ServiceDefinition(AccessPointServiceName)
            {
                Description = $"IEEE 802.11 {virtualName}@%i AP on %i with hostapd",
                Wants = new[]
                {
                    "wpa_supplicant@%i.service"
                },
                Type = ServiceType.Forking,
                PIDFile = hostapdPID,
                Restart = ServiceRestart.OnFailure,
                RestartSec = 2,
                Environments = new[]
                {
                   $"DAEMON_CONF={HostapdConfFilePath}"
                },
                EnvironmentFile = "-/etc/default/hostapd",
                ExecStartPre = $"/sbin/iw dev %i interface add {virtualName}@%i type __ap",
                ExecStart = $"/usr/sbin/hostapd -i {virtualName}@%i -P {hostapdPID} -B {HostapdConfFilePath}",
                ExecStopPost = $"-/sbin/iw dev {virtualName}@%i del",
                WantedBy = new[]
                {
                    "sys-subsystem-net-devices-%i.device"
                },
            };
        }

        private static string ConvertToCsvString(INetworkInterface[] noDhcpInterfaces)
        {
            return string.Join(",", noDhcpInterfaces.Select(i => i.Name));
        }

        public async Task DeleteConfigurationAsync(INetworkInterface iface)
        {
            // Try to stop the access point
            this.Stop();

            // Delete configuration files again
            var fileDeleted = false;
            if (this.fileSystem.File.Exists(HostapdConfFilePath))
            {
                this.fileSystem.File.Delete(HostapdConfFilePath);
                fileDeleted = true;
            }

            if (this.fileSystem.File.Exists(DnsmasqConfFilePath))
            {
                this.fileSystem.File.Delete(DnsmasqConfFilePath);
                fileDeleted = true;
            }

            if (fileDeleted)
            {
                // Reset IP address configuration to station mode
                await this.dhcp.SetIPAddressAsync(iface, IPAddress.Any, null, null, null);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ConnectedAccessPointClient> GetConnectedClients(INetworkInterface iface)
        {
            if (iface == null)
            {
                throw new ArgumentNullException($"Parameter '{nameof(iface)}' must not be null", nameof(iface));
            }

            var commandLineResult = this.processRunner.ExecuteCommand($"sudo iw dev {iface.Name} station dump");

            var clients = new List<ConnectedAccessPointClient>();
            var regex = new Regex(@$"Station (?<MacAddress>[0-9a-fA-F:]{{17}}) \(on {iface.Name}\)");

            var split = regex.Split(commandLineResult.OutputData).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for (var i = 0; i < split.Length; i += 2)
            {
                var macAddress = split[i].Replace(":", "-");
                var content = split[i + 1];

                var contentRegex = new Regex(@"\s*(?<PropertyName>.*):(\s*)(?<PropertyValue>.*)");
                var matches = contentRegex.Matches(content).OfType<Match>();
                
                var connectedTimeSpan = TimeSpan.FromSeconds(double.Parse(RegexExtensions.ParseValue(matches, "connected time").Replace(" seconds", "")));

                clients.Add(new ConnectedAccessPointClient
                {
                    MacAddress = PhysicalAddress.Parse(macAddress),
                    RxBitrate = RegexExtensions.ParseValue(matches, "rx bitrate"),
                    TxBitrate = RegexExtensions.ParseValue(matches, "tx bitrate"),
                    Authorized = RegexExtensions.ParseValueYesNo(matches, "authorized"),
                    Authenticated = RegexExtensions.ParseValueYesNo(matches, "authenticated"),
                    ConnectedTime = connectedTimeSpan, 
                });
            }

            return clients;
        }
    }
}