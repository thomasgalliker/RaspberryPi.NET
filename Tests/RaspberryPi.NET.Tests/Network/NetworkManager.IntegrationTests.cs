using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using RaspberryPi.Network;
using RaspberryPi.Process;
using RaspberryPi.Services;
using RaspberryPi.Storage;
using RaspberryPi.Tests.Logging;
using RaspberryPi.Tests.TestData;
using RaspberryPi.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace RaspberryPi.Tests.Network
{
    public class NetworkManagerIntegrationTests
    {
        private readonly AutoMocker autoMocker;
        private readonly ITestOutputHelper testOutputHelper;

        public NetworkManagerIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;

            this.autoMocker = new AutoMocker();

            this.autoMocker.Use<ILogger<NetworkManager>>(new TestOutputHelperLogger<NetworkManager>(testOutputHelper));
            this.autoMocker.Use<ILogger<NullProcessRunner>>(new TestOutputHelperLogger<NullProcessRunner>(testOutputHelper));
            this.autoMocker.Use<ILogger<DHCP>>(new TestOutputHelperLogger<DHCP>(testOutputHelper));
            this.autoMocker.Use<ILogger<WPA>>(new TestOutputHelperLogger<WPA>(testOutputHelper));
            this.autoMocker.Use<ILogger<AccessPoint>>(new TestOutputHelperLogger<AccessPoint>(testOutputHelper));

            var rootPath = Path.GetFullPath(".");
            this.autoMocker.Use<IFileSystem>(new FileSystem(new TestFile(rootPath), new TestDirectory(rootPath), new TestFileStreamFactory(rootPath)));
            this.autoMocker.Use<IProcessRunner>(this.autoMocker.CreateInstance<NullProcessRunner>());
            this.autoMocker.Use<IDHCP>(this.autoMocker.CreateInstance<DHCP>());
            this.autoMocker.Use<IWPA>(this.autoMocker.CreateInstance<WPA>());
            this.autoMocker.Use<IAccessPoint>(this.autoMocker.CreateInstance<AccessPoint>());

            var networkInterfaceMocks = NetworkInterfaces.GetNetworkInterfaceMocks();
            var networkInterfaceServiceMock = this.autoMocker.GetMock<INetworkInterfaceService>();
            networkInterfaceServiceMock.Setup(n => n.GetAllNetworkInterfaces())
                .Returns(networkInterfaceMocks.Select(n => n.Object));
        }

        [Fact]
        public async Task ShouldSetupAccessPoint()
        {
            // Arrange
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var dhcp = this.autoMocker.CreateInstance<DHCP>();

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var networkManager = this.autoMocker.CreateInstance<NetworkManager>();

            var ssid = "testssid";
            var psk = "testpassword";
            var ipAddress = IPAddress.Parse("192.168.50.1");
            var channel = 6;
            var country = Countries.Switzerland;

            // Act
            await networkManager.SetupAccessPoint(ifaceMock.Object, ssid, psk, ipAddress, channel, country);

            // Assert
            var isAPConfigured = await dhcp.IsAPConfiguredAsync();
            isAPConfigured.Should().BeTrue();

            systemCtlMock.Verify(s => s.RestartService(DHCP.DhcpcdService), Times.Once);
            systemCtlMock.Verify(s => s.RestartService(AccessPoint.HostapdServiceName), Times.Once);
            systemCtlMock.Verify(s => s.RestartService(AccessPoint.DnsmasqServiceName), Times.Once);
            //systemCtlMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSetupStationMode()
        {
            // Arrange
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var dhcp = this.autoMocker.CreateInstance<DHCP>();

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var networkManager = this.autoMocker.CreateInstance<NetworkManager>();

            var network = new WPASupplicantNetwork
            {
                SSID = "testssid",
                PSK = "testpassword"
            };

            // Act
            await networkManager.SetupStationMode(ifaceMock.Object, network);

            // Assert
            var isAPConfigured = await dhcp.IsAPConfiguredAsync();
            isAPConfigured.Should().BeFalse();

            systemCtlMock.Verify(s => s.RestartService(WPA.WpaSupplicantService), Times.Once);
            //systemCtlMock.VerifyNoOtherCalls();
        }
    }
}