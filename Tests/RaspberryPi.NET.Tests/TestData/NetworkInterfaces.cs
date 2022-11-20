using System.Collections.Generic;
using System.Net.NetworkInformation;
using Moq;
using RaspberryPi.Network;

namespace RaspberryPi.Tests.TestData
{
    internal static class NetworkInterfaces
    {
        internal static Mock<INetworkInterface> GetNetworkInterfaceMock(string name)
        {
            var networkInterfaceMock = new Mock<INetworkInterface>();
            networkInterfaceMock.SetupGet(i => i.Name)
                .Returns(name);
            networkInterfaceMock.SetupGet(i => i.OperationalStatus)
                .Returns(OperationalStatus.Up);

            return networkInterfaceMock;
        }
        
        internal static Mock<INetworkInterface> GetLoopback()
        {
            return GetNetworkInterfaceMock("lo");
        }
        
        internal static Mock<INetworkInterface> GetWlan0()
        {
            return GetNetworkInterfaceMock("wlan0");
        }
        
        internal static Mock<INetworkInterface> GetEth0()
        {
            return GetNetworkInterfaceMock("eth0");
        }

        internal static IEnumerable<Mock<INetworkInterface>> GetRapsberryPi4Interfaces()
        {
            yield return GetLoopback();
            yield return GetWlan0();
            yield return GetEth0();
        }

        internal static IEnumerable<Mock<INetworkInterface>> GetRapsberryPiZero2Interfaces()
        {
            yield return GetLoopback();
            yield return GetWlan0();
        }
    }
}
