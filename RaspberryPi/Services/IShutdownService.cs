namespace RaspberryPi.Services
{
    public interface IShutdownService
    {
        /// <summary>
        /// Reboots the system immediately.
        /// </summary>
        void Reboot();

        /// <summary>
        /// Shutdown immediately.
        /// </summary>
        void Shutdown();
    }
}