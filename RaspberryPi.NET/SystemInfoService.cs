﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RaspberryPi.Extensions;
using RaspberryPi.Process;
using UnitsNet;

namespace RaspberryPi
{
    /// <inheritdoc/>
    public class SystemInfoService : ISystemInfoService
    {
        internal const string CpuInfoFilePath = "/proc/cpuinfo";
        internal const string MemInfoFilePath = "/proc/meminfo";
        internal const string FreeBytesWide = "free --bytes --wide";
        internal const string MeasureTemp = "vcgencmd measure_temp";
        internal const string MeasureVolts = "vcgencmd measure_volts";
        internal const string GetThrottled = "vcgencmd get_throttled";

        private static readonly Regex CputTemperaturePattern = new Regex(@"[0-9.-]{3,}");
        private static readonly Regex RandomAccessMemoryPattern = new Regex(@"Mem:\s*(?<total>\d*)\s*(?<used>\d*)\s*(?<free>\d*)\s*(?<shared>\d*)\s*(?<buffers>\d*)\s*(?<cache>\d*)\s*(?<available>\d*)\s*.*$");
        private static readonly Regex SwapMemoryPattern = new Regex(@"Swap:\s*(?<total>\d*)\s*(?<used>\d*)\s*(?<free>\d*)\s*.*$");

        private readonly IProcessRunner processRunner;

        public SystemInfoService(
            IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        /// <inheritdoc/>
        public void SetHostname(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentException($"Parameter '{nameof(hostname)}' must not be null or empty", nameof(hostname));
            }

            this.processRunner.ExecuteCommand($"sudo hostnamectl set-hostname {hostname}");
        }

        /// <inheritdoc/>
        public async Task<HostInfo> GetHostInfoAsync()
        {
            var hostInfo = new HostInfo();

            var commandLineResult = this.processRunner.ExecuteCommand("hostnamectl");


            var keyValueRegex = new Regex(@"^(\s*)(?<Key>[^:{}]*):(?<Value>.*)", RegexOptions.Multiline);
            var matchesAll = keyValueRegex.Matches(commandLineResult.OutputData);

            if (RegexExtensions.TryParseValue(matchesAll, "Static hostname", out var hostname))
            {
                hostInfo.Hostname = hostname;
            }

            if (RegexExtensions.TryParseValue(matchesAll, "Machine ID", out var machineId))
            {
                hostInfo.MachineId = machineId;
            }

            if (RegexExtensions.TryParseValue(matchesAll, "Boot ID", out var bootId))
            {
                hostInfo.BootId = bootId;
            }

            if (RegexExtensions.TryParseValue(matchesAll, "Operating System", out var operatingSystem))
            {
                hostInfo.OperatingSystem = operatingSystem;
            }

            if (RegexExtensions.TryParseValue(matchesAll, "Kernel", out var kernel))
            {
                hostInfo.Kernel = kernel;
            }

            if (RegexExtensions.TryParseValue(matchesAll, "Architecture", out var architecture))
            {
                hostInfo.Architecture = architecture;
            }

            return hostInfo;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<CpuInfo> GetCpuInfoAsync()
        {
            var processorInfos = new List<ProcessorInfo>();
            var cpuInfo = new CpuInfo
            {
                Processors = processorInfos
            };

            var commandLineResult = this.processRunner.ExecuteCommand($"cat {CpuInfoFilePath}");

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(commandLineResult.OutputData));
            using var reader = new StreamReader(memoryStream);

            ProcessorInfo processorInfo = null;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (CheckLineStartsWith(line, "processor"))
                {
                    processorInfo = new ProcessorInfo();
                }
                else if (line.Length <= 1)
                {
                    if (processorInfo != null)
                    {
                        processorInfos.Add(processorInfo);
                    }
                    processorInfo = null;
                }

                if (processorInfo != null)
                {
                    if (CheckLineStartsWith(line, "processor"))
                    {
                        processorInfo.Processor = ReadLineValue(line);
                    }
                    else if (CheckLineStartsWith(line, "model name"))
                    {
                        processorInfo.ModelName = ReadLineValue(line);
                    }
                }
                else
                {
                    if (CheckLineStartsWith(line, "Hardware"))
                    {
                        cpuInfo.Hardware = ReadLineValue(line);
                    }
                    else if (CheckLineStartsWith(line, "Revision"))
                    {
                        cpuInfo.Revision = ReadLineValue(line);
                    }
                    else if (CheckLineStartsWith(line, "Serial"))
                    {
                        cpuInfo.Serial = ReadLineValue(line);
                    }
                    else if (CheckLineStartsWith(line, "Model"))
                    {
                        cpuInfo.Model = ReadLineValue(line);
                    }
                }
            }

            return cpuInfo;
        }

