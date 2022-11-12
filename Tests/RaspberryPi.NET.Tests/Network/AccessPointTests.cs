using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
    public class AccessPointTests
    {
        private readonly AutoMocker autoMocker;
        private readonly ITestOutputHelper testOutputHelper;

        public AccessPointTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.Use<ILogger<AccessPoint>>(new TestOutputHelperLogger<AccessPoint>(testOutputHelper));

            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.Directory.Exists(Path.GetDirectoryName(AccessPoint.HostapdConfFilePath)))
                .Returns(true);
            fileSystemMock.Setup(f => f.Directory.Exists(Path.GetDirectoryName(AccessPoint.DnsmasqConfFilePath)))
                .Returns(true);
            fileSystemMock.Setup(f => f.File.Exists("/bin/bash"))
                .Returns(true);

            fileSystemMock.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string p, string c) => testOutputHelper.WriteLine(
                    $"WriteAllText:{Environment.NewLine}" +
                    $"{p}{Environment.NewLine}" +
                    $"{c}"));

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(It.IsAny<CommandLineInvocation>(), It.IsAny<CancellationToken>()))
                .Returns(CommandLineResult.SuccessResult);
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task ShouldStart()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.File.Exists(AccessPoint.HostapdConfFilePath))
                .Returns(true);
            fileSystemMock.Setup(f => f.File.Exists(AccessPoint.DnsmasqConfFilePath))
                .Returns(true);

            var dhcpMock = this.autoMocker.GetMock<IDHCP>();
            dhcpMock.Setup(d => d.IsAPConfiguredAsync())
                .ReturnsAsync(true);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var accessPoint = this.autoMocker.CreateInstance<AccessPoint>();

            // Act
            await accessPoint.StartAsync();

            // Assert
            fileSystemMock.Verify(f => f.File.Exists(AccessPoint.HostapdConfFilePath), Times.Once);
            fileSystemMock.Verify(f => f.File.Exists(AccessPoint.DnsmasqConfFilePath), Times.Once);
            fileSystemMock.VerifyNoOtherCalls();

            dhcpMock.Verify(d => d.IsAPConfiguredAsync(), Times.Once);
            dhcpMock.VerifyNoOtherCalls();

            processRunnerMock.VerifyNoOtherCalls();

            systemCtlMock.Verify(s => s.IsEnabled("hostapd"), Times.Once);
            systemCtlMock.Verify(s => s.IsActive("hostapd"), Times.Once);
            systemCtlMock.Verify(s => s.StartService("hostapd"), Times.Once);
            systemCtlMock.Verify(s => s.EnableService("hostapd"), Times.Once);
            systemCtlMock.Verify(s => s.UnmaskService("hostapd"), Times.Once);

            systemCtlMock.Verify(s => s.IsEnabled("dnsmasq"), Times.Once);
            systemCtlMock.Verify(s => s.IsActive("dnsmasq"), Times.Once);
            systemCtlMock.Verify(s => s.StartService("dnsmasq"), Times.Once);
            systemCtlMock.Verify(s => s.EnableService("dnsmasq"), Times.Once);
            systemCtlMock.Verify(s => s.UnmaskService("dnsmasq"), Times.Once);

            systemCtlMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldConfigure()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();

            var hostapdStreamWriterMock = new StreamWriterMock("hostapd.conf");
            fileSystemMock.Setup(f => f.FileStreamFactory.CreateStreamWriter(AccessPoint.HostapdConfFilePath, FileMode.Create, FileAccess.Write))
                .Returns(() => hostapdStreamWriterMock.Object);

            var dnsmasqStreamWriterMock = new StreamWriterMock("dnsmasq.conf");
            fileSystemMock.Setup(f => f.FileStreamFactory.CreateStreamWriter(AccessPoint.DnsmasqConfFilePath, FileMode.Create, FileAccess.Write))
                .Returns(() => dnsmasqStreamWriterMock.Object);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();
            var wpaMock = this.autoMocker.GetMock<IWPA>();
            wpaMock.Setup(w => w.GetConfigAsync())
                .ReturnsAsync(WPASupplicantConfs.GetWPASupplicantConf_testssid());

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var accessPoint = this.autoMocker.CreateInstance<AccessPoint>();

            // Act
            await accessPoint.ConfigureAsync(ifaceMock.Object, "testssid", "testpassword", IPAddress.Parse("192.168.50.100"), 6);

            // Assert
            fileSystemMock.Verify(f => f.FileStreamFactory.CreateStreamWriter(AccessPoint.HostapdConfFilePath, FileMode.Create, FileAccess.Write), Times.Once);
            fileSystemMock.Verify(f => f.FileStreamFactory.CreateStreamWriter(AccessPoint.DnsmasqConfFilePath, FileMode.Create, FileAccess.Write), Times.Once);
            fileSystemMock.Verify(f => f.Directory.Exists(@"\etc"), Times.Once);
            fileSystemMock.Verify(f => f.Directory.Exists(@"\etc\hostapd"), Times.Once);
            fileSystemMock.VerifyNoOtherCalls();

            processRunnerMock.Verify(p => p.ExecuteCommand("sudo rfkill unblock wlan", It.IsAny<CancellationToken>()), Times.Once);
            processRunnerMock.VerifyNoOtherCalls();

            wpaMock.Verify(w => w.GetConfigAsync(), Times.Once);
            wpaMock.VerifyNoOtherCalls();

            systemCtlMock.VerifyNoOtherCalls();

            this.testOutputHelper.WriteLine(
                $"{Environment.NewLine}" +
                $"{hostapdStreamWriterMock.GetSummary()}");

            this.testOutputHelper.WriteLine(
                $"{Environment.NewLine}" +
                $"{dnsmasqStreamWriterMock.GetSummary()}");

            hostapdStreamWriterMock.Verify(a => a.WriteLineAsync("ssid=testssid"), Times.Once);
            hostapdStreamWriterMock.Verify(a => a.WriteLineAsync("country_code=CH"), Times.Once);
            hostapdStreamWriterMock.Verify(a => a.WriteLineAsync("channel=6"), Times.Once);
            hostapdStreamWriterMock.Verify(a => a.WriteLineAsync("wpa_passphrase=testpassword"), Times.Once);

            dnsmasqStreamWriterMock.Verify(a => a.WriteLineAsync("interface=wlan0"), Times.Once);
            dnsmasqStreamWriterMock.Verify(a => a.WriteLineAsync("no-dhcp-interface=eth0"), Times.Once);
            dnsmasqStreamWriterMock.Verify(a => a.WriteLineAsync("dhcp-range=192.168.50.151,192.168.50.200,255.255.255.0,24h"), Times.Once);
            dnsmasqStreamWriterMock.Verify(a => a.WriteLineAsync("dhcp-option=option:dns-server,192.168.50.100"), Times.Once);
        }

        [Fact]
        public void ShouldGetConnectedClients()
        {
            // Arrange
            var stationDump = Files.GetIwDevWlan0StationDump();

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand("sudo iw dev wlan0 station dump", It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult(stationDump));

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var accessPoint = this.autoMocker.CreateInstance<AccessPoint>();

            // Act
            var connectedClients = accessPoint.GetConnectedClients(ifaceMock.Object);

            // Assert
            connectedClients.Should().HaveCount(3);
            connectedClients.ElementAt(0).MacAddress.ToString().Should().Be("086AC54CFFCC");
            connectedClients.ElementAt(1).MacAddress.ToString().Should().Be("FE38D8447A1E");
            connectedClients.ElementAt(2).MacAddress.ToString().Should().Be("CED5493CBE9F");
        }

    }
}