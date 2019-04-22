using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using FunctionApp1.Impl;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [Inject] IWorkerService service)
        {            
            log.LogTrace("LogTrace {value}", "SomeValue");
            await Task.Delay(100);
            log.LogDebug("LogDebug {value}", "SomeValue");
            await Task.Delay(100);
            log.LogInformation("LogInformation {value}", "SomeValue");
            await Task.Delay(100);
            log.LogWarning("LogWarning {value}", "SomeValue");
            await Task.Delay(100);
            log.LogError("LogError {value}", "SomeValue");
            await Task.Delay(100);
            log.LogCritical("LogCritical {value}", "SomeValue");
            await Task.Delay(100);

            try
            {
                throw new Exception("Exception Message");
            }
            catch (Exception ex)
            {
                log.LogTrace(ex, "LogTrace {value}", "WithError");
                await Task.Delay(100);
                log.LogDebug(ex, "LogDebug {value}", "WithError");
                await Task.Delay(100);
                log.LogInformation(ex, "LogInformation {value}", "WithError");
                await Task.Delay(100);
                log.LogWarning(ex, "LogWarning {value}", "WithError");
                await Task.Delay(100);
                log.LogError(ex, "LogError {value}", "WithError");
                await Task.Delay(100);
                log.LogCritical(ex, "LogCritical {value}", "WithError");
                await Task.Delay(100);
            }
            log.LogInformation("--------------------------------");
            await Task.Delay(500);
            await service.ExecuteAsync();

            return new OkObjectResult($"Hello");

        }
    }
}
