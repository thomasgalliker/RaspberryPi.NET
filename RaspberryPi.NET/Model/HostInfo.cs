using System.Diagnostics;

namespace RaspberryPi
{
    [DebuggerDisplay("HostInfo: {this.Hostname}")]
    public class HostInfo
    {
        public string Hostname { get; set; }

        public string MachineId { get; set; }

        public string BootId { get; set; }

        public string OperatingSystem { get; set; }

        public string Kernel { get; set; }

        public string Architecture { get; set; }
    }
}