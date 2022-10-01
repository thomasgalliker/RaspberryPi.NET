using System.Collections.Generic;
using System.Net.NetworkInformation;
using Moq;
using RaspberryPi.Network;

namespace RaspberryPi.Tests.TestData
{
    internal static class NetworkInterfaces
    {
        internal static IEnumerable<Mock<INetworkInterface>> GetNetworkInterfaceMocks()
        {
            var networkInterfaceWlan0 = new Mock<INetworkInterface>();
            networkInterfaceWlan0.SetupGet(i => i.Name)
                .Returns("wlan0");
            networkInterfaceWlan0.SetupGet(i => i.OperationalStatus)
                .Returns(OperationalStatus.Up);
            yield return networkInterfaceWlan0;

            var networkInterfaceEth0 = new Mock<INetworkInterface>();
            networkInterfaceEth0.SetupGet(i => i.Name)
                .Returns("eth0");
            networkInterfaceEth0.SetupGet(i => i.OperationalStatus)
                .Returns(OperationalStatus.Up);
            yield return networkInterfaceEth0;
        }
    }
}
