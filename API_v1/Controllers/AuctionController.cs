using API.ErrorHandling;
using API.Hubs;
using AutoMapper;
using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Request;
using Request.Param;
using Respon;
using Respon.AuctionRes;
using Respon.SellerRes;
using Respon.UserRes;
using Service;
using Service.Implement;
using System.Net;
using System.Reflection;

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
        private readonly IBackgroundJobClient _backgroundJobClient;
        private IHubContext<AuctionHub> _hubContext { get; set; }

        public AuctionController(IAuctionService auctionService, IMapper mapper, IUserService userService, IImageService imageService,
            IBidService bidService, ISellerService sellerService, IAddressService addressService, IBackgroundJobClient backgroundJobClient,
            IHubContext<AuctionHub> hubcontext)
        {
            _auctionService = auctionService;
            _mapper = mapper;
            _userService = userService;
            _imageService = imageService;
            _bidService = bidService;
            _sellerService = sellerService;
            _addressService = addressService;
            _backgroundJobClient = backgroundJobClient;
            _hubContext = hubcontext;
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

        private int CountOpenAuctions(string? title, int status, int categoryId, int materialId)
        {
            int count = 0;
            count = _auctionService.GetAuctions(title, status, categoryId, materialId, 0).Count();
            return count;
        }

        private int CountAuctions(string? title, int status, int categoryId, int materialId)
        {
            int count = 0;
            count = _auctionService.GetAllAuctions(title, status, categoryId, materialId, 0).Count();
            return count;
        }

        private int CountAssignedAuctions(int id)
        {
            int count = 0;
            count = _auctionService.GetAuctionAssigned(id).Count();
            return count;
        }

        [HttpGet]
        public IActionResult GetAuctions([FromQuery] string? title,
            //[FromQuery] int status,
            [FromQuery] int categoryId,
            [FromQuery] int materialId,
            [FromQuery] int orderBy,
            [FromQuery] PagingParam pagingParam)
        {
            List<Auction> auctionList = _auctionService.GetAuctions(title, (int)AuctionStatus.RegistrationOpen, categoryId, materialId, orderBy)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);

            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }

            int count = CountOpenAuctions(title, (int)AuctionStatus.RegistrationOpen, categoryId, materialId);

            AuctionListCountResponse response = new AuctionListCountResponse()
            {
                Count = count,
                AuctionList = mappedList
            };

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get auctions successfully",
                Data = response
            });
        }

        [Authorize]
        [HttpGet("manager")]
        public IActionResult ManagerGetAuctions([FromQuery] string? title,

            [FromQuery] int categoryId,
            [FromQuery] int materialId,
            [FromQuery] int orderBy,
            [FromQuery] PagingParam pagingParam, [FromQuery] int status = -1)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<Auction> auctionList = _auctionService.GetAllAuctions(title, status, categoryId, materialId, orderBy)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);

            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

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
                Code = (int)HttpStatusCode.OK,
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

            Bid winnerBid = _bidService.GetHighestBidFromAuction(id);
            if(winnerBid != null)
            {
                WinnerResponse winner = _mapper.Map<WinnerResponse>(_userService.Get((int)winnerBid.BidderId));
                winner.FinalBid = winnerBid.BidAmount;

                response.Winner = winner;
            }
            else
            {
                WinnerResponse winner = new WinnerResponse();
                response.Winner = winner;
            }
            
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get auctions successfully",
                Data = response
            });
        }

        [HttpGet("seller/{id}")]
        public IActionResult GetAuctionBySellerId(int id, [FromQuery] int status, [FromQuery] PagingParam pagingParam)
        {
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(id, status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

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
        [HttpGet("seller/current")]
        public IActionResult GetAuctionByCurrentSeller([FromQuery] int status, [FromQuery] PagingParam pagingParam)
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
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(userId, status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

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
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<Auction> auctionList = _auctionService.GetAuctionAssigned(userId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            AuctionListCountResponse response = new AuctionListCountResponse()
            {
                Count = CountAssignedAuctions(userId),
                AuctionList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get auctions successfully",
                Data = response
            });
        }

        [HttpGet("bidder")]
        public IActionResult GetAuctionsByBidderId([FromQuery] PagingParam pagingParam, [FromQuery] int status = -1)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<Auction> auctionList = _auctionService.GetAuctionJoinedByStatus(status, userId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();

            List<AuctionListBidderResponse> response = _mapper.Map<List<AuctionListBidderResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
                item.IsWon = _auctionService.CheckWinner(userId, item.Id);
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get auctions joined successfully",
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
        [HttpGet("register/check/{id}")]
        public IActionResult CheckAuctionRegistration(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            bool check = _auctionService.CheckRegistration(userId, id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Check auction registration successfully",
                Data = check
            });
        }

        [Authorize]
        [HttpGet("win/check/current/{id}")]
        public IActionResult CheckCurrentUserWinAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            bool check = _auctionService.CheckWinner(userId, id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Check current user win successfully",
                Data = check
            });
        }

        [Authorize]
        [HttpGet("win/check/{id}")]
        public IActionResult CheckUserWinAuction(int id, [FromQuery] int userId)
        {
            //var userId = GetUserIdFromToken();
            //var user = _userService.Get(userId);
            //if (user == null)
            //{
            //    return Unauthorized(new ErrorDetails
            //    {
            //        StatusCode = (int)HttpStatusCode.Unauthorized,
            //        Message = "You are not allowed to access this"
            //    });
            //}
            bool check = _auctionService.CheckWinner(userId, id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Check user win successfully",
                Data = check
            });
        }

        [Authorize]
        [HttpGet("end/{id}")]
        public IActionResult StaffEndAuction(int id)
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
            WinnerResponse mappedWinner = EndAuction(id, false).Result;

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "End auction successfully",
                Data = mappedWinner
            });
        }

        [Authorize]
        [HttpGet("assign")]
        public IActionResult AssignStaffToAuction(int id, int staffId)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _auctionService.AssignStaff(id, staffId);
            var auction = _auctionService.GetAuctionById(id);

            // Notify staff of assigned auction
            _hubContext.Clients.Group("STAFF_" + staffId).SendAsync("ReceiveAuctionAssigned", auction.Id, auction.Title);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Assign staff successfully",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("manager/approve/{id}")]
        public IActionResult ManagerApproveAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _auctionService.ManagerApproveAuction(id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Auction approved successfully",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("manager/reject/{id}")]
        public IActionResult ManagerRejectAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _auctionService.ManagerRejectAuction(id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Auction rejected successfully",
                Data = null
            });
        }

        [HttpPost("staff/time/{id}")]
        public IActionResult StaffSetAuctionTime(int id, [FromBody] AuctionDateRequest request)
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
            _auctionService.StaffSetAuctionTime(id,
                (DateTime)request.RegistrationStart,
                (DateTime)request.RegistrationEnd,
                (DateTime)request.StartedAt,
                (DateTime)request.EndedAt);

            _backgroundJobClient.Schedule(() => HostAuction(id, (int)AuctionStatus.Opened), (DateTime)request.StartedAt);
            _backgroundJobClient.Schedule(() => EndAuction(id, true), (DateTime)request.EndedAt);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Set auction time successfully",
                Data = null
            });
        }

        [HttpPost("order/create")]
        public IActionResult CreateAuctionOrder([FromBody] AuctionOrderRequest request) {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            
            if (user.Role != (int) Role.Buyer && user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _auctionService.CreateAuctionOrder(userId, request);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Create auction order successfully",
                Data = null
            });
        }
        // Change Auction Status
        [NonAction]
        public async Task HostAuction(int auctionId, int status)
        {
            var statusUpdated = _auctionService.HostAuction(auctionId, status);

            if (status == (int)AuctionStatus.Opened && statusUpdated)
            {
                var auction = _auctionService.GetAuctionById(auctionId);
                if (auction == null) return;
                await _hubContext.Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveAuctionOpen", auction.Title);
            }
        }

        // Change Auction Status
        [NonAction]
        public async Task<WinnerResponse?> EndAuction(int auctionId, bool scheduled = true)
        {
            var auction = _auctionService.GetAuctionById(auctionId);
            //if (scheduled && DateTime.Now < auction.EndedAt)
            //{
            //    throw new Exception("404: EndedDate Changed");
            //}

            var winnerBid = _auctionService.EndAuction(auctionId);
            User winner;
            WinnerResponse mappedWinner;
            if (winnerBid != null)
            {
                winner = _userService.Get((int)winnerBid.BidderId);
                mappedWinner = _mapper.Map<WinnerResponse>(winner);
                mappedWinner.FinalBid = winnerBid.BidAmount;
            }
            else
            {
                mappedWinner = null;
            }

            await _hubContext.Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveAuctionEnd", auction.Title);
            return mappedWinner;
        }
    }
}
