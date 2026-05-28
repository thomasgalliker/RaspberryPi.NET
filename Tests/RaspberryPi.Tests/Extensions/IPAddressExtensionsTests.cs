namespace RaspberryPi.Tests.Extensions
{
    public class IPAddressExtensionsTests
    {
        [Fact]
        public void ShouldCalculateCIDR()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("255.255.255.0");

            // Act
            var cidr = ipAddress.CalculateCIDR();

            // Assert
            cidr.Should().Be(24);
        }
    }
}
