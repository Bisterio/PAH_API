using API.Request;
using API.Response;
using API.Response.AuctionRes;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Implement;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IMapper _mapper;

        public AuctionController(IAuctionService auctionService, IMapper mapper)
        {
            _auctionService = auctionService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAuctions([FromQuery] string? title, [FromQuery] int categoryId, [FromQuery] int materialId, [FromQuery] int orderBy) 
        { 
            List<Auction> auctionList = _auctionService.GetAuctions(title, categoryId, materialId, orderBy);
            List<AuctionResponse> response = _mapper.Map<List<AuctionResponse>>(auctionList);
            return Ok(new BaseResponse { Code = 200, Message = "Get auctions successfully", Data = response });
        }

        [HttpGet("{id}")]
        public IActionResult GetAuctionById(int id)
        {
            Auction auction = _auctionService.GetAuctionById(id);
            return Ok(new BaseResponse { Code = 200, Message = "Get auctions successfully", Data = _mapper.Map<AuctionResponse>(auction) });
        }

        [HttpGet("seller/{id}")]
        public IActionResult GetAuctionBySellerId(int id)
        {
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(id);
            List<AuctionResponse> response = _mapper.Map<List<AuctionResponse>>(auctionList);
            return Ok(new BaseResponse { Code = 200, Message = "Get auctions successfully", Data = response });
        }

        [HttpGet("staff/{id}")]
        public IActionResult GetAuctionAssigned(int id)
        {
            List<Auction> auctionList = _auctionService.GetAuctionAssigned(id);
            List<AuctionResponse> response = _mapper.Map<List<AuctionResponse>>(auctionList);
            return Ok(new BaseResponse { Code = 200, Message = "Get auctions successfully", Data = response });
        }

        [HttpPost]
        public IActionResult CreateAuction([FromBody] AuctionRequest request)
        {
            _auctionService.CreateAuction(_mapper.Map<Auction>(request));
            return Ok(new BaseResponse { Code = 200, Message = "Create auction successfully", Data = null });
        }
    }
}
