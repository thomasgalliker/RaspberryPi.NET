using System.Collections.Generic;

namespace RaspberryPi.Network
{
    public interface INetworkInterfaceService
    {
        IEnumerable<INetworkInterface> GetAllNetworkInterfaces();
    }
}