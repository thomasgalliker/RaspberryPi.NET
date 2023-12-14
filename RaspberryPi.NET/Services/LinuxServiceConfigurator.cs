using System;
using Microsoft.Extensions.Logging;
using RaspberryPi.Process;
using RaspberryPi.Storage;

namespace RaspberryPi.Services
{
    public class LinuxServiceConfigurator : IServiceConfigurator
    {
        // TODO: Refactor this service
        // - InstallService should only install the service - no servicectl operations
        // - ReinstallService should only start the service if it was previously running
        // - UninstallService should attempt to stop the service
        private readonly ILogger logger;
        private readonly IFileSystem fileSystem;
        private readonly ISystemCtl systemCtl;
        private readonly IProcessRunner processRunner;

        public LinuxServiceConfigurator(
            ILogger<LinuxServiceConfigurator> logger,
            IFileSystem fileSystem,
            ISystemCtl systemCtl,
            IProcessRunner processRunner)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.systemCtl = systemCtl;
            this.processRunner = processRunner;

            this.CheckSystemPrerequisites();
        }

        public void UninstallService(string serviceName)
        {
            try
            {
                this.logger.LogDebug($"Uninstalling systemd service \"{serviceName}\"...");
                var systemdUnitFilePath = GetServiceFilePath(serviceName);
                this.UninstallServiceInternal(serviceName, systemdUnitFilePath);
                this.logger.LogDebug($"Systemd service \"{serviceName}\" successfully uninstalled");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to uninstall systemd service \"{serviceName}\"");
                throw;
            }
        }

        private void UninstallServiceInternal(string serviceName, string systemdUnitFilePath)
        {
            this.systemCtl.StopService(serviceName);
            this.systemCtl.DisableService(serviceName);
            this.systemCtl.MaskService(serviceName);
            this.fileSystem.File.Delete(systemdUnitFilePath);
        }

        public void InstallService(ServiceDefinition serviceDefinition)
        {
            try
            {
                this.logger.LogDebug($"Installing systemd service \"{serviceDefinition.ServiceName}\"...");
                var systemdUnitFilePath = GetServiceFilePath(serviceDefinition.ServiceName);
                this.InstallServiceInternal(systemdUnitFilePath, serviceDefinition);
                this.logger.LogDebug($"Systemd service \"{serviceDefinition.ServiceName}\" successfully installed");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to install systemd service \"{serviceDefinition.ServiceName}\"");
                throw;
            }
        }

        private void InstallServiceInternal(string systemdUnitFilePath, ServiceDefinition serviceDefinition)
        {
            var serviceFileContent = serviceDefinition.GetSystemdUnitFile();
            this.WriteUnitFile(systemdUnitFilePath, serviceFileContent);
            this.systemCtl.EnableService(serviceDefinition.ServiceName);
        }

        public void ReinstallService(ServiceDefinition serviceDefinition)
        {
            try
            {
                this.logger.LogDebug($"Reinstalling systemd service \"{serviceDefinition.ServiceName}\"...");
                var systemdUnitFilePath = GetServiceFilePath(serviceDefinition.ServiceName);
                this.UninstallServiceInternal(serviceDefinition.ServiceName, systemdUnitFilePath);
                this.InstallServiceInternal(systemdUnitFilePath, serviceDefinition);
                this.logger.LogDebug($"Systemd service \"{serviceDefinition.ServiceName}\" successfully reinstalled");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to reinstall systemd service \"{serviceDefinition.ServiceName}\"");
                throw;
            }
        }

        private void WriteUnitFile(string path, string contents)
        {
            this.fileSystem.File.WriteAllText(path, contents);

            var commandLineInvocation = new CommandLineInvocation("/bin/bash", $"-c \"chmod 644 {path}\"");
            this.processRunner.ExecuteCommand(commandLineInvocation);
        }

        private void CheckSystemPrerequisites()
        {
            if (!this.fileSystem.File.Exists("/bin/bash"))
            {
                throw new Exception("Could not detect bash.");
            }

            if (!this.processRunner.HaveSudoPrivileges())
            {
                throw new Exception("Requires elevated privileges. Please run command as sudo.");
            }

            if (!this.processRunner.IsSystemdInstalled())
            {
                throw new Exception("Could not detect systemd.");
            }
        }

        private static string GetServiceFilePath(string serviceName)
        {
            var serviceFileName = serviceName.EndsWith(".service", StringComparison.InvariantCultureIgnoreCase) ? serviceName : $"{serviceName}.service";
            return $"/etc/systemd/system/{serviceFileName}";
        }
    }
}