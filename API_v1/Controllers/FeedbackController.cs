using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Request.Param;
using Respon;
using Respon.FeedbackRes;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class FeedbackController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFeedbackService _feedbackService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public FeedbackController(IFeedbackService feedbackService, IMapper mapper, IUserService userService)
        {
            _feedbackService = feedbackService;
            _mapper = mapper;
            _userService = userService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            Feedback feedback = _feedbackService.GetById(id);
            FeedbackResponse response = _mapper.Map<FeedbackResponse>(feedback);
            response.BuyerName = _userService.Get(response.BuyerId).Name;
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy đánh giá thành công",
                Data = response 
            });
        }

        [HttpGet]
        [Route("product")]
        public IActionResult GetAll(int productId, [FromQuery] PagingParam pagingParam)
        {
            List<Feedback> feedbackList = _feedbackService.GetAll(productId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<FeedbackResponse> responses = _mapper.Map<List<FeedbackResponse>>(feedbackList);
            foreach (FeedbackResponse response in responses)
            {
                response.BuyerName = _userService.Get(response.BuyerId).Name;
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy tất cả đánh giá thành công",
                Data = responses
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] FeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }

            if (user.Role != (int)Role.Buyer && user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }

            _feedbackService.CreateFeedback(id, request.ProductId, request.BuyerFeedback, request.Ratings);
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Đánh giá sản phẩm thành công", 
                Data = null 
            });
        }
    }
}
