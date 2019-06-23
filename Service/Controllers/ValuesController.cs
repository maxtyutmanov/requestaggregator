using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get(string ids)
        {
            Thread.Sleep(1000);

            var idsSplit = ids?.Split(',');
            if (idsSplit.Length < 20)
            {
                _logger.LogError("Processed {0} items", idsSplit?.Length ?? 0);
            }
            else
            {
                _logger.LogWarning("Processed {0} items", idsSplit?.Length ?? 0);
            }

            var result = idsSplit.ToDictionary(x => x, x => int.Parse(x) % 2 == 0);
            return Ok(result);
        }
    }
}
