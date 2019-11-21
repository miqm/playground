using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Miqm.WebApi.UnderpostingTest
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnderpostigTestController : ControllerBase
    {
        // POST api/values
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult<UnderpostingTestDto> Post([FromBody] UnderpostingTestDto value)
        {
            return Ok(value);
        }
    }

    public class UnderpostingTestDto
    {
        [Required]
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int? P3 { get; set; }
        [Required]
        public int? P4 { get; set; }

        public string Str1 { get; set; }
        [Required]
        public string Str2 { get; set; }

        public UnderpostingTestSub1Dto S1 { get; set; }
        public UnderpostingTestSub2Dto S2 { get; set; }
        public UnderpostingTestSub3Dto S3 { get; set; }
        public UnderpostingTestSub4Dto S4 { get; set; }

        [Required]
        public UnderpostingTestSub1Dto RS1 { get; set; }
        [Required]
        public UnderpostingTestSub2Dto RS2 { get; set; }
        [Required]
        public UnderpostingTestSub3Dto RS3 { get; set; }
        [Required]
        public UnderpostingTestSub4Dto RS4 { get; set; }
    }

    public class UnderpostingTestSub1Dto
    {
        [Required]
        public int Sub { get; set; }
    }

    public class UnderpostingTestSub2Dto
    {
        [Required]
        public int? Sub { get; set; }
    }

    public class UnderpostingTestSub3Dto
    {        
        public int Sub { get; set; }
    }

    public class UnderpostingTestSub4Dto
    {
        public int? Sub { get; set; }
    }
}
