using API.ErrorHandling;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Request;
using Respon;
using Service;
using Service.EmailService;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace API.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class AuthController : ControllerBase {
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthController(ILogger<AuthController> logger, 
            IUserService userService, IMapper mapper, 
            IConfiguration configuration, ITokenService tokenService,
            IEmailService emailService) {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
            _config = configuration;
            _tokenService = tokenService;
            _emailService = emailService;
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
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Email or password is incorrect" });
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
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Please login again" });
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
            return Ok(new BaseResponse { Code = 200, Message = "Register successfully", Data = null });
        }

        [HttpPost("/api/forgotpassword")]
        [AllowAnonymous]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> SendEmailResetPasswordAsync([FromBody] ForgotPasswordRequest request) {
            var user = _userService.GetByEmail(request.Email);
            if (user == null) {
                return NotFound(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.NotFound,
                    Message = "User not found"
                });
            }

            var token = _tokenService.GenerateResetToken();
            _userService.AddResetToken(user.Id, token);
            //var callback = Url.Action(nameof(ResetPassword), nameof(AuthController), new { token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Reset password token", $"Token is: "+token);
            await _emailService.SendEmail(message);
            return Ok(new BaseResponse {
                Code = 200,
                Message = "Send mail successfully",
                Data = null
            });
        }

        [HttpPost("/api/resetpassword")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request) {
            _userService.ResetPassword(request);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Reset password successfully",
                Data = null
            });
        }
    }
}
