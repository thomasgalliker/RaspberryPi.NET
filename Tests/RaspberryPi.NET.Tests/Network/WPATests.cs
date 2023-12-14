using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using RaspberryPi.Extensions;
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
            fileSystemMock.Setup(f => f.Directory.Exists(Path.GetDirectoryName(WPA.WpaSupplicantConfFilePath)))
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
        }

        [Fact]
        public void ShouldGetConnectedSSIDs()
        {
            // Arrange
            var iwgetid = Files.GetIwgetid();

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand("iwgetid -r", It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult(0, iwgetid, null));

            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            var ssids = wpa.GetConnectedSSIDs();

            // Assert
            ssids.Should().HaveCount(1);
            ssids.ElementAt(0).Should().Be("testssid");
        }

        [Fact]
        public async Task ShouldGetSSIDs()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();

            var wpaSupplicantConfStream = Files.GetWPASupplicantConf_Example1_Stream();
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
        public async Task ShouldGetWPASupplicantConfAsync()
        {
            // Arrange
            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();

            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => Files.GetWPASupplicantConf_Example1_Stream());


            fileSystemMock.Setup(f => f.File.Exists(WPA.WpaSupplicantConfFilePath))
                .Returns(true);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var wpa = this.autoMocker.CreateInstance<WPA>();

            // Act
            var wpaSupplicantConf = await wpa.GetWPASupplicantConfAsync();

            // Assert
            this.testOutputHelper.WriteLine(ObjectDumper.Dump(wpaSupplicantConf, DumpStyle.CSharp));

            wpaSupplicantConf.Should().NotBeNull();
            wpaSupplicantConf.APScan.Should().Be(1);
            wpaSupplicantConf.Country.Should().NotBeNull();
            wpaSupplicantConf.Country.Alpha2.Should().Be("CH");
            wpaSupplicantConf.Networks.Should().HaveCount(1);
            wpaSupplicantConf.Networks.ElementAt(0).SSID.Should().Be("testssid");
            wpaSupplicantConf.Networks.ElementAt(0).PSK.Should().Be("testpassword");
        }

        [Fact]
        public async Task ShouldSetWPASupplicantConfAsync()
        {
            // Arrange

            var wpaSupplicantConfMemoryStreamInput = Files.GetWPASupplicantConf_Example2_Stream();
            var wpaSupplicantConfMemoryStreamOutput = new MemoryStream();

            //var wpaSupplicantConfStream = new StringBuilderStream(Files.GetWPASupplicantConf_Example2_Stream());

            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.SetupSequence(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => wpaSupplicantConfMemoryStreamInput.Rewind())
                .Returns(() => wpaSupplicantConfMemoryStreamOutput.Rewind());
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Create, FileAccess.Write))
                .Returns(() => wpaSupplicantConfMemoryStreamOutput);


            fileSystemMock.Setup(f => f.File.Exists(WPA.WpaSupplicantConfFilePath))
                .Returns(true);

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            var systemCtlMock = this.autoMocker.GetMock<ISystemCtl>();

            var wpa = this.autoMocker.CreateInstance<WPA>();
            var wpaSupplicantConf = await wpa.GetWPASupplicantConfAsync();
            wpaSupplicantConf.Country = Countries.Germany;
            wpaSupplicantConf.APScan = 0;
            wpaSupplicantConf.Networks = new[]
            {
                new WPASupplicantNetwork
                {
                    SSID = "newssid",
                    PSK = "newpassword",
                }
            };

            // Act
            await wpa.SetWPASupplicantConfAsync(wpaSupplicantConf);

            // Assert
            var fileContent = wpaSupplicantConfMemoryStreamOutput.GetString();

            this.testOutputHelper.WriteLine(
                $"{Environment.NewLine}" +
                $"{fileContent}");

            fileContent.Should().Be(
                "ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev\r\n" +
                "ap_scan=0\r\n" +
                "update_config=1\r\n" +
                "country=DE\r\n" +
                "\r\n" +
                "network={\r\n" +
                "\tssid=\"newssid\"\r\n" +
                "\tpsk=f83c37fdabe8ff446a9093eecfa70adfc2bb3dfb5c2ab5baebcba0dcacac1a56\r\n" +
                "}\r\n" +
                "\r\n");
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
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => wpaSupplicantConfStream);
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Create, FileAccess.Write))
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
                "\tpsk=c790cda2aa5be23e3808d2ecf42a9d8d22a1fb8a7210d5dda74feed125252be0\r\n" +
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

            var wpaSupplicantConfStream = new StringBuilderStream(Files.GetWPASupplicantConf_Example1_Stream());
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Open, FileAccess.Read))
                .Returns(() => wpaSupplicantConfStream);
            fileSystemMock.Setup(f => f.FileStreamFactory.Create(WPA.WpaSupplicantConfFilePath, FileMode.Create, FileAccess.Write))
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
                "\tpsk=7c73efd9deb6f95aa08e98b1503d57d967f8ae3ad4f60f0b1d2aad00f3f81937\r\n" +
                "\tkey_mgmt=WPA-PSK\r\n" +
                "}\r\n" +
                "\r\n" +
                "network={\r\n" +
                "\tssid=\"testssid_update\"\r\n" +
                "\tpsk=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\r\n" +
                "}\r\n" +
                "\r\n");
        }
    }
}