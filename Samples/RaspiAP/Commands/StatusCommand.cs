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
    public class StatusCommand : Command
    {
        public StatusCommand(
            IAccessPoint accessPoint,
            INetworkInterfaceService networkInterfaceService)
            : base(name: "--status", "Reports the status of the current network configuration")
        {
            this.Handler = new StatusCommandHandler(accessPoint, networkInterfaceService);
        }

        private class StatusCommandHandler : ICommandHandler
        {
            private readonly IAccessPoint accessPoint;
            private readonly INetworkInterfaceService networkInterfaceService;

            public StatusCommandHandler(
                IAccessPoint accessPoint,
                INetworkInterfaceService networkInterfaceService)
            {
                this.accessPoint = accessPoint;
                this.networkInterfaceService = networkInterfaceService;
            }

            public int Invoke(InvocationContext context)
            {
                return 0;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                foreach (var iface in this.networkInterfaceService.GetAll())
                {
                    var connectedClients = this.accessPoint.GetConnectedClients(iface).ToList();
                    Console.WriteLine($"{iface.Name}: # connected clients: {connectedClients.Count}");
                    foreach (var connectedClient in connectedClients)
                    {
                        Console.WriteLine($"client: MacAddress={connectedClient.MacAddress}, connected time: {connectedClient.ConnectedTime}");
                    }
                }

                return 0;
            }
        }
    }
}