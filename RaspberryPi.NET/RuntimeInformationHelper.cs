using System;
using System.Runtime.InteropServices;

namespace RaspberryPi
{
    public static class RuntimeInformationHelper
    {
        public static OSPlatform GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }

            throw new NotSupportedException($"Operating system \"{RuntimeInformation.OSDescription}\" is not supported");
        }
    }
}