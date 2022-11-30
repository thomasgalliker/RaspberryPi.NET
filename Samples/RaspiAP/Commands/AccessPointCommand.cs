using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RaspberryPi;
using RaspberryPi.Network;

namespace RaspiAP.Commands
{
    public class AccessPointCommand : Command
    {
        private static readonly Argument<string> ArgumentSSID = new Argument<string>("ssid", "the ssid");
        private static readonly Argument<string> ArgumentPassphrase = new Argument<string>("passphrase", "the passphrase");

        public AccessPointCommand(
            ILogger<AccessPointCommand> logger,
            INetworkManager networkManager,
            INetworkInterfaceService networkInterfaceService)
            : base(name: "--ap", "Configures an access point")
        {
            this.AddAlias("-ap");
            this.AddAlias("--accesspoint");
            this.AddArgument(ArgumentSSID);
            this.AddArgument(ArgumentPassphrase);
            this.Handler = new AccessPointCommandHandler(logger, networkManager, networkInterfaceService);
            this.AddOption(CommonOptions.CountryOption);
        }

        private class AccessPointCommandHandler : ICommandHandler
        {
            private readonly ILogger<AccessPointCommand> logger;
            private readonly INetworkManager networkManager;
            private INetworkInterfaceService networkInterfaceService;

            public AccessPointCommandHandler(
                ILogger<AccessPointCommand> logger,
                INetworkManager networkManager)
            {
                this.logger = logger;
                this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            }

            public AccessPointCommandHandler(ILogger<AccessPointCommand> logger, INetworkManager networkManager, INetworkInterfaceService networkInterfaceService) : this(logger, networkManager)
            {
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
                var ipAddress = IPAddress.Parse("192.168.99.1");
                var channel = 6;

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

                await this.networkManager.SetupAccessPoint(iface, ssid, passphrase, ipAddress, channel, country);

                return 0;
            }
        }
    }
}