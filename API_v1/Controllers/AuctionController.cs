using API.ErrorHandling;
using API.Request;
using API.Response;
using API.Response.AuctionRes;
using API.Response.ProductRes;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Implement;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IBidService _bidService;

        public AuctionController(IAuctionService auctionService, IMapper mapper, IUserService userService, IImageService imageService, IBidService bidService)
        {
            _auctionService = auctionService;
            _mapper = mapper;
            _userService = userService;
            _imageService = imageService;
            _bidService = bidService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpGet]
        public IActionResult GetAuctions([FromQuery] string? title, [FromQuery] int categoryId, [FromQuery] int materialId, [FromQuery] int orderBy) 
        { 
            List<Auction> auctionList = _auctionService.GetAuctions(title, categoryId, materialId, orderBy);
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                item.ImageUrl = image.ImageUrl;
                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Get auctions successfully", Data = response });
        }

        [HttpGet("{id}")]
        public IActionResult GetAuctionById(int id)
        {
            Auction auction = _auctionService.GetAuctionById(id);
            List<ProductImage> imageList = _imageService.GetAllImagesByProductId(id);
            List<string> imageUrls = imageList.Select(i => i.ImageUrl).ToList();
            AuctionDetailResponse response = _mapper.Map<AuctionDetailResponse>(auction);
            response.ImageUrls = imageUrls;
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get auctions successfully", Data = response });
        }

        [HttpGet("seller/{id}")]
        public IActionResult GetAuctionBySellerId(int id)
        {
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(id);
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                item.ImageUrl = image.ImageUrl;
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get auctions successfully", Data = response });
        }

        [HttpGet("staff/{id}")]
        public IActionResult GetAuctionAssigned(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Staff)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            List<Auction> auctionList = _auctionService.GetAuctionAssigned(id);
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                item.ImageUrl = image.ImageUrl;
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get auctions successfully", Data = response });
        }

        [HttpGet("bidder/{id}")]
        public IActionResult GetAuctionsByBidderId(int id)
        {
            //var userId = GetUserIdFromToken();
            //var user = _userService.Get(userId);
            //if (user == null || user.Role != (int)Role.Buyer)
            //{
            //    return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            //}
            List<Auction> auctionList = _auctionService.GetAuctionJoined(id);
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                item.ImageUrl = image.ImageUrl;
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get auctions successfully", Data = response });
        }

        [HttpPost]
        public IActionResult CreateAuction([FromBody] AuctionRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            _auctionService.CreateAuction(_mapper.Map<Auction>(request));
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Create auction successfully", Data = null });
        }

        [HttpPatch("staff/approve/{id}")]
        public IActionResult StaffApproveAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int) Role.Staff)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            _auctionService.StaffApproveAuction(id);
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Auction approved successfully", Data = null });
        }

        [HttpPatch("staff/reject/{id}")]
        public IActionResult StaffRejectAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int )Role.Staff)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            _auctionService.StaffRejectAuction(id);
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Auction rejected successfully", Data = null });
        }
    }
}
