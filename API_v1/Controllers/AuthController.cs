using API.Request;
using API.Response;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public AuthController(ILogger<AuthController> logger, IUserService userService, IMapper mapper) {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("/api/login")]
        public IActionResult Login([FromBody] LoginRequest request) {
            var user = _userService.Login(request.Email, request.Password);
            if (user == null) {
                return Unauthorized(new ErrorResponse { Code = 401, Message = "Email or password is incorrect" });
            }
            return Ok(new BaseResponse { Code = 200, Message = "Login successfully", Data = user});
        }
        
        [HttpPost]
        [Route("/api/register")]
        public IActionResult Register([FromBody] RegisterRequest request) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            _userService.Register(_mapper.Map<User>(request));
            return Ok(new BaseResponse { Code = 200, Message = "Login successfully", Data = null });
        }
    }
}
