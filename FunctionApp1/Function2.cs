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
    public static class Function2
    {
        [FunctionName("Function2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, 
            [Inject] IStorageService storageService)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                await storageService.TestStorageAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to test storage {message}", ex.Message);
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return new NoContentResult();
        }
    }
}
