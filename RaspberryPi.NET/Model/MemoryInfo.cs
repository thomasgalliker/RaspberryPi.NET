namespace RaspberryPi
{
    public class MemoryInfo
    {
        public RandomAccessMemoryStatus RandomAccessMemory { get; set; }

        public MemoryStatus Swap { get; set; }
    }
}