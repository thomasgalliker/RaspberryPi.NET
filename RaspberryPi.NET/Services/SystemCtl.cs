using System;
using Microsoft.Extensions.Logging;
using RaspberryPi.Process;

namespace RaspberryPi.Services
{
    public class SystemCtl : ISystemCtl
    {
        private readonly ILogger logger;
        private readonly IProcessRunner processRunner;

        public SystemCtl(
            ILogger<SystemCtl> logger,
            IProcessRunner processRunner)
        {
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public bool StartService(string serviceName)
        {
            return this.RunServiceCommand("start", serviceName);
        }

        public bool RestartService(string serviceName)
        {
            return this.RunServiceCommand("restart", serviceName);
        }

        public bool StopService(string serviceName)
        {
            return this.RunServiceCommand("stop", serviceName);
        }

        public bool IsEnabled(string serviceName)
        {
            var result = this.RunServiceCommand("is-enabled -q", serviceName);
            return result;
        }

        public bool IsActive(string serviceName)
        {
            var result = this.RunServiceCommand("is-active -q", serviceName);
            return result;
        }

        public bool EnableService(string serviceName)
        {
            return this.RunServiceCommand("enable", serviceName);
        }

        public bool DisableService(string serviceName)
        {
            return this.RunServiceCommand("disable", serviceName);
        }

        public bool MaskService(string serviceName)
        {
            return this.RunServiceCommand("mask", serviceName);
        }

        public bool UnmaskService(string serviceName)
        {
            return this.RunServiceCommand("unmask", serviceName);
        }

        private bool RunServiceCommand(string command, string serviceName)
        {
            var systemCtlCommand = $"sudo systemctl {command} {serviceName}";
            //this.logger.LogDebug($"RunServiceCommand: {systemCtlCommand}");

            //var commandLineInvocation = new CommandLineInvocation("/bin/bash", $"-c \"{systemCtlCommand}\"");
            var result = this.processRunner.TryExecuteCommand(systemCtlCommand);
            var success = result.Success;
            if (success)
            {
                this.logger.LogDebug(
                    $"RunServiceCommand: '{systemCtlCommand}' finished successfully");
            }
            else
            {
                this.logger.LogInformation(
                    $"RunServiceCommand: '{systemCtlCommand}' failed with exit code {result.ExitCode}{Environment.NewLine}" +
                    $"{result.OutputData}");
            }

            return success;
        }

        public bool ReloadDaemon()
        {
            var systemctlCommand = $"sudo systemctl daemon-reload";
            this.logger.LogDebug($"ReloadDaemon: {systemctlCommand}");

            var result = this.processRunner.TryExecuteCommand(systemctlCommand);
            var success = result.Success;
            if (success)
            {
                this.logger.LogInformation(
                    $"ReloadDaemon: '{systemctlCommand}' finished successfully");
            }
            else
            {
                this.logger.LogInformation(
                    $"ReloadDaemon: '{systemctlCommand}' failed with exit code {result.ExitCode}{Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, $"> Error")}");
            }

            return success;
        }
    }
}