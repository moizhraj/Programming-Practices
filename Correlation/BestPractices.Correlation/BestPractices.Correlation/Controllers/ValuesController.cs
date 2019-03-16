using BestPractices.Correlation.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BestPractices.Correlation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger logger;

        public ValuesController(ILogger _logger)
        {
            logger = _logger;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            logger.LogEvent("Successfull for Get values");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            try
            {
                throw new ArgumentException("Invalid Id provided");
            }
            catch(Exception ex)
            {
                logger.LogException(ex);
            }
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
