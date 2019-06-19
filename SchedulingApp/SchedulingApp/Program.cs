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
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging((hostContext, logger)=>
                {
                    logger.AddConsole();
                    
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
            await host.StartAsync();

            host.WaitForShutdown();
        }
    }

    internal class SchedulerLogger : ILogProvider
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

    internal class HostedProgram : IHostedService
    {
        private readonly IScheduler _scheduler;

        public HostedProgram(IScheduler scheduler)
        {
            _scheduler = scheduler;
            
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler.ScheduleJob(JobBuilder.Create<TestJob>().Build(), TriggerBuilder.Create().WithCronSchedule("0/15 * * * * ?").StartNow().Build());

            return _scheduler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _scheduler.Shutdown(false, cancellationToken);
        }
    }

    internal class TestJob : IJob
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
