namespace RaspberryPi.Network
{
    public static class NetworkInterfaceServiceExtensions
    {
        public static INetworkInterface GetLoopback(this INetworkInterfaceService networkInterfaceService)
        {
            return networkInterfaceService.GetByName("lo");
        }
    }
}
