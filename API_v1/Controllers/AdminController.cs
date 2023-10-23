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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase {
        private readonly IMapper _mapper;
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;

        public AdminController(IMapper mapper, IAdminService adminService, IUserService userService) {
            _mapper = mapper;
            _adminService = adminService;
            _userService = userService;
        }

        private int GetUserIdFromToken() {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpPost("staff")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult AddStaff([FromBody] StaffRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _adminService.CreateStaff(_mapper.Map<User>(request));
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Create staff successfully",
                Data = null
            });
        }
        
        [HttpPatch("staff")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult EditStaff([FromBody] StaffRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _adminService.UpdateStaff(_mapper.Map<User>(request));
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Create staff successfully",
                Data = null
            });
        }

        [HttpGet("account")]
        public IActionResult ViewAllAccount([FromQuery] AccountParam accountParam, [FromQuery] PagingParam pagingParam) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            var list = _adminService.GetAccounts(accountParam);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Get all accounts successfully",
                Data = new { 
                    Count = list.Count,
                    List = list.Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).Select(p => _mapper.Map<UserResponse>(p)).ToList()
                }
            });
        }
        
        [HttpPatch("account")]
        public IActionResult EditStatusAccount([FromBody] AccountUpdate request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            _adminService.UpdateStatusAccount(request.Id, request.Status);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Update account status successfully",
                Data = null
            });
        }

        public class AccountUpdate {
            [Required]
            public int Id { get; set; }
            [Required]
            [Range(0, 1)]
            public int Status { get; set; }
        }
    }
}
