using API.Request;
using API.Response;
using API.Response.UserRes;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Service;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthController(ILogger<AuthController> logger, IUserService userService, IMapper mapper, IConfiguration configuration, ITokenService tokenService) {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
            _config = configuration;
            _tokenService = tokenService;
        }

        [HttpGet]
        //Test function, delete later
        public IActionResult Get() {
            return Ok(_userService.GetAll());
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/login")]
        public IActionResult Login([FromBody] LoginRequest request) {
            var user = _userService.Login(request.Email, request.Password);
            if (user == null) {
                return Unauthorized(new ErrorResponse { Code = 401, Message = "Email or password is incorrect" });
            }
            var token = _userService.AddRefreshToken(user.Id);
            return Ok(new BaseResponse { Code = 200, Message = "Login successfully", Data = token});
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/refresh")]
        public IActionResult Refresh([FromBody] Tokens token) {
            var a = token.AccessToken;
            var principal = _tokenService.GetPrincipalFromExpiredToken(token.AccessToken);
            var id = int.Parse(principal.FindFirst("UserId").Value);

            var dbToken = _userService.GetSavedRefreshToken(id, token.RefreshToken);
            if (dbToken == null) {
                return Unauthorized();
            }

            var newToken = _userService.AddRefreshToken(id);
            return Ok(new BaseResponse { Code = 200, Message = "Refresh successfully", Data = newToken });
        }
        
        [HttpPost]
        [AllowAnonymous]
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
