using System.Collections.Generic;

namespace RaspberryPi
{
    public class CpuInfo
    {
        public CpuInfo()
        {
            this.Processors = new List<ProcessorInfo>();
        }

        public IReadOnlyCollection<ProcessorInfo> Processors { get; set; }

        public string Hardware { get; set; }

        public string Revision { get; set; }

        public string Serial { get; set; }

        public string Model { get; set; }
    }
}