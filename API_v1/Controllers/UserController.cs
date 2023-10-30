using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Request.Param;
using Respon;
using Respon.UserRes;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] PagingParam pagingParam)
        {
            List<User> userList = _userService.GetAll().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<UserResponse> mappedList = _mapper.Map<List<UserResponse>>(userList);
            UserListCountResponse responses = new UserListCountResponse()
            {
                Count = _userService.GetAll().Count(),
                UserList = mappedList
            };
            return Ok(new BaseResponse 
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all users successfully", 
                Data = responses
            });
        }

        [HttpGet("customer")]
        public IActionResult GetAllBuyerAndSeller([FromQuery] PagingParam pagingParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Staff && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<User> userList = _userService.GetAllBuyersSellers()
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList(); 
            int count = _userService.GetAllBuyersSellers().Count();
            List<UserResponse> mappedList = _mapper.Map<List<UserResponse>>(userList);
            CustomerListCountResponse response = new CustomerListCountResponse()
            {
                Count = count,
                CustomerList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all customers successfully",
                Data = response
            });
        }

        [HttpGet("/api/staff")]
        public IActionResult GetAllStaffs([FromQuery] PagingParam pagingParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<User> staffList = _userService.GetAllStaffs().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<StaffResponse> mappedList = _mapper.Map<List<StaffResponse>>(staffList);
            StaffListCountResponse response = new StaffListCountResponse()
            {
                Count = _userService.GetAllStaffs().Count(),
                StaffList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all staffs successfully",
                Data = response
            });
        }

        [HttpGet("/api/staff/available")]
        public IActionResult GetAllAvailableStaffs()
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<User> staffList = _userService.GetAvailableStaffs();
            List<StaffResponse> responses = _mapper.Map<List<StaffResponse>>(staffList);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get all staffs successfully",
                Data = responses
            });
        }

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

        [HttpGet("reactivate/request")]
        public IActionResult GetReactivateRequestList()
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
            };
            var reactivateRequests = _userService.GetReactivateRequestList();
            List<UserResponse> responses = _mapper.Map<List<UserResponse>>(reactivateRequests);    
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Get reactivate requests successfully",
                Data = responses
            });
        }

        [HttpGet("deactivate/{id}")]
        public IActionResult Deactivate(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
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

        [HttpGet("reactivate/{id}")]
        public IActionResult Reactivate(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
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

        [HttpGet("seller/approve")]
        public IActionResult AcceptSeller(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)))
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

        [HttpGet("seller/reject")]
        public IActionResult RejectSeller(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)))
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
                Message = "Reject seller successfully",
                Data = null
            });
        }

        [HttpPatch("updateprofile")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult UpdateUser([FromBody] UpdateProfileRequest request) {
            var userId = GetUserIdFromToken();
            _userService.UpdateProfile(userId, request);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Update profile successfully",
                Data = null
            });
        }
    }
}
