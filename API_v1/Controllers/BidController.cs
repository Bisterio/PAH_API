using API.Response;
using API.Response.BidRes;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpGet("auction/{id}")]
        public IActionResult GetBidsFromAuction(int id) 
        {
            List<Bid> bidList = _bidService.GetAllBidsFromAuction(id);
            List<BidResponse> response = _mapper.Map<List<BidResponse>>(bidList);
            foreach (var bid in response)
            {
                bid.BidderName = _userService.Get((int)bid.BidderId).Name;
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get bids successfully", Data = response });
        }
    }
}
