using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RaspberryPi.Services
{
    public class NullSystemInfoService : ISystemInfoService
    {
        private readonly ILogger logger;

        public NullSystemInfoService(ILogger<NullSystemInfoService> logger)
        {
            this.logger = logger;
        }

        public Task<CpuInfo> GetCpuInfoAsync()
        {
            this.logger.LogDebug($"GetCpuInfoAsync");
            return Task.FromResult<CpuInfo>(null);
        }

        public CpuSensorsStatus GetCpuSensorsStatus()
        {
            this.logger.LogDebug($"GetCpuSensorsStatus");
            return null;
        }

        public Task<HostInfo> GetHostInfoAsync()
        {
            this.logger.LogDebug($"GetHostInfoAsync");
            return Task.FromResult<HostInfo>(null);
        }

        public MemoryInfo GetMemoryInfo()
        {
            this.logger.LogDebug($"GetMemoryInfo");
            return null;
        }

        public void SetHostname(string hostname)
        {
            this.logger.LogDebug($"SetHostname: hostname={hostname}");
        }
    }
}
