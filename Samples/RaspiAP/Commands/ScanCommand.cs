using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RaspberryPi.Network;

namespace RaspiAP.Commands
{
    public class ScanCommand : Command
    {
        public ScanCommand(
            IWPA wpa,
            INetworkInterfaceService networkInterfaceService)
            : base(name: "--scan", "Scans for SSIDs")
        {
            this.Handler = new ScanCommandHandler(wpa, networkInterfaceService);
        }

        private class ScanCommandHandler : ICommandHandler
        {
            private readonly IWPA wpa;
            private readonly INetworkInterfaceService networkInterfaceService;

            public ScanCommandHandler(
                IWPA wpa,
                INetworkInterfaceService networkInterfaceService)
            {
                this.wpa = wpa;
                this.networkInterfaceService = networkInterfaceService;
            }

            public int Invoke(InvocationContext context)
            {
                return 0;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
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

                var ssids = this.wpa.ScanSSIDs(iface).ToList();
                Console.WriteLine(string.Join(Environment.NewLine, ssids.Select(s => $"{s}")));

                return 0;
            }
        }
    }
}