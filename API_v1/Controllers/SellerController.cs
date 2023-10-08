using API.ErrorHandling;
using API.Response;
using API.Response.UserRes;
using AutoMapper;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly IMapper _mapper;

        public SellerController(IUserService userService, ISellerService sellerService, IMapper mapper)
        {
            _userService = userService;
            _sellerService = sellerService;
            _mapper = mapper;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
             //|| user.Role != (int)Role.Seller
            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            var seller = _sellerService.GetSeller(userId);
            if (seller == null)
            {
                seller = new DataAccess.Models.Seller();
            }
            SellerDetailResponse response = _mapper.Map<SellerDetailResponse>(seller);
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get seller successfully", Data = response });
        }
    }
}
