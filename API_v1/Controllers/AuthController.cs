﻿using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
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
        private readonly string _templatesPath;

        public AuthController(ILogger<AuthController> logger, 
            IUserService userService, IMapper mapper, 
            IConfiguration configuration, ITokenService tokenService,
            IEmailService emailService, IConfiguration pathConfig) {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
            _config = configuration;
            _tokenService = tokenService;
            _emailService = emailService;
            _templatesPath = pathConfig["Path:Templates"];
        }

        [HttpGet]
        //Test function, delete later
        public IActionResult Get() {
            return Ok(_userService.GetAll());
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/customer/login")]
        public IActionResult LoginCustomer([FromBody] LoginRequest request) {
            var user = _userService.Login(request.Email, request.Password);
            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Email hoặc mật khẩu không chính xác" });
            }
            else if (user.Role != (int)Role.Buyer && user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Nhân viên PAH hãy đăng nhập bằng trang hệ thống" });
            }
            if (user.Status == (int) Status.Unverified) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = 401, 
                    Message = "This account need to be verified first" });
            }
            var token = _userService.AddRefreshToken(user.Id);
            return Ok(new BaseResponse { Code = 200, Message = "Login successfully", Data = token});
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/staff/login")]
        public IActionResult LoginStaff([FromBody] LoginRequest request)
        {
            var user = _userService.Login(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Email hoặc mật khẩu không chính xác" });
            }
            else if (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Quý khách vui lòng đăng nhập bằng trang dành cho khách hàng" });
            }
            if (user.Status == (int)Status.Unverified)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = 401,
                    Message = "This account need to be verified first"
                });
            }
            var token = _userService.AddRefreshToken(user.Id);
            return Ok(new BaseResponse { Code = 200, Message = "Login successfully", Data = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/login/google")]
        public IActionResult LoginWithGoogle([FromBody] LoginGoogleRequest request)
        {
            var user = _userService.LoginWithGoogle(request.Email, request.Name, request.ProfilePicture);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Email hoặc mật khẩu không chính xác" });
            }
            else if(user.Role != (int)Role.Buyer && user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "Nhân viên PAH hãy đăng nhập bằng trang hệ thống" });
            }
            var token = _userService.AddRefreshToken(user.Id);
            return Ok(new BaseResponse { Code = 200, Message = "Login successfully", Data = token });
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
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            _userService.Register(_mapper.Map<User>(request));
            var code = _userService.CreateVerificationCode(request.Email);
            var link = Url.Link("Verify account", new { email = request.Email, code = code });

            // Get HTML template
            string fullPath = Path.Combine(_templatesPath, "RegisterEmail.html");
            StreamReader str = new StreamReader(fullPath);
            string mailText = str.ReadToEnd();
            str.Close();
            mailText = mailText.Replace("[verifyLink]", link);

            var message = new Message(new string[] { request.Email }, "Xác thực tài khoản PAH", mailText);
            await _emailService.SendEmail(message);
            return Ok(new BaseResponse { 
                Code = 200, Message = 
                "Register successfully, please check your email for link", 
                Data = null });
        }

        [HttpPost("/api/password/forgot")]
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

        [HttpPost("/api/password/reset")]
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

        [HttpGet("/api/verify/{email}/{code}", Name = "Verify account")]
        [AllowAnonymous]
        public IActionResult Verify([FromRoute] VerificationRequest request) {
            try
            {
                _userService.VerifyAccount(request.Email, request.Code);
            }
            catch
            {
               return Redirect("/mvc/error");
            }
            
            return Redirect("/mvc/verify");
        }
        
        [HttpGet("/api/verify/{email}/resend")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerificationCode(string email) {
            var code = _userService.CreateVerificationCode(email);
            var link = Url.Link("Verify account", new { email = email, code = code });
            var message = new Message(new string[] { email }, "PAH email verification", $"Click on this link to verify your account: " + link);
            await _emailService.SendEmail(message);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Verification code sent successfully",
                Data = null
            });
        }
    }
}
