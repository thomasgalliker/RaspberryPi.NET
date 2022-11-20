using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaspberryPi.Extensions;
using RaspberryPi.Internals;
using RaspberryPi.Process;
using RaspberryPi.Services;
using RaspberryPi.Storage;
using RaspberryPi.Utils;

namespace RaspberryPi.Network
{
    /// <summary>
    /// Functions for WiFi network management via wpa_supplicant
    /// </summary>
    public class WPA : IWPA
    {
        public const string WpaSupplicantService = "wpa_supplicant.service";
        public const string WpaSupplicantConfFilePath = "/etc/wpa_supplicant/wpa_supplicant.conf";
        private const int PskMinLength = 8;
        private const int PskMaxLength = 64;
        private const string ESSID = "ESSID:\"";
        private const int FileBufferSize = 1024;
        private static readonly string[] NewLineChars = new string[] { "\n", "\r\n" };

        private readonly ILogger logger;
        private readonly ISystemCtl systemCtl;
        private readonly IProcessRunner processRunner;
        private readonly IFileSystem fileSystem;
        private readonly INetworkInterfaceService networkInterface;

        public WPA(
            ILogger<WPA> logger,
            ISystemCtl systemCtl,
            IProcessRunner processRunner,
            IFileSystem fileSystem,
            INetworkInterfaceService networkInterface)
        {
            this.logger = logger;
            this.systemCtl = systemCtl;
            this.processRunner = processRunner;
            this.fileSystem = fileSystem;
            this.networkInterface = networkInterface;
        }

        /// <inheritdoc/>
        public void Start()
        {
            this.EnsureWpaSupplicantConf();

            if (!this.systemCtl.IsEnabled(WpaSupplicantService))
            {
                this.systemCtl.EnableService(WpaSupplicantService);
            }

            if (!this.systemCtl.IsActive(WpaSupplicantService))
            {
                this.systemCtl.StartService(WpaSupplicantService);
            }
        }

        /// <inheritdoc/>
        public void Restart()
        {
            this.EnsureWpaSupplicantConf();

            if (!this.systemCtl.IsEnabled(WpaSupplicantService))
            {
                this.systemCtl.EnableService(WpaSupplicantService);
            }

            this.systemCtl.RestartService(WpaSupplicantService);
        }

