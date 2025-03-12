using System.Net;
using System.Threading.Tasks;

namespace RaspberryPi.Network
{
    public interface INetworkManager
    {
        //Task SetupAccessPoint(AccessPointConfiguration accessPointConfiguration);

        //Task SetupAccessPoint(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country);
        
        Task SetupAccessPointAsync(INetworkInterface iface, string ssid, string psk, IPAddress ipAddress, int? channel, Country country);
        
        Task SetupStationModeAsync(INetworkInterface iface, WPASupplicantNetwork network, Country country = null);

        Task ConnectToWifiNetworkAsync(INetworkInterface iface, WPASupplicantNetwork network, Country country = null);

        Task RemoveWifiNetworkAsync(INetworkInterface iface, string ssid);
    }
}