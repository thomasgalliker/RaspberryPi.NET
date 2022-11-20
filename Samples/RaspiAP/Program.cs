using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaspberryPi;
using RaspberryPi.Extensions;
using RaspberryPi.Network;

namespace RaspiAP
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            Console.WriteLine(
                $"RaspiAP version {assemblyVersion} {Environment.NewLine}" +
                $"Copyright(C) superdev GmbH. All rights reserved.{Environment.NewLine}");

            //var osplatform = RuntimeInformationHelper.GetOperatingSystem();
            //if (osplatform != OSPlatform.Linux)
            //{
            //    Console.WriteLine($"This program only runs on RaspberryPi. OSPlatform \"{osplatform}\" is not supported.");
            //    return -1;
            //}

            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // This sample console app runs with Microsoft.Extensions.DependencyInjection.
            // However, you can also manually construct the dependency trees if you wish so.
            var serviceCollection = new ServiceCollection();

            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            serviceCollection.AddLogging(o =>
            {
                o.ClearProviders();
                o.SetMinimumLevel(LogLevel.Debug);
                o.AddSimpleConsole(c =>
                {
                    c.TimestampFormat = $"{dateTimeFormat.ShortDatePattern} {dateTimeFormat.LongTimePattern} ";
                })
                .AddDebug();
            });
            serviceCollection.AddRaspberryPi();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //Console.ReadKey();

            var networkInterfaceService = serviceProvider.GetRequiredService<INetworkInterfaceService>();
            var wlan0 = networkInterfaceService.GetByName("wlan0");

            var wpa = serviceProvider.GetRequiredService<IWPA>();
            var accessPoint = serviceProvider.GetRequiredService<IAccessPoint>();
            var networkManager = serviceProvider.GetRequiredService<INetworkManager>();

            if (args.Contains("99"))
            {
                var ssids = wpa.ScanSSIDs(wlan0).ToList();
                Console.WriteLine(string.Join(Environment.NewLine, ssids.Select(s => $"{s}")));
            }
            else if (args.Contains("100"))
            {
                var wpaSupplicantNetwork = new WPASupplicantNetwork
                {
                    SSID = "galliker",
                    PSK = "abcdefg12345678",
                };
                await networkManager.SetupStationMode(wlan0, wpaSupplicantNetwork);
            }
            //else if (args.Contains("101"))
            //{
            //    // Setup an access point
            //    var @interface = serviceProvider.GetRequiredService<IInterface>();
            //    await @interface.SetConfig(wlan0, "1.2.3.4", 0);
            //}
            else if (args.Contains("102"))
            {
                var ssid = "testssid";
                var psk = "testpassword";
                var ipAddress = IPAddress.Parse("192.168.99.1");
                var channel = 6;
                var country = Countries.Switzerland;

                await networkManager.SetupAccessPoint2(wlan0, ssid, psk, ipAddress, channel, country);
            }
            else if (args.Contains("103"))
            {
                var @interface = serviceProvider.GetRequiredService<IInterface>();
                var report = await @interface.ReportAsync(wlan0);
                Console.WriteLine(report);
            }
            else if (args.Contains("104"))
            {
                foreach (var iface in networkInterfaceService.GetAll())
                {
                    var connectedClients = accessPoint.GetConnectedClients(iface).ToList();
                    Console.WriteLine($"{iface.Name}: # connected clients: {connectedClients.Count}");
                    foreach (var connectedClient in connectedClients)
                    {
                        Console.WriteLine($"client: MacAddress={connectedClient.MacAddress}, connected time: {connectedClient.ConnectedTime}");
                    }
                }
               
            }

            //Console.ReadKey();
            return 0;
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                Console.WriteLine("Unobserved task exception occurred: {0}", ex);
            }
            else
            {
                Console.WriteLine("Unobserved task exception occurred but no exception was supplied");
            }

            e.SetObserved();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine("Unhandled exception occurred: {0}", ex);
            }
            else
            {
                Console.WriteLine("Unhandled exception occurred: {0}", e.ExceptionObject);
            }
        }
    }
}