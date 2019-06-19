using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Impl
{
    class WorkerService : IWorkerService
    {
        private readonly ILogger _logger;

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }
        public async Task ExecuteAsync()
        {
            _logger.LogTrace("WorkerService LogTrace {value}", "ServiceValue");
            await Task.Delay(100);
            _logger.LogDebug("WorkerService LogDebug {value}", "ServiceValue");
            await Task.Delay(100);
            _logger.LogInformation("WorkerService LogInformation {value}", "ServiceValue");
            await Task.Delay(100);
            _logger.LogWarning("WorkerService LogWarning {value}", "ServiceValue");
            await Task.Delay(100);
            _logger.LogError("WorkerService LogError {value}", "ServiceValue");
            await Task.Delay(100);
            _logger.LogCritical("WorkerService LogCritical {value}", "ServiceValue");

            await Task.Delay(1000);

            try
            {
                throw new Exception("Exception Message");
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "WorkerService LogTrace {value}", "ServiceError");
                await Task.Delay(100);
                _logger.LogDebug(ex, "WorkerService LogDebug {value}", "ServiceError");
                await Task.Delay(100);
                _logger.LogInformation(ex, "WorkerService LogInformation {value}", "ServiceError");
                await Task.Delay(100);
                _logger.LogWarning(ex, "WorkerService LogWarning {value}", "ServiceError");
                await Task.Delay(100);
                _logger.LogError(ex, "WorkerService LogError {value}", "ServiceError");
                await Task.Delay(100);
                _logger.LogCritical(ex, "WorkerService LogCritical {value}", "ServiceError");
                await Task.Delay(100);
            }

        }
    }
}
