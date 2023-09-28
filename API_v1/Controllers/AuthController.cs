using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly ILogger _logger;

        public AuthController(ILogger<AuthController> logger) {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get() {
            throw new Exception("Error when login");
            return Ok(new {Email = "asdasd", Password = "asdasd" });
        }
    }
}
