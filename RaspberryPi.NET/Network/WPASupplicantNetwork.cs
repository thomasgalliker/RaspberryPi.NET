using System.Collections.Generic;
using System.Diagnostics;

namespace RaspberryPi.Network
{
    /// <summary>
    /// The network configuration of wpa_supplicant.conf.
    /// Reference: https://www.daemon-systems.org/man/wpa_supplicant.conf.5.html
    /// </summary>
    [DebuggerDisplay("{this.SSID}")]
    public class WPASupplicantNetwork
    {
        /// <summary>
        /// Network name (as announced by the access point). An ASCII or hex string.
        /// </summary>
        public string SSID { get; set; }

        /// <summary>
        /// SSID scan technique; 0 (default) or 1. 
        /// Technique 0 scans for the SSID using a broadcast Probe Request frame while 1 uses a directed Probe Request frame. 
        /// Access points that cloak themselves by not broadcasting their SSID require technique 1, but beware that this scheme can cause scanning to take longer to complete.
        /// </summary>
        public bool ScanSSID { get; set; }

        /// <summary>
        /// WPA preshared key used in WPA-PSK mode.
        /// The key is specified as 64 hex digits or as an 8-63 character ASCII passphrase.
        /// ASCII passphrases can be converted to a 256-bit key using the network SSID by the wpa_passphrase utility.
        /// </summary>
        public string PSK { get; set; }

        /// <summary>
        /// List of acceptable key management protocols.
        /// One or more of:
        /// - WPA-PSK (WPA pre-shared key)
        /// - WPA-EAP (WPA using EAP authentication)
        /// - IEEE8021X (IEEE 802.1x using EAP authentication and, optionally, dynamically generated WEP keys)
        /// - NONE (plaintext or static WEP keys)
        /// If not set this defaults to “WPA-PSK WPA-EAP”.
        /// </summary>
        public string KeyMgmt { get; set; }

        /// <summary>
        /// List of acceptable protocols. One or more of:
        /// - WPA (IEEE 802.11i/D3.0)
        /// - RSN or WPA2 (IEEE 802.11i). WPA2 is another name for RSN .
        /// If not set this defaults to “WPA RSN”.
        /// </summary>
        public string Proto { get; set; }

        /// <summary>
        /// List of acceptable pairwise (unicast) ciphers for WPA. One or more of:
        /// - CCMP (AES in Counter mode with CBC-MAC, RFC 3610, IEEE 802.11i/D7.0)
        /// - TKIP (Temporal Key Integrity Protocol, IEEE 802.11i/D7.0)
        /// - NONE (deprecated)
        /// If not set this defaults to “CCMP TKIP”.
        /// </summary>
        public string Pairwise { get; set; }

        /// <summary>
        /// List of allowed IEEE 802.11 authentication algorithms. One or more of:
        /// - OPEN (Open System authentication, required for WPA/WPA2)
        /// - SHARED (Shared Key authentication)
        /// - LEAP (LEAP/Network EAP).
        /// If not set automatic selection is used (Open System with LEAP enabled if LEAP is allowed as one of the EAP methods).
        /// </summary>
        public string AuthAlg { get; set; }

        /// <summary>
        /// Enables or disables the current network configuration.
        /// </summary>
        public bool Disabled { get; set; }
    }
}