using API.Request;
using API.Response;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private readonly IResponseService _responseService;
        private readonly IMapper _mapper;

        public ResponseController(IResponseService responseService, IMapper mapper)
        {
            _responseService = responseService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int feedbackId)
        {
            DataAccess.Models.Response response = _responseService.GetByFeedbackId(feedbackId);
            return Ok(new BaseResponse { Code = 200, Message = "Get response successfully", Data = response });
        }

        [HttpPost]
        public IActionResult Create([FromBody] ResponseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _responseService.Reply(_mapper.Map<DataAccess.Models.Response>(request));
            return Ok(new BaseResponse { Code = 200, Message = "Response successfully", Data = null });
        }
    }
}
