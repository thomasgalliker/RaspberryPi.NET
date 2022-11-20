using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaspberryPi;
using RaspberryPi.Extensions;

internal partial class Program
{
    private static int Main(string[] args)
    {
        var assemblyVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        Console.WriteLine(
            $"RaspberryPi.ConsoleApp version {assemblyVersion} {Environment.NewLine}" +
            $"Copyright(C) superdev GmbH. All rights reserved.{Environment.NewLine}");

        var osplatform = OperatingSystemHelper.GetOperatingSystem();
        if (osplatform != OSPlatform.Linux)
        {
            Console.WriteLine($"This program only runs on RaspberryPi. OSPlatform \"{osplatform}\" is not supported.");
            return -1;
        }

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

        //Console.ReadKey();

        // Retrieve general system information such as CPU sensor data.
        var systemInfoService = serviceProvider.GetRequiredService<ISystemInfoService>();
        var cpuSensorsStatus = systemInfoService.GetCpuSensorsStatus();
        Console.WriteLine($"CPU Sensors Status:");
        Console.WriteLine($"Temperature: {cpuSensorsStatus.Temperature}°C");
        Console.WriteLine($"Voltage: {cpuSensorsStatus.Voltage}V");
        Console.WriteLine($"CurrentlyThrottled: {cpuSensorsStatus.CurrentlyThrottled}");
        Console.WriteLine();

        var memoryInfo = systemInfoService.GetMemoryInfo();
        Console.WriteLine($"Memory Info:");
        Console.WriteLine($"RAM Total: {memoryInfo.RandomAccessMemory.Total / 1024 / 1024} MB");
        Console.WriteLine($"RAM Used: {memoryInfo.RandomAccessMemory.Used / 1024 / 1024} MB");
        Console.WriteLine($"RAM Free: {memoryInfo.RandomAccessMemory.Free / 1024 / 1024} MB");
        Console.WriteLine();

        //Console.ReadKey();
        return 0;
    }
}