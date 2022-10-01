using System.Threading.Tasks;

namespace RaspberryPi
{
    public interface ISystemInfoService
    {
        void SetHostname(string hostname);

        Task<CpuInfo> GetCpuInfoAsync();

        CpuSensorsStatus GetCpuSensorsStatus();

        MemoryInfo GetMemoryInfo();

        Task<HostInfo> GetHostInfoAsync();
    }
}