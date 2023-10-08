using API.Request;
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
        private readonly IMapper _mapper;

        public BidController(IBidService bidService, IMapper mapper)
        {
            _bidService = bidService;
            _mapper = mapper;
        }

        [HttpGet("auction/{id}")]
        public IActionResult GetBidsFromAuction(int id, [FromQuery] PagingParam pagingParam) 
        {
            List<Bid> bidList = _bidService.GetAllBidsFromAuction(id)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<BidResponse> response = _mapper.Map<List<BidResponse>>(bidList);
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get bids successfully", Data = response });
        }
    }
}
