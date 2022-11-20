using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGetUtils.CLI.Extensions;
using RaspberryPi.Extensions;
using RaspiAP.Commands;

namespace RaspiAP
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var assembly = typeof(Program).Assembly;
            var assemblyName = assembly.GetName();

            Console.WriteLine(
                $"{assemblyName.Name} version {assemblyName.Version} {Environment.NewLine}" +
                $"Copyright(C) superdev GmbH. All rights reserved.{Environment.NewLine}");

            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var serviceProvider = BuildServiceProvider();
            var parser = BuildParser(serviceProvider);
            var result = await parser.InvokeAsync(args).ConfigureAwait(false);

            if (args.Length == 0)
            {
                result = await parser.RunInteractiveMode();
            }

            return result;
        }

        private static Parser BuildParser(IServiceProvider serviceProvider)
        {
            var rootCommand = new RootCommand();
            rootCommand.Description = $"Configures wifi access point and station mode on Raspberry Pi";

            //rootCommand.AddGlobalOption(CommonOptions.SilentOption);

            var commandLineBuilder = new CommandLineBuilder(rootCommand);

            var commands = serviceProvider.GetServices<Command>();
            foreach (var command in commands)
            {
                commandLineBuilder.Command.Add(command);
            }

            return commandLineBuilder
                .UseDefaults()
                .Build();
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.SetMinimumLevel(LogLevel.Debug);
                configure.AddDebug();
                configure.AddSimpleConsole(o =>
                {
                    o.SingleLine = false;
                    o.TimestampFormat = "hh:mm:ss ";
                });
            });

            services.AddSingleton<Command, AccessPointCommand>();
            services.AddSingleton<Command, StationModeCommand>();
            services.AddSingleton<Command, ScanCommand>();
            services.AddSingleton<Command, StatusCommand>();
            services.AddRaspberryPi();

            return services.BuildServiceProvider();
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