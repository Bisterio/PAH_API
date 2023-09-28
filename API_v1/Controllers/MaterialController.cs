using API.Response;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Implement;

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
            return Ok(new BaseResponse { Code = 200, Message = "Get all categories successfully", Data = materialList });
        }
    }
}
