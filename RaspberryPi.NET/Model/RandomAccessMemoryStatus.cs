using UnitsNet;

namespace RaspberryPi
{
    public class RandomAccessMemoryStatus : MemoryStatus
    {
        public Information Shared { get; set; }

        public Information Buffers { get; set; }

        public Information Cache { get; set; }

        public Information Available { get; set; }
    }
}