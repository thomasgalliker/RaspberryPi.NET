using FluentAssertions;
using RaspberryPi.Utils;
using Xunit;

namespace RaspberryPi.Tests.Utils
{
    public class WPAPassphraseTests
    {
        [Fact]
        public void ShouldGetHash()
        {
            // Arrange
            var ssid = "testssid";
            var passphrase = "testpassword";

            // Act
            var hash = WPAPassphrase.GetHash(ssid, passphrase);

            // Assert
            hash.Should().Be("7c73efd9deb6f95aa08e98b1503d57d967f8ae3ad4f60f0b1d2aad00f3f81937");
        }
    }
}
