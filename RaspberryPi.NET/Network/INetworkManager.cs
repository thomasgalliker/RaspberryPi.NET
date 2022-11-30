using System.Net;
using System.Threading.Tasks;

namespace RaspberryPi.Network
{
    public interface INetworkManager
    {
        //Task SetupAccessPoint(AccessPointConfiguration accessPointConfiguration);

        //Task SetupAccessPoint(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country);
        
        Task SetupAccessPoint2(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country);
        
        Task SetupStationMode(INetworkInterface iface, WPASupplicantNetwork network, Country country = null);
    }
}