        private void EnsureWpaSupplicantConf()
        {
            if (!this.fileSystem.File.Exists(WpaSupplicantConfFilePath))
            {
                throw new InvalidOperationException($"No WiFi configuration found. Use {nameof(AddOrUpdateNetworkAsync)} to configure at least one SSID.");
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (this.systemCtl.IsEnabled(WpaSupplicantService))
            {
                this.systemCtl.DisableService(WpaSupplicantService);
            }

            if (this.systemCtl.IsActive(WpaSupplicantService))
            {
                this.systemCtl.StopService(WpaSupplicantService);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSSIDsAsync()
        {
            var ssids = new List<string>();
            if (this.fileSystem.File.Exists(WpaSupplicantConfFilePath))
            {
                using var reader = this.fileSystem.FileStreamFactory.CreateStreamReader(WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read);

                var inNetworkSection = false;
                string ssid = null;
                while (!reader.EndOfStream)
                {
                    var line = (await reader.ReadLineAsync()).TrimStart();
                    if (inNetworkSection)
                    {
                        if (ssid == null)
                        {
                            if (line.StartsWith("ssid="))
                            {
                                var startPosition = "ssid=".Length;
                                var endPosition = line.Length - startPosition;
                                ssid = line.Substring(startPosition, endPosition).Trim(' ', '\t', '"');
                            }
                        }
                        else if (line.StartsWith("}"))
                        {
                            if (ssid != null)
                            {
                                ssids.Add(ssid);
                                ssid = null;
                            }
                            inNetworkSection = false;
                        }
                    }
                    else if (line.StartsWith("network={"))
                    {
                        inNetworkSection = true;
                    }
                }

                if (ssid != null)
                {
                    ssids.Add(ssid);
                }
            }

            return ssids;
        }

        /// <summary>
        /// Report the current WiFi stations
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetReportAsync()
        {
            var ssids = await this.GetSSIDsAsync();
            if (ssids.Any())
            {
                StringBuilder builder = new();

                // List SSIDs
                builder.AppendLine("Remembered networks:");
                foreach (var ssid in ssids)
                {
                    builder.AppendLine(ssid);
                }

                // Current IP address configuration
                var networkInterfaces = this.networkInterface.GetAll();
                foreach (var iface in networkInterfaces)
                {
                    if (iface.OperationalStatus == OperationalStatus.Up && iface.Name.StartsWith("w"))
                    {
                        var ipAddress = (from item in iface.GetIPProperties().UnicastAddresses
                                         where item.Address.AddressFamily == AddressFamily.InterNetwork
                                         select item.Address).FirstOrDefault() ?? IPAddress.Any;
                        var netMask = (from item in iface.GetIPProperties().UnicastAddresses
                                       where item.Address.AddressFamily == AddressFamily.InterNetwork
                                       select item.IPv4Mask).FirstOrDefault() ?? IPAddress.Any;
                        var gateway = (from item in iface.GetIPProperties().GatewayAddresses
                                       where item.Address.AddressFamily == AddressFamily.InterNetwork
                                       select item.Address).FirstOrDefault() ?? IPAddress.Any;
                        var dnsServer = (from item in iface.GetIPProperties().DnsAddresses
                                         where item.AddressFamily == AddressFamily.InterNetwork
                                         select item).FirstOrDefault() ?? IPAddress.Any;
                        builder.AppendLine($"IP={ipAddress} GW={gateway} NM={netMask} DNS={dnsServer}");
                        break;
                    }
                }

                return builder.ToString().Trim();
            }

            // No networks available
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<string> ScanSSIDs(INetworkInterface iface)
        {
            if (iface == null)
            {
                throw new ArgumentNullException($"Parameter '{nameof(iface)}' must not be null", nameof(iface));
            }

            var commandLineResult = this.processRunner.ExecuteCommand($"sudo iwlist {iface.Name} scan");

            return commandLineResult.OutputData.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries)
               .Where(line => line.Contains(ESSID))
               .Select(line => line.Substring(line.IndexOf(ESSID) + ESSID.Length).TrimEnd('"'));
        }

        /// <inheritdoc/>
        public async Task<WPASupplicantConf> GetConfigAsync()
        {
            this.logger.LogDebug("GetConfigAsync");

            // Source: https://github.com/snowdayclub/rpi-wifisetup-ble/blob/5bc236a90fa6c8ccaebef845d62c526fa2b487d2/wpamanager.py

            if (!this.fileSystem.File.Exists(WpaSupplicantConfFilePath))
            {
                return null;
            }

            var conf = new WPASupplicantConf();
            using var configStream = this.fileSystem.FileStreamFactory.Create(WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read);
            {
                using var reader = new StreamReader(configStream, Encoding.UTF8, true, FileBufferSize, leaveOpen: true);
                {
                    var fileContent = await reader.ReadToEndAsync();

                    var keyValueRegex = new Regex(@"^(\s*)(?<Key>[^={}]*)=(?<Value>.*)", RegexOptions.Multiline);
                    var matchesAll = keyValueRegex.Matches(fileContent);

                    if (RegexExtensions.TryParseValue(matchesAll, "ctrl_interface", out var ctrlInterface))
                    {
                        conf.CtrlInterface = ctrlInterface;
                    }

                    if (RegexExtensions.TryParseValue(matchesAll, "ap_scan", out var apscan))
                    {
                        conf.APScan = int.Parse(apscan);
                    }

                    if (RegexExtensions.TryParseValue(matchesAll, "update_config", out var updateConfig))
                    {
                        conf.UpdateConfig = int.Parse(updateConfig);
                    }

                    if (RegexExtensions.TryParseValue(matchesAll, "country", out var country))
                    {
                        conf.Country = Countries.FromAlpha2(country);
                    }

                    var networkSplit = fileContent.Split(new[] { "network=" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var networkSplitItem in networkSplit.Skip(1))
                    {
                        var networkMatches = keyValueRegex.Matches(networkSplitItem);
                        var network = new WPASupplicantNetwork();

                        if (RegexExtensions.TryParseValue(networkMatches, "ssid", out var ssid))
                        {
                            network.SSID = ssid.Replace("\"", "");
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "scan_ssid", out var scanssid))
                        {
                            network.ScanSSID = ConvertIntStringToBool(scanssid);
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "psk", out var psk))
                        {
                            if (psk.StartsWith("\"") && psk.EndsWith("\""))
                            {
                                network.PSK = psk.Substring(1, psk.Length - 2);
                            }
                            else
                            {
                                network.PSK = psk;
                            }
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "key_mgmt", out var keyMgmt))
                        {
                            network.KeyMgmt = keyMgmt;
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "proto", out var proto))
                        {
                            network.Proto = proto;
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "pairwise", out var pairwise))
                        {
                            network.Pairwise = pairwise;
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "auth_alg", out var authAlg))
                        {
                            network.AuthAlg = authAlg;
                        }

                        if (RegexExtensions.TryParseValue(networkMatches, "disabled", out var disabled))
                        {
                            network.Disabled = ConvertIntStringToBool(disabled);
                        }

                        conf.Networks.Add(network);
                    }
                }

