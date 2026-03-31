using System.Collections.Generic;
using RaspberryPi.Network;

namespace RaspberryPi.Tests.TestData
{
    internal static class WPASupplicantConfs
    {
        internal static WPASupplicantConf GetWPASupplicantConf_testssid()
        {
            return new WPASupplicantConf
            {
                APScan = 1,
                UpdateConfig = 1,
                Networks = new List<WPASupplicantNetwork>
                {
                    new WPASupplicantNetwork
                    {
                        SSID = "testssid",
                        PSK = "testpassword",
                        KeyMgmt = "WPA-PSK",
                        Proto = null,
                        Pairwise = null,
                        AuthAlg = null,
                        Disabled = false,
                    }
                }
            };
        }
    }
}
