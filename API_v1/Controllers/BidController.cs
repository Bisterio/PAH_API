using AutoMapper;
using DataAccess;
using DataAccess.Models;
using API.Request;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request.Param;
using Respon;
using Respon.BidRes;
using Service;
using System.Net;
using API.ErrorHandling;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public BidController(IBidService bidService, IMapper mapper, IUserService userService)
        {
            _bidService = bidService;
            _mapper = mapper;
            _userService = userService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpGet("auction/{id}")]
        public IActionResult GetBidsFromAuction(int id, [FromQuery] int status, [FromQuery] PagingParam pagingParam) 
        {
            List<Bid> bidList = _bidService.GetAllBidsFromAuction(id, status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();

            List<BidResponse> response = _mapper.Map<List<BidResponse>>(bidList);
            foreach (var bid in response)
            {
                bid.BidderName = _userService.Get((int)bid.BidderId).Name;
            }
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Get bids successfully",
                Data = response
            });
        }

        [Authorize]
        [HttpPost]
        public IActionResult PlaceBid([FromBody] BidRequest request)
        {
            var bidderId = GetUserIdFromToken();
            var bidder = _userService.Get(bidderId);
            if (bidder == null || bidder.Role != (int)Role.Buyer)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _bidService.PlaceBid(bidderId, _mapper.Map<Bid>(request));
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Place bid successfully",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("retract/{id}")]
        public IActionResult RetractBid(int id)
        {
            var bidderId = GetUserIdFromToken();
            var bidder = _userService.Get(bidderId);
            if (bidder == null || bidder.Role != (int)Role.Buyer)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _bidService.RetractBid(id, bidderId);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Retract successfully",
                Data = null
            });
        }
    }
}
