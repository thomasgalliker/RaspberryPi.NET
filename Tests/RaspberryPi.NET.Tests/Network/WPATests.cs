using System;
using System.IO;
using System.Linq;
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
using RaspberryPi.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace RaspberryPi.Tests.Network
{
    public class WPATests
    {
        private readonly AutoMocker autoMocker;
        private readonly ITestOutputHelper testOutputHelper;

        public WPATests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;

            this.autoMocker = new AutoMocker();
            this.autoMocker.Use<ILogger<WPA>>(new TestOutputHelperLogger<WPA>(testOutputHelper));

            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
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
        }

        [Fact]
        public async Task ShouldGetSSIDs()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();

            var wpaSupplicantConfStream = Files.GetWPASupplicantConfStream();
            var wpaSupplicantConfStreamReader = new StreamReader(wpaSupplicantConfStream);
            fileSystemMock.Setup(f => f.FileStreamFactory.CreateStreamReader(WPA.WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => wpaSupplicantConfStreamReader);

            fileSystemMock.Setup(f => f.File.Exists(WPA.WpaSupplicantConfFilePath))
                .Returns(true);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            var ssids = await wpa.GetSSIDsAsync();

            // Assert
            ssids.Should().HaveCount(1);
            ssids.ElementAt(0).Should().Be("testssid");
        }

        [Fact]
        public void ShouldScanSSIDs()
        {
            // Arrange
            var iwlistWlan0Scan = Files.GetIwlistWlan0Scan();

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand("sudo iwlist wlan0 scan", It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult(0, iwlistWlan0Scan, null));

            var ifaceMock = NetworkInterfaces.GetWlan0();

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            var ssids = wpa.ScanSSIDs(ifaceMock.Object);

            // Assert
            ssids.Should().HaveCount(2);
            ssids.ElementAt(0).Should().Be("MyNetwork1");
            ssids.ElementAt(1).Should().Be("MyNetwork2");
        }
        
        [Fact]
        public async Task ShouldGetCountryCode()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();

            var wpaSupplicantConfStream = Files.GetWPASupplicantConfStream();
            var wpaSupplicantConfStreamReader = new StreamReader(wpaSupplicantConfStream);
            fileSystemMock.Setup(f => f.FileStreamFactory.CreateStreamReader(WPA.WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => wpaSupplicantConfStreamReader);

            fileSystemMock.Setup(f => f.File.Exists(WPA.WpaSupplicantConfFilePath))
                .Returns(true);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            var countryCode = await wpa.GetCountryCodeAsync();

            // Assert
            countryCode.Should().Be("CH");
        }

        [Fact]
        public async Task ShouldAddOrUpdateNetwork_FromDefaultConfig()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.Directory.Exists(Path.GetDirectoryName(WPA.WpaSupplicantConfFilePath)))
                .Returns(true);
            fileSystemMock.Setup(f => f.File.Exists(WPA.WpaSupplicantConfFilePath))
                .Returns(true);

            var wpaSupplicantConfStream = new StringBuilderStream("");
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                .Returns(() => wpaSupplicantConfStream);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var network = new WPASupplicantNetwork
            {
                SSID = "testssid_update",
                PSK = "testpassword_update",
            };

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            await wpa.AddOrUpdateNetworkAsync(network);

            // Assert
            var fileContent = wpaSupplicantConfStream.ToString();

            this.testOutputHelper.WriteLine(
                $"{Environment.NewLine}" +
                $"{fileContent}");

            fileContent.Should().Be(
                "ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev\r\n" +
                "ap_scan=1\r\n" +
                "update_config=1\r\n" +
                "country=CH\r\n" +
                "\r\n" +
                "network={\r\n" +
                "\tssid=\"testssid_update\"\r\n" +
                "\tpsk=\"testpassword_update\"\r\n" +
                "}\r\n" +
                "\r\n");
        }

        [Fact]
        public async Task ShouldAddOrUpdateNetwork_FromExistingConfig()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.Directory.Exists(Path.GetDirectoryName(WPA.WpaSupplicantConfFilePath)))
                .Returns(false);
            fileSystemMock.Setup(f => f.File.Exists(WPA.WpaSupplicantConfFilePath))
                .Returns(true);

            var wpaSupplicantConfStream = new StringBuilderStream(Files.GetWPASupplicantConfStream());

            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                .Returns(() => wpaSupplicantConfStream);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var network = new WPASupplicantNetwork
            {
                SSID = "testssid_update",
                PSK = new string('A', 64),
            };

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            await wpa.AddOrUpdateNetworkAsync(network);

            // Assert
            var fileContent = wpaSupplicantConfStream.ToString();

            this.testOutputHelper.WriteLine(
                $"{Environment.NewLine}" +
                $"{fileContent}");

            fileContent.Should().Be(
                "ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev\r\n" +
                "ap_scan=1\r\n" +
                "update_config=1\r\n" +
                "country=CH\r\n" +
                "\r\n" +
                "network={\r\n" +
                "\tssid=\"testssid\"\r\n" +
                "\tpsk=\"testpassword\"\r\n" +
                "\tkey_mgmt=WPA-PSK\r\n" +
                "}\r\n" +
                "\r\n" +
                "network={\r\n" +
                "\tssid=\"testssid_update\"\r\n" +
                "\tpsk=2ee5dcaf42116dfdb78bf0a59aebb1caf5c0b7c77b574200956736b59b1a243c\r\n" +
                "}\r\n" +
                "\r\n");
        }
    }
}