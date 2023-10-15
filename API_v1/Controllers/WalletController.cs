using API.ErrorHandling;
using Microsoft.AspNetCore.Cors;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Request.ThirdParty.Zalopay;
using System.Net;
using DataAccess;
using Respon;
using Respon.WalletRes;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpPost("payment/order/{orderId:int}")]
        public IActionResult Pay(int orderId) {
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
                    Message = "Your account is not available"
                });
            }
            if (user.Role != (int) Role.Buyer && user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _walletService.CheckoutWallet(userId, orderId);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = $"Pay for order {orderId} successfully",
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
