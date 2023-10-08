using API.Response;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Implement;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            List<Material> materialList = _materialService.GetAll();
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get all materials successfully", Data = materialList });
        }
    }
}
