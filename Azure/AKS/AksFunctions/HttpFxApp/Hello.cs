using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HttpFxApp
{
    // ReSharper disable once UnusedType.Global
    public static class Hello
    {
        [FunctionName("Hello")]
        // ReSharper disable once UnusedMember.Global
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", "Post", "Put", "Delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.Log(LogLevel.Information, "Processing Started");
            await Task.Delay(int.TryParse(Environment.GetEnvironmentVariable("WORK_DELAY"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var delay) ? delay : 750);
            log.Log(LogLevel.Information, "Processing Finished");
            log.Log(LogLevel.Information, "Response preparing");
            using var streamReader = new StreamReader(req.Body, Encoding.Default);
            var responseMessage = new
            {
                Body = await streamReader.ReadToEndAsync(),
                Headers = req.Headers.Select(x => new {x.Key, Values = string.Join("; ", x.Value)}).ToArray(),
                Query = req.QueryString.HasValue ? req.QueryString.Value : string.Empty,
                req.Method,
                Host = req.Host.Value
            };
            log.Log(LogLevel.Information, "Response prepared");
            return new OkObjectResult(responseMessage);
        }
    }
}
