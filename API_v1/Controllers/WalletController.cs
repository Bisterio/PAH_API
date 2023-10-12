using API.ErrorHandling;
using API.Response;
using Microsoft.AspNetCore.Cors;
using API.Response.WalletRes;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.CustomRequest;
using Service.ThirdParty.Zalopay;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class WalletController : ControllerBase {
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public WalletController(IWalletService walletService, IUserService userService, IMapper mapper) {
            _walletService = walletService;
            _userService = userService;
            _mapper = mapper;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpPost("topup")]
        public async Task<IActionResult> Topup([FromBody] TopupRequest request) {
            var userId = GetUserIdFromToken();  
            if (userId == null) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "You are not logged in" 
                });
            }

            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this" 
                });
            }

            await _walletService.Topup(userId, request);
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Topup successfully", 
                Data = null 
            });
        }

        [Authorize]
        [HttpGet("current")]
        public IActionResult GetByCurrentUser()
        {
            var userId = GetUserIdFromToken();
            Wallet wallet = _walletService.GetByCurrentUser(userId);
            if (wallet == null)
            {
                wallet = new Wallet();
            }
            WalletCurrentUserResponse response = _mapper.Map<WalletCurrentUserResponse>(wallet);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get current user wallet successfully",
                Data = response
            });
        }
    }
}