                return conf;
            }
        }

        /// <inheritdoc/>
        public async Task SetConfigAsync(WPASupplicantConf conf)
        {
            this.logger.LogDebug("SetConfigAsync");

            if (conf == null)
            {
                throw new ArgumentNullException(nameof(conf), $"Parameter '{nameof(conf)}' must not be null");
            }

            if (conf.Country == null)
            {
                throw new ArgumentNullException($"{nameof(conf)}.{nameof(conf.Country)}", $"Parameter '{nameof(conf)}.{nameof(conf.Country)}' must not be null");
            }

            if (conf.Networks.Any(n => string.IsNullOrEmpty(n.SSID)))
            {
                throw new ArgumentException($"Parameter '{nameof(conf)}.{nameof(conf.Networks)}.{nameof(WPASupplicantNetwork.SSID)}' must not be null or empty", $"{nameof(conf)}.{nameof(conf.Networks)}.{nameof(WPASupplicantNetwork.SSID)}");
            }

            //if (conf.Networks.Any(n => string.IsNullOrEmpty(n.PSK) && n.KeyMgmt != "NONE"))
            //{
            //}

            if (conf.Networks.Any(n => !string.IsNullOrEmpty(n.PSK) && (n.PSK.Length < PskMinLength || n.PSK.Length > PskMaxLength)))
            {
                throw new ArgumentException($"Parameter '{nameof(conf)}.{nameof(conf.Networks)}.{nameof(WPASupplicantNetwork.PSK)}' must be between {PskMinLength} and {PskMaxLength} characters.", $"{nameof(conf)}.{nameof(conf.Networks)}.{nameof(WPASupplicantNetwork.PSK)}");
            }

            var wpaSupplicantDir = Path.GetDirectoryName(WpaSupplicantConfFilePath);
            if (!this.fileSystem.Directory.Exists(wpaSupplicantDir))
            {
                this.fileSystem.Directory.CreateDirectory(wpaSupplicantDir);
            }

            if (this.fileSystem.File.Exists(WpaSupplicantConfFilePath))
            {
                this.fileSystem.File.Delete(WpaSupplicantConfFilePath);
            }

            using var configStream = this.fileSystem.FileStreamFactory.Create(WpaSupplicantConfFilePath, FileMode.Create, FileAccess.Write);
            {
                using var writer = new StreamWriter(configStream, Encodings.UTF8EncodingWithoutBOM, FileBufferSize, leaveOpen: true);
                {
                    await writer.WriteLineAsync($"ctrl_interface={conf.CtrlInterface}");

                    if (conf.APScan is int apscan)
                    {
                        await writer.WriteLineAsync($"ap_scan={apscan}");
                    }

                    if (conf.UpdateConfig is int updateConfig)
                    {
                        await writer.WriteLineAsync($"update_config={updateConfig}");
                    }

                    await writer.WriteLineAsync($"country={conf.Country.Alpha2}");
                    await writer.WriteLineAsync();

                    if (conf.Networks != null)
                    {
                        foreach (var network in conf.Networks)
                        {
                            await writer.WriteLineAsync("network={");
                            await writer.WriteLineAsync($"\tssid=\"{network.SSID}\"");

                            if (network.ScanSSID)
                            {
                                await writer.WriteLineAsync($"\tscan_ssid={ConvertBoolToIntString(network.ScanSSID)}");
                            }

                            if (!string.IsNullOrEmpty(network.PSK))
                            {
                                if (network.PSK.Length < 64)
                                {
                                    var pskHash = WPAPassphrase.GetHash(network.SSID, network.PSK);
                                    await writer.WriteLineAsync($"\tpsk={pskHash}");
                                }
                                else
                                {
                                    await writer.WriteLineAsync($"\tpsk={network.PSK}");
                                }
                            }

                            if (!string.IsNullOrEmpty(network.KeyMgmt))
                            {
                                await writer.WriteLineAsync($"\tkey_mgmt={network.KeyMgmt}");
                            }

                            if (!string.IsNullOrEmpty(network.Proto))
                            {
                                await writer.WriteLineAsync($"\tproto={network.Proto}");
                            }

                            if (!string.IsNullOrEmpty(network.Pairwise))
                            {
                                await writer.WriteLineAsync($"\tpairwise={network.Pairwise}");
                            }

                            if (!string.IsNullOrEmpty(network.AuthAlg))
                            {
                                await writer.WriteLineAsync($"\tauth_alg={network.AuthAlg}");
                            }

                            if (network.Disabled)
                            {
                                await writer.WriteLineAsync($"\tdisabled={ConvertBoolToIntString(network.Disabled)}");
                            }

                            await writer.WriteLineAsync("}");
                            await writer.WriteLineAsync();
                        }
                    }
                }
            }

            this.processRunner.ExecuteCommand($"sudo chmod 600 {WpaSupplicantConfFilePath}");

            this.processRunner.ExecuteCommand($"sudo rfkill unblock wifi");

            // Restart the service to apply the new configuration
            this.Restart();

            this.logger.LogDebug($"SetConfigAsync finished successfully");
        }

        public async Task<WPASupplicantNetwork> GetNetworkAsync(string ssid)
        {
            var conf = await this.GetConfigAsync();
            conf ??= new WPASupplicantConf();

            var network = conf.Networks.SingleOrDefault(n => n.SSID == ssid);
            return network;
        }

        /// <inheritdoc/>
        public async Task AddOrUpdateNetworkAsync(WPASupplicantNetwork network)
        {
            this.logger.LogDebug($"AddOrUpdateNetworkAsync: ssid={network.SSID}");

            var conf = await this.GetConfigAsync();
            conf ??= new WPASupplicantConf();

            var existingNetwork = conf.Networks.SingleOrDefault(n => n.SSID == network.SSID);
            if (existingNetwork != null)
            {
                conf.Networks.Remove(existingNetwork);
            }

            conf.Networks.Add(network);

            await this.SetConfigAsync(conf);
        }

        /// <inheritdoc/>
        public async Task RemoveNetworkAsync(WPASupplicantNetwork network)
        {
            this.logger.LogDebug($"RemoveNetworkAsync: ssid={network.SSID}");

            var conf = await this.GetConfigAsync();
            conf ??= new WPASupplicantConf();

            var existingNetwork = conf.Networks.SingleOrDefault(n => n.SSID == network.SSID);
            if (existingNetwork == null)
            {
                throw new InvalidOperationException($"Network with SSID={network.SSID} does not exist.");
            }

            conf.Networks.Remove(existingNetwork);

            await this.SetConfigAsync(conf);
        }

        private static bool ConvertIntStringToBool(string disabled)
        {
            return int.Parse(disabled) == 1 ? true : false;
        }

        private static string ConvertBoolToIntString(bool value)
        {
            return value ? "1" : "0";
        }
    }
}