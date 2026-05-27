using Microsoft.AspNetCore.Mvc;

namespace EqDemo.Controllers
{
    public class OidcConfigurationController : Controller
    {
        private readonly ILogger<OidcConfigurationController> _logger;
        private readonly IConfiguration _configuration;

        public OidcConfigurationController(IConfiguration configuration, ILogger<OidcConfigurationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("_configuration/{clientId}")]
        public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        {
            return Ok(new Dictionary<string, string>());
        }
    }
}