        /// <inheritdoc/>
        public CpuSensorsStatus GetCpuSensorsStatus()
        {
            // Sources:
            // https://github.com/rembertmagri/pi-control-panel/blob/a5e4f0bf25cd9574a7a799ad4183f57494295e24/src/Infrastructure/PiControlPanel.Infrastructure.OnDemand/Services/CpuService.cs
            // https://stackoverflow.com/questions/12798611/splitting-a-hex-number
            // 
            var result = this.processRunner.ExecuteCommand(MeasureTemp);

            var temperature = 0d;
            var match = CputTemperaturePattern.Match(result.OutputData);
            if (match.Success)
            {
                temperature = double.Parse(match.Value, CultureInfo.InvariantCulture);
            }

            var voltage = 0d;
            result = this.processRunner.ExecuteCommand(MeasureVolts);
            var voltageSplit = result.OutputData.Split('=');
            if (voltageSplit.Length >= 2)
            {
                var voltsWithUnit = voltageSplit[1].Trim();
                var voltsString = voltsWithUnit.Substring(0, voltsWithUnit.Length - 1);
                voltage = double.Parse(voltsString, CultureInfo.InvariantCulture);
            }

            result = this.processRunner.ExecuteCommand(GetThrottled);
            var getThrottledResultString = result.OutputData.Substring(result.OutputData.IndexOf('x') + 1).Trim();
            var getThrottledInBinary = Convert.ToString(Convert.ToInt32(getThrottledResultString, 16), 2);
            var binaryLength = getThrottledInBinary.Length;

            // Bit  Hex     Value Meaning
            // 0    1       Under-voltage detected
            // 1    2       ARM frequency has been caped
            // 2    4       Currently throttled
            // 3    8       Soft temperature limit is active
            // 16   1000    Under-voltage has occurred
            // 17   2000    ARM frequency capping has occurred
            // 18   4000    Throttling has occurred
            // 19   8000    Soft temperature limit has occurred

            return new CpuSensorsStatus
            {
                Temperature = Temperature.FromDegreesCelsius(temperature),
                Voltage = ElectricPotential.FromVolts(voltage),
                UnderVoltageDetected = binaryLength > 0 && '1'.Equals(getThrottledInBinary[binaryLength - 1]),
                ArmFrequencyCapped = binaryLength > 1 && '1'.Equals(getThrottledInBinary[binaryLength - 2]),
                CurrentlyThrottled = binaryLength > 2 && '1'.Equals(getThrottledInBinary[binaryLength - 3]),
                SoftTemperatureLimitActive = binaryLength > 3 && '1'.Equals(getThrottledInBinary[binaryLength - 4]),
                UnderVoltageOccurred = binaryLength > 16 && '1'.Equals(getThrottledInBinary[binaryLength - 17]),
                ArmFrequencyCappingOccurred = binaryLength > 17 && '1'.Equals(getThrottledInBinary[binaryLength - 18]),
                ThrottlingOccurred = binaryLength > 18 && '1'.Equals(getThrottledInBinary[binaryLength - 19]),
                SoftTemperatureLimitOccurred = binaryLength > 19 && '1'.Equals(getThrottledInBinary[binaryLength - 20]),
            };
        }

        /// <inheritdoc/>
        public MemoryInfo GetMemoryInfo()
        {
            var result = this.processRunner.ExecuteCommand(FreeBytesWide);
            var randomAccessMemoryStatus = GetRandomAccessMemoryStatus(result.OutputData);
            var swapMemoryStatus = GetSwapMemoryStatus(result.OutputData);

            return new MemoryInfo
            {
                RandomAccessMemory = randomAccessMemoryStatus,
                Swap = swapMemoryStatus
            };
        }

        private static RandomAccessMemoryStatus GetRandomAccessMemoryStatus(string content)
        {
            var groups = RandomAccessMemoryPattern.Match(content).Groups;
            var total = int.Parse(groups["total"].Value);
            var used = int.Parse(groups["used"].Value);
            var free = int.Parse(groups["free"].Value);
            var shared = int.Parse(groups["shared"].Value);
            var buffers = int.Parse(groups["buffers"].Value);
            var cache = int.Parse(groups["cache"].Value);
            var available = int.Parse(groups["available"].Value);

            return new RandomAccessMemoryStatus
            {
                Total = Information.FromBytes(total),
                Used = Information.FromBytes(used),
                Free = Information.FromBytes(free),
                Shared = Information.FromBytes(shared),
                Buffers = Information.FromBytes(buffers),
                Cache = Information.FromBytes(cache),
                Available = Information.FromBytes(available),
            };
        }

        private static MemoryStatus GetSwapMemoryStatus(string content)
        {
            var groups = SwapMemoryPattern.Match(content).Groups;
            var total = int.Parse(groups["total"].Value);
            var used = int.Parse(groups["used"].Value);
            var free = int.Parse(groups["free"].Value);

            return new MemoryStatus
            {
                Total = Information.FromBytes(total),
                Used = Information.FromBytes(used),
                Free = Information.FromBytes(free),
            };
        }

        [Obsolete("Use TryParseValue instead")]
        private static bool CheckLineStartsWith(string line, string startsWith)
        {
            return line.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase);
        }

        [Obsolete("Use TryParseValue instead")]
        private static string ReadLineValue(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).Trim();
        }
    }
}
