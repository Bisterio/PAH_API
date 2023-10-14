using API.ErrorHandling;
using API.Request;
using API.Response;
using API.Response.AuctionRes;
using API.Response.ProductRes;
using API.Response.SellerRes;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Implement;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IBidService _bidService;
        private readonly ISellerService _sellerService;
        private readonly IAddressService _addressService;

        public AuctionController(IAuctionService auctionService, IMapper mapper, IUserService userService, IImageService imageService, 
            IBidService bidService, ISellerService sellerService, IAddressService addressService)
        {
            _auctionService = auctionService;
            _mapper = mapper;
            _userService = userService;
            _imageService = imageService;
            _bidService = bidService;
            _sellerService = sellerService;
            _addressService = addressService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        private SellerWithAddressResponse GetSellerResponse(int sellerId)
        {
            Seller seller = _sellerService.GetSeller(sellerId);
            SellerWithAddressResponse sellerResponse = new SellerWithAddressResponse();
            if (seller != null)
            {
                sellerResponse = _mapper.Map<SellerWithAddressResponse>(seller);
                sellerResponse.Province = null;
                sellerResponse.WardCode = null;
                sellerResponse.Ward = null;
                sellerResponse.DistrictId = null;
                sellerResponse.District = null;
                sellerResponse.Street = null;

                Address address = _addressService.GetByCustomerId(sellerId)
                    .Where(a => a.Type == (int)AddressType.Pickup && a.IsDefault == true)
                    .FirstOrDefault();

                if (address != null)
                {
                    sellerResponse.Province = address.Province;
                    sellerResponse.WardCode = address.WardCode;
                    sellerResponse.Ward = address.Ward;
                    sellerResponse.DistrictId = address.DistrictId;
                    sellerResponse.District = address.District;
                    sellerResponse.Street = address.Street;
                }
            }
            return sellerResponse;
        }

        private int CountAuctions(string? title, int status, int categoryId, int materialId)
        {
            int count = 0;
            count = _auctionService.GetAuctions(title, status, categoryId, materialId, 0).Count();
            return count;
        }

        [HttpGet]
        public IActionResult GetAuctions([FromQuery] string? title,
            [FromQuery] int status,
            [FromQuery] int categoryId, 
            [FromQuery] int materialId, 
            [FromQuery] int orderBy,
            [FromQuery] PagingParam pagingParam) 
        { 
            List<Auction> auctionList = _auctionService.GetAuctions(title, status, categoryId, materialId, orderBy)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);
            
            foreach (var item in mappedList)
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

            int count = CountAuctions(title, status, categoryId, materialId);

            AuctionListCountResponse response = new AuctionListCountResponse()
            {
                Count = count,
                AuctionList = mappedList                
            };

            return Ok(new BaseResponse 
            { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Get auctions successfully",
                Data = response
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetAuctionById(int id)
        {
            Auction auction = _auctionService.GetAuctionById(id);

            if (auction == null)
            {
                return NotFound(new ErrorDetails 
                { 
                    StatusCode = 400, 
                    Message = "This auction is not exist" 
                });
            }

            List<ProductImage> imageList = _imageService.GetAllImagesByProductId((int)auction.ProductId);
            List<string> imageUrls = imageList.Select(i => i.ImageUrl).ToList();
            AuctionDetailResponse response = _mapper.Map<AuctionDetailResponse>(auction);
            response.ImageUrls = imageUrls;

            response.NumberOfBids = _bidService.GetNumberOfBids(id);
            response.NumberOfBidders = _bidService.GetNumberOfBidders(id);

            Bid highestBid = _bidService.GetHighestBidFromAuction(auction.Id);
            response.CurrentPrice = response.StartingPrice;
            if (highestBid != null)
            {
                response.CurrentPrice = highestBid.BidAmount;
            }

            response.Seller = GetSellerResponse((int)auction.Product.SellerId);
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Get auctions successfully", 
                Data = response 
            });
        }

        [HttpGet("seller/{id}")]
        public IActionResult GetAuctionBySellerId(int id, [FromQuery] PagingParam pagingParam)
        {
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(id)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
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
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Get auctions successfully", 
                Data = response 
            });
        }

        [Authorize]
        [HttpGet("staff")]
        public IActionResult GetAuctionAssigned([FromQuery] PagingParam pagingParam)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Staff)
            {
                return Unauthorized(new ErrorDetails 
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this" 
                });
            }
            List<Auction> auctionList = _auctionService.GetAuctionAssigned(userId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
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
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Get auctions successfully", 
                Data = response 
            });
        }

        [HttpGet("bidder/{id}")]
        public IActionResult GetAuctionsByBidderId(int id, [FromQuery] PagingParam pagingParam)
        {
            //var userId = GetUserIdFromToken();
            //var user = _userService.Get(userId);
            //if (user == null || user.Role != (int)Role.Buyer)
            //{
            //    return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            //}
            List<Auction> auctionList = _auctionService.GetAuctionJoined(id)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();

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
            return Ok(new BaseResponse 
            {
                Code = (int)HttpStatusCode.OK, 
                Message = "Get auctions successfully",
                Data = response 
            });
        }

        [HttpPost]
        public IActionResult CreateAuction([FromBody] AuctionRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails 
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this"
                });
            }
            _auctionService.CreateAuction(_mapper.Map<Auction>(request));
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Create auction successfully", 
                Data = null 
            });
        }

        [Authorize]
        [HttpPatch("staff/approve/{id}")]
        public IActionResult StaffApproveAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int) Role.Staff)
            {
                return Unauthorized(new ErrorDetails 
                {
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this" 
                });
            }
            _auctionService.StaffApproveAuction(id);
            return Ok(new BaseResponse
            { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Auction approved successfully", 
                Data = null 
            });
        }

        [Authorize]
        [HttpPatch("staff/reject/{id}")]
        public IActionResult StaffRejectAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int )Role.Staff)
            {
                return Unauthorized(new ErrorDetails 
                {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this" 
                });
            }
            _auctionService.StaffRejectAuction(id);
            return Ok(new BaseResponse 
            { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Auction rejected successfully", 
                Data = null 
            });
        }
    }
}
