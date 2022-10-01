namespace RaspberryPi
{
    public class RandomAccessMemoryStatus : MemoryStatus
    {
        public int Shared { get; set; }

        public int Buffers { get; set; }

        public int Cache { get; set; }

        public int Available { get; set; }
    }
}