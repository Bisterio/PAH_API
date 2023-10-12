using API.ErrorHandling;
using API.Response;
using API.Response.UserRes;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
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

        [HttpGet("/api/staff")]
        public IActionResult GetAllStaffs()
        {
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
        [HttpPatch("deactivate")]
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
    }
}
