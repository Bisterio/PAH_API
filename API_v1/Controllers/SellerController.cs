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
        private readonly IMapper _mapper;

        public SellerController(IUserService userService, ISellerService sellerService, IMapper mapper, IAddressService addressService)
        {
            _userService = userService;
            _sellerService = sellerService;
            _mapper = mapper;
            _addressService = addressService;
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

        [Authorize]
        [HttpPost]
        public IActionResult SellerRequest([FromBody] SellerRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);

            Seller seller = new Seller()
            {
                Id = userId,
                Name = request.Name,
                Phone = request.Phone,
                ProfilePicture = request.ProfilePicture,
                Status = (int)SellerStatus.Pending
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
                Data = responses
            });
        }
    }
}
