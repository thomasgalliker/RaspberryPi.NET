using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using RaspberryPi.Process;
using RaspberryPi.Storage;
using RaspberryPi.Tests.Logging;
using RaspberryPi.Tests.TestData;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace RaspberryPi.Tests
{
    public class SystemInfoServiceTests
    {
        private readonly AutoMocker autoMocker;

        public SystemInfoServiceTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.Use<ILogger<SystemInfoService>>(new TestOutputHelperLogger<SystemInfoService>(testOutputHelper));

            var fileSystemMock = this.autoMocker.GetMock<IFileSystem>();
            fileSystemMock.Setup(f => f.File.Exists("/bin/bash"))
                .Returns(true);

            fileSystemMock.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string p, string c) => testOutputHelper.WriteLine(
                    $"WriteAllText:{Environment.NewLine}" +
                    $"{p}{Environment.NewLine}" +
                    $"{c}"));
        }

        [Fact]
        public async Task ShouldGetHostInfo()
        {
            // Arrange
            var cpuInfoTxt = Files.GetHostInfoTxt();
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult(0, cpuInfoTxt, ""));

            var systemInfoService = this.autoMocker.CreateInstance<SystemInfoService>();

            // Act
            var hostInfo = await systemInfoService.GetHostInfoAsync();

            // Assert
            hostInfo.Should().NotBeNull();
            hostInfo.Should().BeEquivalentTo(
                new HostInfo
                {
                    Hostname = "testraspi",
                    MachineId = "cddb04fdadd242d5bd415ac5931050be",
                    BootId = "497cb38084944957a9655ddc57e34625",
                    OperatingSystem = "Raspbian GNU/Linux 11 (bullseye)",
                    Kernel = "Linux 5.15.61-v7+",
                    Architecture = "arm",
                });
        }

        [Fact]
        public async Task ShouldGetCpuInfo()
        {
            // Arrange
            var cpuInfoTxt = Files.GetCpuInfoTxt();
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult(cpuInfoTxt));

            var systemInfoService = this.autoMocker.CreateInstance<SystemInfoService>();

            // Act
            var cpuInfo = await systemInfoService.GetCpuInfoAsync();

            // Assert
            cpuInfo.Should().NotBeNull();
            cpuInfo.Processors.Should().HaveCount(4);
            cpuInfo.Hardware.Should().Be("BCM2835");
            cpuInfo.Revision.Should().Be("902120");
            cpuInfo.Serial.Should().Be("0000000053a77f3d");
            cpuInfo.Model.Should().Be("Raspberry Pi Zero 2 W Rev 1.0");
        }
        
        [Fact]
        public void ShouldGetMemoryInfo()
        {
            // Arrange
            var freeTxt = Files.GetFreeTxt();
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult(freeTxt));

            var systemInfoService = this.autoMocker.CreateInstance<SystemInfoService>();

            // Act
            var memoryInfo = systemInfoService.GetMemoryInfo();

            // Assert
            memoryInfo.Should().NotBeNull();
            memoryInfo.RandomAccessMemory.Should().NotBeNull();
            memoryInfo.Swap.Should().NotBeNull();
        }

        [Fact]
        public void ShouldGetCpuSensorsStatus()
        {
            // Arrange
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(SystemInfoService.MeasureTemp, It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult("temp=34.9'C"));
            processRunnerMock.Setup(p => p.ExecuteCommand(SystemInfoService.MeasureVolts, It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult("volt=1.2500V"));
            processRunnerMock.Setup(p => p.ExecuteCommand(SystemInfoService.GetThrottled, It.IsAny<CancellationToken>()))
                .Returns(new CommandLineResult("throttled=0x50005"));

            var systemInfoService = this.autoMocker.CreateInstance<SystemInfoService>();

            // Act
            var cpuSensorsStatus = systemInfoService.GetCpuSensorsStatus();

            // Assert
            cpuSensorsStatus.Should().BeEquivalentTo(
                new CpuSensorsStatus
                {
                    Temperature = Temperature.FromDegreesCelsius(34.9d),
                    Voltage = ElectricPotential.FromVolts(1.25d),
                    UnderVoltageDetected = true,
                    ArmFrequencyCapped = false,
                    CurrentlyThrottled = true,
                    SoftTemperatureLimitActive = false,
                    UnderVoltageOccurred = true,
                    ArmFrequencyCappingOccurred = false,
                    ThrottlingOccurred = true,
                    SoftTemperatureLimitOccurred = false,
                });
        }
    }
}