using API.Request;
using API.Response;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] PagingParam pagingParam)
        {
            List<Category> categoryList = _categoryService.GetAll()
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();

            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK,
                Message = "Get all categories successfully",
                Data = categoryList 
            });
        }
    }
}
