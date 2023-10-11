using API.Response;
using API.Response.WalletRes;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IMapper _mapper;

        public WalletController(IWalletService walletService, IMapper mapper)
        {
            _walletService = walletService;
            _mapper = mapper;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [Authorize]
        [HttpGet("current")]
        public IActionResult GetByCurrentUser()
        {
            var userId = GetUserIdFromToken();  
            Wallet wallet = _walletService.GetByCurrentUser(userId);
            if(wallet == null)
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
