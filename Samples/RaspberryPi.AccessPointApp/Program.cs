using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaspberryPi.Extensions;
using RaspberryPi.Model;
using RaspberryPi.Network;

namespace RaspberryPi.AccessPointApp
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Console.WriteLine(
                $"RaspberryPi.AccessPointApp version {typeof(Program).Assembly.GetName().Version} {Environment.NewLine}" +
                $"Copyright(C) superdev GmbH. All rights reserved.{Environment.NewLine}");

            var osplatform = RuntimeInformationHelper.GetOperatingSystem();
            if (osplatform != OSPlatform.Linux)
            {
                Console.WriteLine($"This program only runs on RaspberryPi. OSPlatform \"{osplatform}\" is not supported.");
                return -1;
            }

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
                });
            });
            serviceCollection.AddRaspberryPi();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Console.ReadKey();

            // Setup an access point
            var accessPoint = serviceProvider.GetRequiredService<IAccessPoint>();

            var ssid = "testssid";
            var psk = "testpassword";
            var ipAddress = IPAddress.Parse("192.168.99.1");
            var channel = 6;
            var country = Countries.Switzerland;

            await accessPoint.ConfigureAsync(ssid, psk, ipAddress, channel, country);

            //Console.ReadKey();
            return 0;
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

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
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

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