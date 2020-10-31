using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace WebApi2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var text = Environment.GetEnvironmentVariable("TEXT");
            _logger.LogInformation("TEXT: {text}", text);
            return Ok($"TEXT: {text}");
        }
    }
}
