using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Respon;
using Respon.UserRes;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper, ISellerService sellerService)
        {
            _userService = userService;
            _mapper = mapper;
            _sellerService = sellerService;
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
            if(userId == -1)
            {
                return Unauthorized(new ErrorDetails
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You must login to use this." 
                });
            }
            var user = _userService.Get(userId);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get user successfully", 
                Data = _mapper.Map<UserDetailResponse>(user)
            });
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetByUserId(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You must login to use this."
                });
            }
            var user = _userService.Get(id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get user successfully",
                Data = _mapper.Map<UserDetailResponse>(user)
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<User> userList = _userService.GetAll();
            List<UserResponse> responses = _mapper.Map<List<UserResponse>>(userList);
            return Ok(new BaseResponse 
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all users successfully", 
                Data = responses
            });
        }

        [Authorize]
        [HttpGet("customer")]
        public IActionResult GetAllBuyerAndSeller()
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Staff)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<User> userList = _userService.GetAllBuyersSellers();
            List<UserResponse> mappedList = _mapper.Map<List<UserResponse>>(userList);
            CustomerListCountResponse response = new CustomerListCountResponse()
            {
                Count = userList.Count,
                CustomerList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all customers successfully",
                Data = response
            });
        }

        [Authorize]
        [HttpGet("/api/staff")]
        public IActionResult GetAllStaffs()
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<User> staffList = _userService.GetAllStaffs();
            List<StaffResponse> responses = _mapper.Map<List<StaffResponse>>(staffList);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all staffs successfully",
                Data = responses
            });
        }

        [Authorize]
        [HttpGet("deactivate")]
        public IActionResult SelfDeactivate()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            _userService.Deactivate(user);
            return Ok(new BaseResponse
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Self deactivate successfully", 
                Data = null
            });
        }

        [Authorize]
        [HttpGet("deactivate/{id}")]
        public IActionResult Deactivate(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            };
            _userService.Deactivate(_userService.Get(id));
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Deactivate successfully",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("reactivate/{id}")]
        public IActionResult Reactivate(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }            
            _userService.Reactivate(_userService.Get(id));
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Reactivate successfully",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("seller/approve")]
        public IActionResult AcceptSeller(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager)))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            var seller = _sellerService.GetSeller(id);
            _userService.AcceptSeller(seller);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Approve seller successfully",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("seller/reject")]
        public IActionResult RejectSeller(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager)))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            var seller = _sellerService.GetSeller(id);
            _userService.RejectSeller(seller);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Approve seller successfully",
                Data = null
            });
        }
    }
}
