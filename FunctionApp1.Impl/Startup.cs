using FunctionApp1.Impl;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using Microsoft.ApplicationInsights.Channel;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;
using System.Linq;
using System.IO;

[assembly: WebJobsStartup(typeof(Startup))]

namespace FunctionApp1.Impl
{

    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection<AutofacServiceProviderBuilder>();
        }
    }

    internal class AutofacServiceProviderBuilder : IServiceProviderBuilder
    {
        private const string CONSOLE_OUTPUT_TEMPLATE = @"{UtcTimestamp:yyyy-MM-dd'T'HH:mm:ss.fff} [{LogEventLevel}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        private readonly LoggingLevelSwitch _logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);

        protected IConfiguration Configuration { get; }

        public AutofacServiceProviderBuilder(IConfiguration configuration) => Configuration = configuration;

        public IServiceProvider Build()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With<UtcTimestampEnricher>()
                .Enrich.With<AppInsightsEnricher>()
                .MinimumLevel.ControlledBy(_logLevelSwitch);

            var telemetryConverter = new MyTelemetryConverter();
            var telemetryConfiguration = TelemetryConfiguration.Active;
            telemetryConfiguration.TelemetryInitializers.Add(new MyTelemetryInitializer());

            if (Configuration.GetValue<string>("AppEnvironment") == "local")
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console(
                        outputTemplate: CONSOLE_OUTPUT_TEMPLATE)
                    .WriteTo.ApplicationInsights(telemetryConfiguration, telemetryConverter);
            }
            else
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(
                        Path.Combine(Configuration.GetValue<string>("HOME"), "LogFiles", "Application", "log.txt"),
                        outputTemplate: CONSOLE_OUTPUT_TEMPLATE,
                        buffered:false,
                        shared: false,
                        fileSizeLimitBytes: 10485760,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true
                       )
                    .WriteTo.ApplicationInsights(telemetryConfiguration, telemetryConverter);
            }

            var services = new ServiceCollection();
            services.Configure<IConfiguration>(Configuration);
            services.Configure<FunctionAppSettings>(Configuration);

            services.AddLogging(c =>
            {
                c.SetMinimumLevel(LogLevel.Trace);
                //c.AddAzureWebAppDiagnostics();
                c.AddSerilog(loggerConfiguration.CreateLogger(), dispose: true);
            });
            var builder = new ContainerBuilder();
            builder.Populate(services);
            Configure(builder);
            return new AutofacServiceProvider(builder.Build());
        }

        protected void Configure(ContainerBuilder builder)
        {
            
            builder.RegisterType<WorkerService>().As<IWorkerService>().InstancePerLifetimeScope();
            builder.RegisterType<StorageService>().As<IStorageService>().InstancePerLifetimeScope();
        }
    }

    class UtcTimestampEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
        {
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("UtcTimestamp", logEvent.Timestamp.UtcDateTime));
        }
    }
    class AppInsightsEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
        {
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("LogEventLevel", logEvent.Level.ToMicrosoftLogLevel().ToString()));
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("prop__{OriginalFormat}", logEvent.MessageTemplate));
        }
    }
    public static class LogExtenstions
    {
        public static LogLevel ToMicrosoftLogLevel(this LogEventLevel logEventLevel)
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                    return LogLevel.Trace;
                case LogEventLevel.Debug:
                    return LogLevel.Debug;
                case LogEventLevel.Information:
                    return LogLevel.Information;
                case LogEventLevel.Warning:
                    return LogLevel.Warning;
                case LogEventLevel.Error:
                    return LogLevel.Error;
                case LogEventLevel.Fatal:
                    return LogLevel.Critical;
                default:
                    return LogLevel.None;
            }
        }
    }
}
