using System.Net;
using System.Threading.Tasks;

namespace RaspberryPi.Network
{
    public interface IInterface
    {
        Task ManageGateway(INetworkInterface iface, IPAddress gateway, IPAddress dnsServer);

        Task ManageNetmask(INetworkInterface iface, IPAddress netmask);

        Task<string> ReportAsync(INetworkInterface iface);

        Task SetConfig(INetworkInterface iface, object pParam, object sParam);
    }
}