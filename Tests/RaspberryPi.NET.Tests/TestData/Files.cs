using System.IO;
using System.Reflection;
using RaspberryPi.Internals.ResourceLoader;

namespace RaspberryPi.Tests.TestData
{
    public static class Files
    {
        private static readonly Assembly Assembly = typeof(Files).Assembly;

        public static string GetHostInfoTxt()
        {
            return ResourceLoader.Current.GetEmbeddedResourceString(Assembly, "hostinfo.txt");
        }

        public static string GetCpuInfoTxt()
        {
            return ResourceLoader.Current.GetEmbeddedResourceString(Assembly, "cpuinfo.txt");
        }
        
        public static string GetFreeTxt()
        {
            return ResourceLoader.Current.GetEmbeddedResourceString(Assembly, "free.txt");
        }

        public static Stream GetWPASupplicantConfStream()
        {
            return ResourceLoader.Current.GetEmbeddedResourceStream(Assembly, "wpa_supplicant.conf");
        }

        public static Stream GetDhcpdConfStream()
        {
            return ResourceLoader.Current.GetEmbeddedResourceStream(Assembly, "dhcpcd.conf");
        }
    }
}
