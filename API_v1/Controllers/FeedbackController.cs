using API.Request;
using API.Response;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFeedbackService _feedbackService;
        private readonly IMapper _mapper;

        public FeedbackController(IFeedbackService feedbackService, IMapper mapper)
        {
            _feedbackService = feedbackService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            Feedback feedback = _feedbackService.GetById(id);
            return Ok(new BaseResponse { Code = 200, Message = "Get feedback successfully", Data = feedback });
        }

        [HttpGet]
        [Route("product")]
        public IActionResult GetAll(int productId)
        {
            List<Feedback> feedbackList = _feedbackService.GetAll(productId);
            return Ok(new BaseResponse { Code = 200, Message = "Get all feedback successfully", Data = feedbackList });
        }

        [HttpPost]
        public IActionResult Create([FromBody] FeedbackRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _feedbackService.CreateFeedback(_mapper.Map<Feedback>(request));
            return Ok(new BaseResponse { Code = 200, Message = "Feedback successfully", Data = null });
        }
    }
}
