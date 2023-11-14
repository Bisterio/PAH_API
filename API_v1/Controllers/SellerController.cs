using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Request;
using Respon;
using Respon.OrderRes;
using Respon.SellerRes;
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
        private readonly IAddressService _addressService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IAuctionService _auctionService;
        private readonly IMapper _mapper;

        public SellerController(IUserService userService, ISellerService sellerService, IMapper mapper, IAddressService addressService, 
            IProductService productService, IOrderService orderService, IAuctionService auctionService)
        {
            _userService = userService;
            _sellerService = sellerService;
            _mapper = mapper;
            _addressService = addressService;
            _productService = productService;
            _orderService = orderService;
            _auctionService = auctionService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [Authorize]
        [HttpGet("current")]
        public IActionResult Get()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
             //|| user.Role != (int)Role.Seller
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                { StatusCode =
                (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            var seller = _sellerService.GetSeller(userId);
            var address = _addressService.GetPickupBySellerId(userId);

            SellerDetailResponse response = _mapper.Map<SellerDetailResponse>(new Seller());

            if(seller != null)
            {
                response.Id = seller.Id;
                response.Name = seller.Name;
                response.Phone = seller.Phone;
                response.ProfilePicture = seller.ProfilePicture;
                response.RegisteredAt = seller.RegisteredAt;
                response.Ratings = seller.Ratings;
                response.Status = seller.Status;
                response.RecipientName = address.RecipientName;
                response.RecipientPhone = address.RecipientPhone;
                response.Province = address.Province;
                response.ProvinceId = address.ProvinceId;
                response.District = address.District;
                response.DistrictId = address.DistrictId;
                response.Ward = address.Ward;
                response.WardCode = address.WardCode;
                response.Street = address.Street;
            }
            
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get seller successfully",
                Data = response 
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var seller = _sellerService.GetSeller(id);
            var address = _addressService.GetPickupBySellerId(id);

            SellerDetailResponse response = _mapper.Map<SellerDetailResponse>(new Seller());

            if (seller != null)
            {
                response.Id = seller.Id;
                response.Name = seller.Name;
                response.Phone = seller.Phone;
                response.ProfilePicture = seller.ProfilePicture;
                response.RegisteredAt = seller.RegisteredAt;
                response.Ratings = seller.Ratings;
                response.Status = seller.Status;
                response.RecipientName = address.RecipientName;
                response.RecipientPhone = address.RecipientPhone;
                response.Province = address.Province;
                response.ProvinceId = address.ProvinceId;
                response.District = address.District;
                response.DistrictId = address.DistrictId;
                response.Ward = address.Ward;
                response.WardCode = address.WardCode;
                response.Street = address.Street;
            }

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get seller successfully",
                Data = response
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SellerRequest([FromBody] SellerRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            var shopId = await _sellerService.CreateShopIdAsync(request);

            Seller seller = new Seller()
            {
                Id = userId,
                Name = request.Name,
                Phone = request.Phone,
                ProfilePicture = request.ProfilePicture,
                Status = (int)SellerStatus.Pending,
                ShopId = shopId.ToString()
            };
            int existed = _sellerService.CreateSeller(userId, seller);

            Address address = new Address()
            {
                CustomerId = userId,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                Province = request.Province,
                ProvinceId = request.ProvinceId,
                District = request.District,
                DistrictId = request.DistrictId,
                Ward = request.Ward,
                WardCode = request.WardCode,
                Street = request.Street,
                Type = (int)AddressType.Pickup,
                IsDefault = true
            };
            if(existed == 0)
            {
                _addressService.Create(address);
                return Ok(new BaseResponse
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Request to become seller successfully",
                    Data = null
                });
            }
            else
            {
                _sellerService.UpdateSeller(seller);
                _addressService.UpdateSellerAddress(address, userId);
                return Ok(new BaseResponse
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Update seller successfully",
                    Data = null
                });
            }
        }

        [Authorize]
        [HttpGet("request")]
        public IActionResult GetSellerRequestList()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<Seller> sellerRequests = _sellerService.GetSellerRequestList();
            List<SellerRequestResponse> responses = _mapper.Map<List<SellerRequestResponse>>(sellerRequests);
            foreach (var item in responses)
            {
                var pickupAddress = _addressService.GetPickupBySellerId(item.Id);
                item.Province = pickupAddress.Province;
                item.DistrictId = pickupAddress.DistrictId;
                item.District = pickupAddress.District;
                item.WardCode = pickupAddress.WardCode;
                item.Ward = pickupAddress.Ward;
                item.Street = pickupAddress.Street;

                var sellerUser = _userService.Get(item.Id);
                item.UserName = sellerUser.Name;
                item.Email = sellerUser.Email;
                item.Phone = sellerUser.Phone;
                item.Gender = sellerUser.Gender;
                item.Dob = sellerUser.Dob;
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get seller requests successfully",
                Data = new {
                    Count = responses.Count,
                    List = responses
                }
            });
        }

        [Authorize]
        [HttpGet("dashboard")]
        public IActionResult GetDashboardFromCurrentSeller()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Seller))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            var sales = _sellerService.GetSalesCurrentSeller(userId);
            var sellingProducts = _productService.GetProductsBySellerId(userId)
                .Where(p => p.Status == (int)Status.Available && p.Type == (int)ProductType.ForSale).Count();
            var doneOrders = _orderService.GetBySellerId(userId, new List<int>() { (int)OrderStatus.Done }).Count();
            var processingOrders = _orderService.GetProcessingBySellerId(userId).Count();
            var totalOrders = _orderService.GetBySellerId(userId, new List<int>()).Count();
            var totalAuctions = _auctionService.GetAuctionBySellerId(userId, -1).Count();
            SellerSalesResponse response = new SellerSalesResponse()
            {
                TotalSales = sales,
                SellingProduct = sellingProducts,
                ProcessingOrders = processingOrders,
                DoneOrders = doneOrders,
                TotalOrders = totalOrders,
                TotalAuctions = totalAuctions,
            };

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get sales of current seller successfully",
                Data = response
            });
        }
    }
}
