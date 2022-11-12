using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using RaspberryPi.Network;
using RaspberryPi.Process;
using RaspberryPi.Services;
using RaspberryPi.Storage;
using RaspberryPi.Tests.Logging;
using RaspberryPi.Tests.TestData;
using Xunit;
using Xunit.Abstractions;

namespace RaspberryPi.Tests.Network
{
    public class DHCPTests
    {
        private readonly AutoMocker autoMocker;
        private readonly ITestOutputHelper testOutputHelper;

        public DHCPTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;

            this.autoMocker = new AutoMocker();
            this.autoMocker.Use<ILogger<DHCP>>(new TestOutputHelperLogger<DHCP>(testOutputHelper));

            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.File.Exists(DHCP.DhcpcdConfFilePath))
                .Returns(true);

            fileSystemMock.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string p, string c) => testOutputHelper.WriteLine(
                    $"WriteAllText:{Environment.NewLine}" +
                    $"{p}{Environment.NewLine}" +
                    $"{c}"));

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(It.IsAny<CommandLineInvocation>(), It.IsAny<CancellationToken>()))
                .Returns(CommandLineResult.SuccessResult);
        }

        [Fact]
        public async Task ShouldSetIPAddress_ForAccessPoint()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.FileStreamFactory.CreateStreamReader(DHCP.DhcpcdConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => new StreamReader(Files.GetDhcpcdConfStream()));

            var configStream = new MemoryStream();
            await Files.GetDhcpcdConfStream().CopyToAsync(configStream);
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(DHCP.DhcpcdConfFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                .Returns(() => configStream);

            var networkInterfaceMocks = NetworkInterfaces.GetNetworkInterfaceMocks();
            var networkInterfaceMock = this.autoMocker.GetMock<INetworkInterfaceService>();
            networkInterfaceMock.Setup(n => n.GetAllNetworkInterfaces())
                .Returns(networkInterfaceMocks.Select(m => m.Object));

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var ip = IPAddress.Parse("192.168.1.50");
            var netmask = IPAddress.Parse("255.255.255.0");
            var gateway = IPAddress.Parse("192.168.1.1");
            var dnsServer = IPAddress.Parse("192.168.1.2");
            var forAP = true;

            var dhcp = this.autoMocker.CreateInstance<DHCP>();

            // Act
            await dhcp.SetIPAddressAsync(ifaceMock.Object, ip, netmask, gateway, dnsServer, forAP);

            // Assert
            fileSystemMock.Verify(f => f.File.Exists(DHCP.DhcpcdConfFilePath), Times.Once);
            fileSystemMock.Verify(f => f.FileStreamFactory.CreateStreamReader(DHCP.DhcpcdConfFilePath, FileMode.Open, FileAccess.Read), Times.Once);
            fileSystemMock.Verify(f => f.FileStreamFactory.Create(DHCP.DhcpcdConfFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite), Times.Once);
            fileSystemMock.VerifyNoOtherCalls();

            processRunnerMock.VerifyNoOtherCalls();

            systemCtlMock.Verify(s => s.RestartService(DHCP.DhcpcdService), Times.Once);
            systemCtlMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSetIPAddress_NotForAccessPoint()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.FileStreamFactory.CreateStreamReader(DHCP.DhcpcdConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => new StreamReader(Files.GetDhcpcdConfStream()));

            var configStream = new MemoryStream();
            await Files.GetDhcpcdConfStream().CopyToAsync(configStream);
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(DHCP.DhcpcdConfFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                .Returns(() => configStream);

            var networkInterfaceMocks = NetworkInterfaces.GetNetworkInterfaceMocks();
            var networkInterfaceServiceMock = this.autoMocker.GetMock<INetworkInterfaceService>();
            networkInterfaceServiceMock.Setup(n => n.GetAllNetworkInterfaces())
                .Returns(networkInterfaceMocks.Select(m => m.Object));

            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var ip = IPAddress.Parse("192.168.1.50");
            var netmask = IPAddress.Parse("255.255.255.0");
            var gateway = IPAddress.Parse("192.168.1.1");
            var dnsServer = IPAddress.Parse("192.168.1.2");
            var forAP = (bool?)null;

            var dhcp = this.autoMocker.CreateInstance<DHCP>();

            // Act
            await dhcp.SetIPAddressAsync(ifaceMock.Object, ip, netmask, gateway, dnsServer, forAP);

            // Assert
            fileSystemMock.Verify(f => f.File.Exists(DHCP.DhcpcdConfFilePath), Times.Once);
            fileSystemMock.Verify(f => f.FileStreamFactory.CreateStreamReader(DHCP.DhcpcdConfFilePath, FileMode.Open, FileAccess.Read), Times.Once);
            fileSystemMock.Verify(f => f.FileStreamFactory.Create(DHCP.DhcpcdConfFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite), Times.Once);
            fileSystemMock.VerifyNoOtherCalls();

            networkInterfaceServiceMock.Verify(p => p.GetAllNetworkInterfaces(), Times.Once);
            networkInterfaceServiceMock.Verify(p => p.SetLinkDown(It.Is<INetworkInterface>(i => i.Name == "wlan0")), Times.Once);
            networkInterfaceServiceMock.Verify(p => p.SetLinkUp(It.Is<INetworkInterface>(i => i.Name == "wlan0")), Times.Once);
            networkInterfaceServiceMock.VerifyNoOtherCalls();

            systemCtlMock.VerifyNoOtherCalls();
        }
    }
}