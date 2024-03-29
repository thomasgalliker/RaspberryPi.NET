﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaspberryPi.Services
{
    /// <summary>
    /// Representation of a service unit.
    /// </summary>
    /// <remarks>
    /// See also: https://www.digitalocean.com/community/tutorials/understanding-systemd-units-and-unit-files
    /// </remarks>
    public class ServiceDefinition
    {
        public ServiceDefinition(string serviceName)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (!Regex.IsMatch(serviceName, @"^[a-zA-Z0-9_.@]*$"))
            {
                throw new ArgumentException("Service name must only contain alphanumeric characters", nameof(serviceName));
            }

            this.ServiceName = serviceName;
        }

        public string ServiceName { get; private set; }

        public ServiceType? Type { get; set; }

        public string PIDFile { get; set; }

        public string WorkingDirectory { get; set; }

        public string Description { get; set; }

        public string SyslogIdentifier { get; set; }

        public string ExecStartPre { get; set; }

        public string ExecStart { get; set; }

        public string ExecStop { get; set; }

        public string ExecStopPost { get; set; }

        public string KillSignal { get; set; }

        public KillMode? KillMode { get; set; }

        public string UserName { get; set; }

        public string GroupName { get; set; }

        public ServiceRestart? Restart { get; set; }

        public int? RestartSec { get; set; }

        public IEnumerable<string> AfterServices { get; set; }

        public IEnumerable<string> Wants { get; set; }

        public IEnumerable<string> WantedBy { get; set; }

        public IEnumerable<string> Environments { get; set; }

        public string EnvironmentFile { get; set; }

        public string Busname { get; set; }

        public string GetSystemdUnitFile()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("[Unit]");
            if (!string.IsNullOrEmpty(this.Description))
            {
                stringBuilder.AppendLine($"Description={this.Description.Replace(Environment.NewLine, " ")}");
            }

            if (this.AfterServices != null && this.AfterServices.Any())
            {
                stringBuilder.AppendLine($"After={string.Join(" ", this.AfterServices)}");
            }

            if (this.Wants != null && this.Wants.Any())
            {
                stringBuilder.AppendLine($"Wants={string.Join(" ", this.Wants)}");
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("[Service]");

            var serviceType = this.Type ?? this.GetDefaultServiceType();
            stringBuilder.AppendLine($"Type={GetServiceTypeDirective(serviceType)}");

            if (!string.IsNullOrEmpty(this.PIDFile))
            {
                stringBuilder.AppendLine($"PIDFile={this.PIDFile}");
            }

            if (!string.IsNullOrEmpty(this.WorkingDirectory))
            {
                stringBuilder.AppendLine($"WorkingDirectory={this.WorkingDirectory}");
            }

            if (!string.IsNullOrEmpty(this.ExecStartPre))
            {
                stringBuilder.AppendLine($"ExecStartPre={this.ExecStartPre}");
            }

            if (!string.IsNullOrEmpty(this.ExecStart))
            {
                stringBuilder.AppendLine($"ExecStart={this.ExecStart}");
            }

            if (!string.IsNullOrEmpty(this.ExecStop))
            {
                stringBuilder.AppendLine($"ExecStop={this.ExecStop}");
            }

            if (!string.IsNullOrEmpty(this.ExecStopPost))
            {
                stringBuilder.AppendLine($"ExecStopPost={this.ExecStopPost}");
            }

            if (!string.IsNullOrEmpty(this.KillSignal))
            {
                stringBuilder.AppendLine($"KillSignal={this.KillSignal}");
            }

            if (this.KillMode is KillMode killMode)
            {
                stringBuilder.AppendLine($"KillMode={GetKillModeDirective(killMode)}");
            }

            var syslogIdentifier = this.SyslogIdentifier ?? this.ServiceName;
            if (!string.IsNullOrEmpty(syslogIdentifier))
            {
                stringBuilder.AppendLine($"SyslogIdentifier={syslogIdentifier}");
            }

            if (!string.IsNullOrEmpty(this.UserName) || !string.IsNullOrEmpty(this.GroupName))
            {
                stringBuilder.AppendLine();

                if (!string.IsNullOrEmpty(this.UserName))
                {
                    stringBuilder.AppendLine($"User={this.UserName}");
                }

                if (!string.IsNullOrEmpty(this.GroupName))
                {
                    stringBuilder.AppendLine($"Group={this.GroupName}");
                }
            }

            if (this.Restart is ServiceRestart serviceRestart)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Restart={GetRestartDirective(serviceRestart)}");

                if (this.RestartSec != null && serviceRestart != ServiceRestart.No)
                {
                    stringBuilder.AppendLine($"RestartSec={this.RestartSec}");
                }
            }

            if (this.Environments != null && this.Environments.Any())
            {
                stringBuilder.AppendLine();
                foreach (var environment in this.Environments)
                {
                    stringBuilder.AppendLine($"Environment={environment}");
                }
            }

            if (!string.IsNullOrEmpty(this.EnvironmentFile))
            {
                stringBuilder.AppendLine($"EnvironmentFile={this.EnvironmentFile}");
            }

            stringBuilder.AppendLine();

            if (this.WantedBy != null && this.WantedBy.Any())
            {
                stringBuilder.AppendLine("[Install]");
                stringBuilder.AppendLine($"WantedBy={string.Join(" ", this.WantedBy)}");
            }

            return stringBuilder.ToString().Trim('\n', '\r', ' ');
        }

        private ServiceType GetDefaultServiceType()
        {
            if (this.Type == null)
            {
                if (string.IsNullOrEmpty(this.ExecStart))
                {
                    return ServiceType.Oneshot;
                }
            }

            return ServiceType.Simple;
        }

        private static string GetKillModeDirective(KillMode killMode)
        {
            switch (killMode)
            {
                case Services.KillMode.No:
                    return "no";
                case Services.KillMode.Process:
                    return "process";
                default:
                    throw new NotSupportedException($"KillMode '{killMode}' is not supported");
            }
        }

        private static string GetRestartDirective(ServiceRestart serviceRestart)
        {
            switch (serviceRestart)
            {
                case ServiceRestart.No:
                    return "no";
                case ServiceRestart.Always:
                    return "always";
                case ServiceRestart.OnSuccess:
                    return "on-success";
                case ServiceRestart.OnFailure:
                    return "on-failure";
                case ServiceRestart.OnAbnormal:
                    return "on-abnormal";
                case ServiceRestart.OnAbort:
                    return "on-abort";
                case ServiceRestart.OnWatchdog:
                    return "on-watchdog";
                default:
                    throw new NotSupportedException($"ServiceRestart '{serviceRestart}' is not supported");
            }
        }

        private static string GetServiceTypeDirective(ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.Simple:
                    return "simple";
                case ServiceType.Forking:
                    return "forking";
                case ServiceType.Oneshot:
                    return "oneshot";
                case ServiceType.Dbus:
                    return "dbus";
                case ServiceType.Notify:
                    return "notify";
                case ServiceType.Idle:
                    return "idle";
                default:
                    throw new NotSupportedException($"ServiceType '{serviceType}' is not supported");
            }
        }

        public override string ToString()
        {
            return this.ServiceName;
        }
    }
}