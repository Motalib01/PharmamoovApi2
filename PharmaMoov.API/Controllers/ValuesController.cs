using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace PharmaMoov.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "Status:", "API Running.." };
        }

        [HttpGet("GetCurrentCultureDate")]
        public string GetCurrentCultureDate()
        {
            return DateTime.Now.ToString();
        }
    }
}
