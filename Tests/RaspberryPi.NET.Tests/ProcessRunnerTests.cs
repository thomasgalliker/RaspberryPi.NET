using System;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using RaspberryPi.Process;
using RaspberryPi.Tests.Logging;
using Xunit;
using Xunit.Abstractions;

namespace RaspberryPi.Tests
{
    public class ProcessRunnerTests
    {
        private readonly AutoMocker autoMocker;

        public ProcessRunnerTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.Use<ILogger<ProcessRunner>>(new TestOutputHelperLogger<ProcessRunner>(testOutputHelper));
        }

        [Fact]
        public void ShouldExecuteCommand_Success()
        {
            // Arrange
            var commandLineInvocation = new CommandLineInvocation("dotnet", "--info");

            var processRunner = this.autoMocker.CreateInstance<ProcessRunner>();

            // Act
            CommandLineResult result;
            using (var cancellationTokenSource = new CancellationTokenSource(1000))
            {
                result = processRunner.ExecuteCommand(commandLineInvocation, cancellationTokenSource.Token);
            }

            // Assert
            result.Should().NotBeNull();
            result.OutputData.Should().NotBeEmpty();
            result.ErrorData.Should().BeEmpty();
        }

        [Fact]
        public void ShouldExecuteCommand_ReturnsErrorData()
        {
            // Arrange
            var commandLineInvocation = new CommandLineInvocation("dotnet", "--arg");

            var processRunner = this.autoMocker.CreateInstance<ProcessRunner>();

            // Act
            var result = processRunner.TryExecuteCommand(commandLineInvocation);

            // Assert
            result.Should().NotBeNull();
            result.OutputData.Should().BeEmpty();
            result.ErrorData.Should().NotBeEmpty();
        }

        [Fact]
        public void ShouldExecuteCommand_ThrowsCommandLineException_IfExecutableDoesNotExist()
        {
            // Arrange
            var commandLine = "executable argument1 argument2";

            var processRunner = this.autoMocker.CreateInstance<ProcessRunner>();

            // Act
            Action action = () => processRunner.ExecuteCommand(commandLine);

            // Assert
            action.Should().Throw<CommandLineException>();
        }
    }
}