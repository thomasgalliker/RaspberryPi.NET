using System;
using System.Runtime.InteropServices;
using RaspberryPi;
using RaspberryPi.Network;
using RaspberryPi.Process;
using RaspberryPi.Services;
using RaspberryPi.Storage;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds registrations for services provided by RaspberryPi.NET library,
        /// e.g. <seealso cref="IProcessRunner"/>, <seealso cref="IJournalctl"/>, <seealso cref="ISystemCtl"/>, ...
        /// </summary>
        /// <param name="services"></param>
        public static void AddRaspberryPi(this IServiceCollection services, bool omitPlatformCheck = false)
        {
            var osplatform = OperatingSystemHelper.GetOperatingSystem();
            if (osplatform == OSPlatform.Linux)
            {
                services.AddSingleton<IProcessRunner, ProcessRunner>();
                services.AddSingleton<IServiceConfigurator, LinuxServiceConfigurator>();
                services.AddSingleton<ISystemInfoService, SystemInfoService>();
                services.AddSingleton<IShutdownService, ShutdownService>();
            }
            //#if DEBUG
            else if (osplatform == OSPlatform.Windows)
            {
                services.AddSingleton<IProcessRunner, NullProcessRunner>();
                services.AddSingleton<IServiceConfigurator, NullServiceConfigurator>();
                services.AddSingleton<ISystemInfoService, NullSystemInfoService>();
                services.AddSingleton<IShutdownService, NullShutdownService>();
            }
            //#endif
            else if (omitPlatformCheck == false)
            {
                throw new NotSupportedException($"This library only runs on RaspberryPi. OSPlatform \"{osplatform}\" is not supported.");
            }

            services.AddSingleton<IJournalctl, Journalctl>();

            // Network
            services.AddSingleton<INetworkManager, NetworkManager>();
            services.AddSingleton<IInterface, Interface>();
            services.AddSingleton<IAccessPoint, AccessPoint>();
            services.AddSingleton<IDHCP, DHCP>();
            services.AddSingleton<IWPA, WPA>();
            services.AddSingleton<INetworkInterfaceService, NetworkInterfaceService>();

            // Services
            services.AddSingleton<ISystemCtl, SystemCtl>();

            // Storage
            services.AddSingleton<IFile, File>();
            services.AddSingleton<IDirectory, Directory>();
            services.AddSingleton<IFileStreamFactory, FileStreamFactory>();
            services.AddSingleton<IFileSystem, FileSystem>();

            // System
        }
    }
}