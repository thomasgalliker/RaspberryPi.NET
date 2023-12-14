using UnitsNet;

namespace RaspberryPi
{
    public class MemoryStatus
    {
        public Information Total { get; set; }

        public Information Used { get; set; }

        public Information Free { get; set; }
    }
}