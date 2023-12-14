using System;
using System.Net;

namespace RaspberryPi.Extensions
{
    public static class IPAddressExtensions
    {
        public static int? CalculateCIDR(this IPAddress subnet)
        {
            int? cidr = null;

            if (subnet != null)
            {
                var subnetMask = BitConverter.ToUInt32(subnet.GetAddressBytes(), 0);
                for (var i = 0; i < 32; i++)
                {
                    if ((subnetMask & (1u << i)) != 0)
                    {
                        cidr ??= 0;
                        cidr++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return cidr;
        }

    }
}
