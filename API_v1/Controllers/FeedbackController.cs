using API.ErrorHandling;
using API.Request;
using API.Response;
using AutoMapper;
using DataAccess;
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
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get feedback successfully", Data = feedback });
        }

        [HttpGet]
        [Route("product")]
        public IActionResult GetAll(int productId, [FromQuery] PagingParam pagingParam)
        {
            List<Feedback> feedbackList = _feedbackService.GetAll(productId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get all feedbacks successfully", Data = feedbackList });
        }

        [HttpPost]
        public IActionResult Create([FromBody] FeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _feedbackService.CreateFeedback(_mapper.Map<Feedback>(request));
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Feedback successfully", Data = null });
        }
    }
}
