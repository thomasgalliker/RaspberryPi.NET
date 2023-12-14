using Microsoft.Extensions.Logging;

namespace RaspberryPi.Services
{
    public class NullServiceConfigurator : IServiceConfigurator
    {
        private readonly ILogger logger;

        public NullServiceConfigurator(ILogger<NullServiceConfigurator> logger)
        {
            this.logger = logger;
        }

        public void InstallService(ServiceDefinition serviceDefinition)
        {
            this.logger.LogDebug($"InstallService: \"{serviceDefinition.ServiceName}\"");
        }

        public void ReinstallService(ServiceDefinition serviceDefinition)
        {
            this.logger.LogDebug($"ReinstallService: \"{serviceDefinition.ServiceName}\"");
        }

        public void UninstallService(string serviceName)
        {
            this.logger.LogDebug($"UninstallService: \"{serviceName}\"");
        }
    }
}