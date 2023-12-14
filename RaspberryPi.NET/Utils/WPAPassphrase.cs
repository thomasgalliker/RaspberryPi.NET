using System;
using System.Text;

namespace RaspberryPi.Utils
{
    public static class WPAPassphrase
    {
        public static string GetHash(string ssid, string passphrase)
        {
            if (string.IsNullOrEmpty(ssid))
            {
                throw new ArgumentException($"'{nameof(ssid)}' cannot be null or empty.", nameof(ssid));
            }

            if (string.IsNullOrEmpty(passphrase))
            {
                throw new ArgumentException($"'{nameof(passphrase)}' cannot be null or empty.", nameof(passphrase));
            }

            var ssidBytes = Encoding.UTF8.GetBytes(ssid);
            var passphraseBytes = Encoding.UTF8.GetBytes(passphrase);
            var rfc2898DeriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(passphraseBytes, ssidBytes, 4096).GetBytes(32);
            var hash = BitConverter.ToString(rfc2898DeriveBytes).Replace("-", "").ToLowerInvariant();
            return hash;
        }
    }
}
