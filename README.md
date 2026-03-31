# RaspberryPi

RaspberryPi is a .NET library that wraps a number of Raspberry Pi and Linux administration tasks behind a simple API.
It is aimed at applications that need to inspect Raspberry Pi system state, manage Wi-Fi configuration, interact with `systemctl`, or install systemd services from managed code.

The library targets `netstandard2.0` and `netstandard2.1`, so it can be used from many modern .NET applications.

## Platform Support

When the library runs on a Raspberry Pi or another Linux system, `AddRaspberryPi()` registers the real implementations that call Linux tools such as `systemctl`, `hostnamectl`, `vcgencmd`, and Wi-Fi/network configuration commands.
When the same code runs on a non-Linux system, the registration switches several services to null or no-op implementations so the application can still start, but Raspberry Pi specific operations are effectively unavailable.

Supported Raspberry Pi models:

| Model | Supported |
| --- |-----------|
| Raspberry Pi 4 | ✅         |
| Raspberry Pi Zero | ✅         |
| Raspberry Pi Zero W | ✅         |
| Raspberry Pi Zero 2W | ✅         |
| Raspberry Pi 400 | ?         |
| Raspberry Pi 5 | ?         |

Support assumes the device is running Raspberry Pi OS or another Linux distribution with the required tools and services available.
Features related to Wi-Fi, access point mode, and low-level device information can still vary depending on the wireless chipset, driver support, and system configuration.

## What This Library Covers

This library currently provides APIs for:

- Reading Raspberry Pi host and hardware information
- Reading memory usage and CPU sensor status
- Managing Wi-Fi station mode and access point configuration
- Editing WPA and DHCP related configuration
- Running Linux commands through an abstraction
- Installing and managing systemd services
- Working with `systemctl`

The repository also contains sample applications under `Samples/` that demonstrate how the library can be wired into a console app.

## Download and Install RaspberryPi

This project is designed as a reusable .NET library with package id `RaspberryPi`.

Use the NuGet Package Manager Console:

```powershell
PM> Install-Package RaspberryPi
```

Or the .NET CLI:

```bash
dotnet add package RaspberryPi
```

## Dependency Injection Integration

The easiest way to use the library is through `Microsoft.Extensions.DependencyInjection`.
Call `AddRaspberryPi()` to register the main services:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddRaspberryPi();

var serviceProvider = services.BuildServiceProvider();
```

On Linux, the real implementations are registered.
On other platforms, several services fall back to no-op implementations so applications can still start without immediately failing.

If you want to skip the platform guard during testing or local development, use:

```csharp
services.AddRaspberryPi();
```

## API Usage

### Read System Information

`ISystemInfoService` can be used to read Raspberry Pi specific system information such as host info, CPU sensor values, and memory usage.

```csharp
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi;
using UnitsNet.Units;

var services = new ServiceCollection();
services.AddRaspberryPi();

var serviceProvider = services.BuildServiceProvider();
var systemInfoService = serviceProvider.GetRequiredService<ISystemInfoService>();

var hostInfo = await systemInfoService.GetHostInfoAsync();
Console.WriteLine($"Hostname: {hostInfo?.Hostname}");
Console.WriteLine($"OS: {hostInfo?.OperatingSystem}");
Console.WriteLine($"Kernel: {hostInfo?.Kernel}");

var cpuSensors = systemInfoService.GetCpuSensorsStatus();
Console.WriteLine($"CPU temperature: {cpuSensors?.Temperature}");
Console.WriteLine($"Voltage: {cpuSensors?.Voltage}");
Console.WriteLine($"Currently throttled: {cpuSensors?.CurrentlyThrottled}");

var memoryInfo = systemInfoService.GetMemoryInfo();
Console.WriteLine(
    $"RAM used: {memoryInfo?.RandomAccessMemory?.Used.ToUnit(InformationUnit.Megabyte)} MB");
