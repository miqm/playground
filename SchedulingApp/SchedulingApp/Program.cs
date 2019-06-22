using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Autofac.Extras.Quartz;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SchedulingApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await RunHost();
        }

        public static async Task RunHost(CancellationToken cancellationToken = default)
        {
            var host = new HostBuilder()
                            .ConfigureLogging((hostContext, logger) =>
                            {
                                logger.AddConsole();
                                logger.AddEventSourceLogger();                                

                            })
                            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                            .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                            {
                                builder.RegisterModule(new QuartzAutofacFactoryModule());
                                builder.RegisterModule(new QuartzAutofacJobsModule(typeof(Program).Assembly));
                                builder.RegisterType<SchedulerLogger>().AsSelf().SingleInstance();
                            })
                            .ConfigureServices((hostContext, services) =>
                            {
                                services.AddHostedService<HostedProgram>();
                            })
                            .Build();
            LogProvider.SetCurrentLogProvider(host.Services.GetRequiredService<SchedulerLogger>());
            await host.RunAsync(cancellationToken);
        }
    }

    public class SchedulerLogger : ILogProvider
    {
        private readonly ILogger<SchedulerLogger> _logger;

        public SchedulerLogger(ILogger<SchedulerLogger> logger)
        {
            _logger = logger;
        }
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null)
                    _logger.Log((LogLevel)(int)level, exception, func(), parameters);
                return true;
            };
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            //not used in Quartz
            throw new NotImplementedException();
        }

        public IDisposable OpenNestedContext(string message)
        {
            //not used in Quartz
            throw new NotImplementedException();
        }
    }

    public class HostedProgram : IHostedService
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<HostedProgram> _logger;

        public HostedProgram(IScheduler scheduler, ILogger<HostedProgram> logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting scheduler...");
            await _scheduler.ScheduleJob(JobBuilder.Create<TestJob>().Build(), TriggerBuilder.Create().WithCronSchedule("0/15 * * * * ?").StartNow().Build());
            await _scheduler.Start(cancellationToken);
            _logger.LogInformation("Scheduler started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping scheduler...");
            await _scheduler.Shutdown(false, cancellationToken);
            _logger.LogInformation("Scheduler stopped.");

        }
    }

    public class TestJob : IJob
    {
        private readonly ILogger<TestJob> _logger;

        public TestJob(ILogger<TestJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Tik...");
            await Task.Delay(500);
            _logger.LogInformation("Tak!");
        }
    }
}
