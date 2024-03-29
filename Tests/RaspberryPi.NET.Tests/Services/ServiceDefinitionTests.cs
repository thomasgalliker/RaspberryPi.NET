﻿using System;
using FluentAssertions;
using RaspberryPi.Services;
using Xunit;

namespace RaspberryPi.Tests.Services
{
    public class ServiceDefinitionTests
    {
        [Fact]
        public void ShouldCreateServiceDefinition_ThrowsArgumentException()
        {
            // Arrange
            var serviceName = "%service/!0";

            // Act
            Action action = () => new ServiceDefinition(serviceName);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ShouldGetSystemdUnitFile_Default()
        {
            // Arrange
            var serviceDefinition = new ServiceDefinition("service.Name");

            // Act
            var result = serviceDefinition.GetSystemdUnitFile();

            // Assert
            result.Should().Be(
                "[Unit]\r\n" +
                "\r\n" +
                "[Service]\r\n" +
                "Type=oneshot\r\n" +
                "SyslogIdentifier=service.Name");
        }

        [Fact]
        public void ShouldGetSystemdUnitFile_Sample()
        {
            // Arrange
            var serviceDefinition = new ServiceDefinition("service_Name")
            {
                Description = "Test service description",
                Type = ServiceType.Notify,
                WorkingDirectory = "/home/pi/directory",
                SyslogIdentifier = "service.name",
                ExecStart = "/home/pi/directory/executable",
                ExecStop = "execStop",
                KillSignal = "killSignal",
                KillMode = KillMode.Process,
                Restart = ServiceRestart.No,
                UserName = "pi",
                GroupName = "pi",
                AfterServices = new[]
                {
                    "network-online.target",
                    "firewalld.service"
                },
                Wants = new[]
                {
                    "network-online.target"
                },
                Environments = new[]
                {
                    "ASPNETCORE_ENVIRONMENT=Production",
                    "DOTNET_PRINT_TELEMETRY_MESSAGE=false",
                    "DOTNET_ROOT=/home/pi/.dotnet"
                },
                WantedBy = new[]
                {
                    "multi-user.target"
                }
            };

            // Act
            var result = serviceDefinition.GetSystemdUnitFile();

            // Assert
            result.Should().Be(
                "[Unit]\r\n" +
                "Description=Test service description\r\n" +
                "After=network-online.target firewalld.service\r\n" +
                "Wants=network-online.target\r\n" +
                "\r\n" +
                "[Service]\r\n" +
                "Type=notify\r\n" +
                "WorkingDirectory=/home/pi/directory\r\n" +
                "ExecStart=/home/pi/directory/executable\r\n" +
                "ExecStop=execStop\r\n" +
                "KillSignal=killSignal\r\n" +
                "KillMode=process\r\n" +
                "SyslogIdentifier=service.name\r\n" +
                "\r\n" +
                "User=pi\r\n" +
                "Group=pi\r\n" +
                "\r\n" +
                "Restart=no\r\n" +
                "\r\n" +
                "Environment=ASPNETCORE_ENVIRONMENT=Production\r\n" +
                "Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false\r\n" +
                "Environment=DOTNET_ROOT=/home/pi/.dotnet\r\n" +
                "\r\n" +
                "[Install]\r\n" +
                "WantedBy=multi-user.target");
        }
    }
}
