using System.Threading;
using Microsoft.Extensions.Logging;

namespace RaspberryPi.Process
{
    internal class NullProcessRunner : IProcessRunner
    {
        private readonly ILogger logger;

        public NullProcessRunner(ILogger<NullProcessRunner> logger)
        {
            this.logger = logger;
        }

        public CommandLineResult TryExecuteCommand(string commandLine, CancellationToken cancellationToken = default)
        {
            var commandLineInvocation = new CommandLineInvocation(commandLine);
            return this.TryExecuteCommand(commandLineInvocation);
        }

        public CommandLineResult TryExecuteCommand(CommandLineInvocation invocation, CancellationToken cancellationToken = default)
        {
            var cmdResult = this.ExecuteCommandInternal(invocation);
            return cmdResult;
        }

        public CommandLineResult ExecuteCommand(string commandLine, CancellationToken cancellationToken = default)
        {
            var commandLineInvocation = new CommandLineInvocation(commandLine);
            return this.ExecuteCommand(commandLineInvocation);
        }

        public CommandLineResult ExecuteCommand(CommandLineInvocation invocation, CancellationToken cancellationToken = default)
        {
            var cmdResult = this.ExecuteCommandInternal(invocation);
            cmdResult.EnsureSuccessExitCode();
            return cmdResult;
        }

        private CommandLineResult ExecuteCommandInternal(CommandLineInvocation invocation)
        {
            this.logger.LogDebug($"{invocation.Executable} {invocation.Arguments}");
            return CommandLineResult.SuccessResult;
        }

    }
}