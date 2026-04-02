using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using RaspberryPi.Network;
using RaspberryPi.Process;
using RaspberryPi.Tests.Logging;
using Xunit;
using Xunit.Abstractions;

namespace RaspberryPi.Tests.Network
{
    public class NetworkInterfaceServiceTests
    {
        private readonly AutoMocker autoMocker;

        public NetworkInterfaceServiceTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.Use<ILogger<NetworkInterfaceService>>(new TestOutputHelperLogger<NetworkInterfaceService>(testOutputHelper));

            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();
            processRunnerMock.Setup(p => p.ExecuteCommand(It.IsAny<CommandLineInvocation>(), It.IsAny<CancellationToken>()))
                .Returns(CommandLineResult.SuccessResult);
        }

        [Fact]
        public void ShouldSetLinkUp()
        {
            // Arrange
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();

            var ifaceMock = new Mock<INetworkInterface>();
            ifaceMock.SetupGet(i => i.Name)
                .Returns("wlan0");

            var networkInterfaceService = this.autoMocker.CreateInstance<NetworkInterfaceService>();

            // Act
            networkInterfaceService.SetLinkUp(ifaceMock.Object);

            // Assert
            processRunnerMock.Verify(p => p.ExecuteCommand("sudo ip link set wlan0 up", CancellationToken.None), Times.Once);
            processRunnerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldSetLinkDown()
        {
            // Arrange
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();

            var ifaceMock = new Mock<INetworkInterface>();
            ifaceMock.SetupGet(i => i.Name)
                .Returns("wlan0");

            var networkInterfaceService = this.autoMocker.CreateInstance<NetworkInterfaceService>();

            // Act
            networkInterfaceService.SetLinkDown(ifaceMock.Object);

            // Assert
            processRunnerMock.Verify(p => p.ExecuteCommand("sudo ip link set wlan0 down", CancellationToken.None), Times.Once);
            processRunnerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldSetMacAddress()
        {
            // Arrange
            var processRunnerMock = this.autoMocker.GetMock<IProcessRunner>();

            var ifaceMock = new Mock<INetworkInterface>();
            ifaceMock.SetupGet(i => i.Name)
                .Returns("wlan0");
            ifaceMock.SetupGet(i => i.OperationalStatus)
                .Returns(OperationalStatus.Up);

            var macAddress = PhysicalAddress.Parse("00-11-22-33-44-55");

            var networkInterfaceService = this.autoMocker.CreateInstance<NetworkInterfaceService>();

            // Act
            networkInterfaceService.SetMacAddress(ifaceMock.Object, macAddress);

            // Assert
            processRunnerMock.Verify(p => p.ExecuteCommand("sudo ip link set wlan0 down", CancellationToken.None), Times.Once);
            processRunnerMock.Verify(p => p.ExecuteCommand("sudo ip link set dev wlan0 address 00:11:22:33:44:55", CancellationToken.None), Times.Once);
            processRunnerMock.Verify(p => p.ExecuteCommand("sudo ip link set wlan0 up", CancellationToken.None), Times.Once);
            processRunnerMock.VerifyNoOtherCalls();
        }
    }
}