```

You can also update the device hostname:

```csharp
systemInfoService.SetHostname("raspberrypi-kiosk");
```

### Configure Wi-Fi Station Mode

`INetworkManager` can connect the Raspberry Pi to an existing Wi-Fi network by updating `wpa_supplicant` and related networking configuration.

```csharp
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi;
using RaspberryPi.Network;

var services = new ServiceCollection();
services.AddRaspberryPi();

var serviceProvider = services.BuildServiceProvider();
var networkManager = serviceProvider.GetRequiredService<INetworkManager>();

await networkManager.SetupStationModeAsync(
    iface: new NetworkInterface("wlan0"),
    network: new WPASupplicantNetwork
    {
        SSID = "MyWifi",
        PSK = "SuperSecretPassword",
        ScanSSID = false
    },
    country: Countries.Switzerland);
```

### Configure Access Point Mode

The same `INetworkManager` can also prepare an access point configuration.

```csharp
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi;
using RaspberryPi.Network;

var services = new ServiceCollection();
services.AddRaspberryPi();

var serviceProvider = services.BuildServiceProvider();
var networkManager = serviceProvider.GetRequiredService<INetworkManager>();

await networkManager.SetupAccessPointAsync(
    iface: new NetworkInterface("wlan0"),
    ssid: "MyPiAccessPoint",
    psk: "ChangeMe123",
    ipAddress: IPAddress.Parse("192.168.4.1"),
    channel: 6,
    country: Countries.Switzerland);
```

The repository includes the sample project `Samples/RaspiAP` which demonstrates station mode, scanning, access point setup, and status commands.

### Install a systemd Service

`IServiceConfigurator` and `ServiceDefinition` can be used to generate and install a unit file in `/etc/systemd/system`.
These operations require elevated privileges and a Linux system with `systemd`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Services;

var services = new ServiceCollection();
services.AddRaspberryPi();

var serviceProvider = services.BuildServiceProvider();
var serviceConfigurator = serviceProvider.GetRequiredService<IServiceConfigurator>();

serviceConfigurator.InstallService(new ServiceDefinition("my-worker")
{
    Description = "My background worker",
    WorkingDirectory = "/opt/my-worker",
    ExecStart = "/usr/bin/dotnet /opt/my-worker/MyWorker.dll",
    Restart = ServiceRestart.OnFailure,
    RestartSec = 10,
    WantedBy = new[] { "multi-user.target" }
});
```

### Control Existing Services with systemctl

`ISystemCtl` is a lightweight wrapper around common `systemctl` operations.

```csharp
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Services;

var services = new ServiceCollection();
services.AddRaspberryPi();

var serviceProvider = services.BuildServiceProvider();
var systemCtl = serviceProvider.GetRequiredService<ISystemCtl>();

if (!systemCtl.IsActive("ssh"))
{
    systemCtl.StartService("ssh");
}
```

## Notes

- Most of the library is intended to run on Linux, especially on Raspberry Pi OS.
- Some operations call tools such as `vcgencmd`, `hostnamectl`, `systemctl`, `wpa_cli`, and other Linux utilities.
- Service installation and certain networking operations require `sudo` privileges.
- For broader usage examples, see `Samples/RaspberryPi.ConsoleApp` and `Samples/RaspiAP`.

## License

This project is Copyright &copy; 2026 [Thomas Galliker](https://ch.linkedin.com/in/thomasgalliker).
Free for non-commercial use. For commercial use please contact the author.

## Links

- https://www.daemon-systems.org/man/wpa_supplicant.conf.5.html
- https://github.com/snowdayclub/rpi-wifisetup-ble/blob/5bc236a90fa6c8ccaebef845d62c526fa2b487d2/wpamanager.py
- https://github.com/PetrusBellmonte/MyDino/blob/d36755fc7a152b40fe71918576df6c9e6d15c20e/wifi/WifiConnector.md
- https://stackoverflow.com/questions/43644082/raspberry-pi-3-using-hostapd-and-dnsmasq-how-to-set-default-web-page-on-connect
- https://chriscant.phdcc.com/2010/02/systemstring-hidden-utf8-bom.html
