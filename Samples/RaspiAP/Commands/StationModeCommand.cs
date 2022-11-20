using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaspberryPi;
using RaspberryPi.Network;

namespace RaspiAP.Commands
{
    public class StationModeCommand : Command
    {
        private static readonly Argument<string> ArgumentSSID = new Argument<string>("ssid", "the ssid");
        private static readonly Argument<string> ArgumentPassphrase = new Argument<string>("passphrase", "the passphrase");

        public StationModeCommand(
            ILogger<StationModeCommand> logger,
            INetworkManager networkManager,
            INetworkInterfaceService networkInterfaceService)
            : base(name: "--client", "Configures a wifi client")
        {
            this.AddAlias("-c");
            this.AddAlias("--c");
            this.AddArgument(ArgumentSSID);
            this.AddArgument(ArgumentPassphrase);
            this.Handler = new StationModeCommandHandler(logger, networkManager, networkInterfaceService);
            this.AddOption(CommonOptions.CountryOption);
        }

        private class StationModeCommandHandler : ICommandHandler
        {
            private readonly ILogger<StationModeCommand> logger;
            private readonly INetworkManager networkManager;
            private readonly INetworkInterfaceService networkInterfaceService;

            public StationModeCommandHandler(
                ILogger<StationModeCommand> logger,
                INetworkInterfaceService nugetClient)
            {
                this.logger = logger;
                this.networkInterfaceService = nugetClient ?? throw new ArgumentNullException(nameof(nugetClient));
            }

            public StationModeCommandHandler(ILogger<StationModeCommand> logger, INetworkManager networkManager, INetworkInterfaceService networkInterfaceService)
            {
                this.logger = logger;
                this.networkManager = networkManager;
                this.networkInterfaceService = networkInterfaceService;
            }

            public int Invoke(InvocationContext context)
            {
                return 0;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                var ssid = context.ParseResult.GetValueForArgument(ArgumentSSID);
                var passphrase = context.ParseResult.GetValueForArgument(ArgumentPassphrase);
                var countryCode = context.ParseResult.GetValueForOption(CommonOptions.CountryOption);
                var country = !string.IsNullOrEmpty(countryCode) ? Countries.FromAlpha2(countryCode) : Countries.Switzerland;

                var network = new WPASupplicantNetwork
                {
                    SSID = ssid,
                    PSK = passphrase,
                };

                INetworkInterface iface;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    iface = this.networkInterfaceService.GetAll()
                        .FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && i.OperationalStatus == OperationalStatus.Up);
                }
                else
                {
                    iface = this.networkInterfaceService.GetByName("wlan0");
                }

                await this.networkManager.SetupStationMode(iface, network, country);

                return 0;
            }
        }
    }
}