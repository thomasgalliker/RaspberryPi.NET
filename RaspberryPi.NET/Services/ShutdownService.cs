using RaspberryPi.Process;

namespace RaspberryPi.Services
{
    public class ShutdownService : IShutdownService
    {
        private readonly IProcessRunner processRunner;

        public ShutdownService(IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        public void Shutdown()
        {
            this.processRunner.ExecuteCommand("sudo shutdown -h now");
        }

        public void Reboot()
        {
            this.processRunner.ExecuteCommand("sudo shutdown -r now");
        }
    }
}
