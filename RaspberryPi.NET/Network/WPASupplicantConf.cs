using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaspberryPi.Network
{
    /// <summary>
    /// Source: https://www.daemon-systems.org/man/wpa_supplicant.conf.5.html
    /// </summary>
    [DebuggerDisplay("WPASupplicantConf: Networks=[{this.Networks.Count}]")]
    public class WPASupplicantConf
    {
        private const string DefaultCtrlInterface = "DIR=/var/run/wpa_supplicant GROUP=netdev";
        private const int DefaultApScan = 1;
        private const int DefaultUpdateConfig = 1;
        private static readonly Country DefaultCountry = Countries.Switzerland;

        public WPASupplicantConf()
        {
            this.CtrlInterface = DefaultCtrlInterface;
            this.APScan = DefaultApScan;
            this.UpdateConfig = DefaultUpdateConfig;
            this.Country = DefaultCountry;
            this.Networks = new List<WPASupplicantNetwork>();
        }

        public WPASupplicantConf(string content)
        {
            // TODO: Parse content
            throw new NotImplementedException();
        }

        public string CtrlInterface { get; set; }

        public int? APScan { get; set; }

        public int? UpdateConfig { get; set; }

        public Country Country { get; set; }

        public ICollection<WPASupplicantNetwork> Networks { get; set; }

        public override string ToString()
        {
            // TODO: Generate wpa_supplicant.conf
            return base.ToString();
        }
    }
}