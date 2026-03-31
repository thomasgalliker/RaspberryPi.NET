namespace RaspberryPi.Services
{
    public interface ISystemCtl
    {
        /// <summary>
        /// Checks if the service unit is enabled.
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns>Returns <c>true</c> if the service unit is enabled.</returns>
        bool IsEnabled(string serviceName);

        /// <summary>
        /// Checks if the service unit is active (=running).
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns>Returns <c>true</c> if the service unit is active.</returns>
        bool IsActive(string serviceName);

        /// <summary>
        /// Marks the service unit for autostart at boot time (in a unit-specific manner, described in its [Install] section).
        /// Disabled services can be started manually using <seealso cref="StartService(string)"/>.
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool DisableService(string serviceName);

        /// <summary>
        /// Marks the service unit to not automatically start at boot time.
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool EnableService(string serviceName);

        /// <summary>
        /// Masking disallows attempts to start the service unit (either manually or as a dependency of any other unit, including the dependencies of the default boot target).
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool MaskService(string serviceName);

        /// <summary>
        /// Unmasking allows attempts to start the service unit (either manually or as a dependency of any other unit, including the dependencies of the default boot target).
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool UnmaskService(string serviceName);

        /// <summary>
        /// Resetarts the service unit.
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool RestartService(string serviceName);

        /// <summary>
        /// Starts the service unit.
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool StartService(string serviceName);

        /// <summary>
        /// Stops the service unit.
        /// </summary>
        /// <param name="serviceName">The service unit.</param>
        /// <returns></returns>
        bool StopService(string serviceName);

        /// <summary>
        /// Reload systemd manager configuration. 
        /// </summary>
        /// <returns></returns>
        bool ReloadDaemon();
    }
}