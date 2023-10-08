using API.ErrorHandling;
using API.Request;
using API.Response;
using API.Response.FeedbackRes;
using API.Response.ProductRes;
using API.Response.SellerRes;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISellerService _sellerService;
        private readonly IAddressService _addressService;
        private readonly IFeedbackService _feedbackService;

        public ProductController(IProductService productService, IUserService userService, IMapper mapper, IImageService imageService, 
            ISellerService sellerService, IAddressService addressService, IFeedbackService feedbackService)
        {
            _productService = productService;
            _userService = userService;
            _mapper = mapper;
            _imageService = imageService;
            _sellerService = sellerService;
            _addressService = addressService;
            _feedbackService = feedbackService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        private SellerResponse GetSellerResponse(int sellerId)
        {
            Seller seller = _sellerService.GetSeller(sellerId);
            SellerResponse sellerResponse = new SellerResponse();
            if (seller != null)
            {
                sellerResponse = _mapper.Map<SellerResponse>(seller);
                Address address = _addressService.GetByCustomerId(sellerId).Where(a => a.Type == (int)AddressType.Pickup && a.IsDefault == true).FirstOrDefault();
                sellerResponse.Province = address.Province;
                sellerResponse.WardCode = address.WardCode;
                sellerResponse.Ward = address.Ward;
                sellerResponse.DistrictId = address.DistrictId;
                sellerResponse.District = address.District;
                sellerResponse.Street = address.Street;
            }
            return sellerResponse;
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string? nameSearch, 
            [FromQuery] int materialId, [FromQuery] int categoryId, 
            [FromQuery] int type, [FromQuery] decimal priceMin, 
            [FromQuery] decimal priceMax, [FromQuery] int orderBy,
            [FromQuery] PagingParam pagingParam)
        {
            List<Product> productList = _productService.GetProducts(nameSearch, materialId, categoryId, type, priceMin, priceMax, orderBy)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<ProductListResponse> response = _mapper.Map<List<ProductListResponse>>(productList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.Id);
                item.ImageUrl = image.ImageUrl;
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get products successfully", Data = response });
        }

        [HttpGet("seller/{id}")]
        public IActionResult GetProductsBySellerId(int id, [FromQuery] PagingParam pagingParam)
        {
            List<Product> productList = _productService.GetProductsBySellerId(id)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            if (productList == null)
            {
                return NotFound(new ErrorDetails { StatusCode = 400, Message = "This seller is not exist" });
            }
            List<ProductListResponse> response = _mapper.Map<List<ProductListResponse>>(productList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.Id);
                item.ImageUrl = image.ImageUrl;
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get products by seller successfully", Data = response });
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            Product product = _productService.GetProductById(id);
            if(product == null)
            {
                return NotFound(new ErrorDetails { StatusCode = 400, Message = "This product is not exist" });
            }
            ProductDetailResponse response = _mapper.Map<ProductDetailResponse>(product);
            List<ProductImage> imageList = _imageService.GetAllImagesByProductId(id);
            List<string> imageUrls = imageList.Select(i => i.ImageUrl).ToList();
            response.ImageUrls = imageUrls;

            response.Seller = GetSellerResponse((int)product.SellerId);

            List<FeedbackResponse> feedbacks = _mapper.Map<List<FeedbackResponse>>(_feedbackService.GetAll(id));
            if(feedbacks == null || feedbacks.Count == 0)
            {
                feedbacks = new List<FeedbackResponse>();
            }
            response.Feedbacks = feedbacks;

            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Get product successfully", Data = response });
        }

        [HttpPost]
        public IActionResult RegisterProduct([FromBody] ProductRequest request)
        {
            //var userId = GetUserIdFromToken();
            //var user = _userService.Get(userId);
            //if (user == null || user.Role != (int) Role.Seller)
            //{
            //    return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            //}

            var auctionRequest = new AuctionRequest
            {
                Title = request.Title,
                Step = request.Step,
                StartedAt = request.StartedAt,
                EndedAt = request.EndedAt,
            };

            _productService.CreateProduct(_mapper.Map<Product>(request), _mapper.Map<Auction>(auctionRequest));
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Register product successfully", Data = null });
        }

        [HttpPatch("{id}")]
        public IActionResult EditProduct(int id, [FromBody] ProductRequest request)
        {
            //var userId = GetUserIdFromToken();
            //var user = _userService.Get(userId);
            //if (user == null || user.Role != (int)Role.Seller)
            //{
            //    return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            //}

            var auctionRequest = new AuctionRequest
            {
                Title = request.Title,
                Step = request.Step,
                StartedAt = request.StartedAt,
                EndedAt = request.EndedAt,
            };

            Product product = _productService.UpdateProduct(id, _mapper.Map<Product>(request), _mapper.Map<Auction>(auctionRequest));
            if (product == null)
            {
                return NotFound(new ErrorDetails { StatusCode = 400, Message = "This product is not exist" });
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Edit product successfully", Data = null });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            Product product = _productService.DeleteProduct(id);
            if (product == null)
            {
                return NotFound(new ErrorDetails { StatusCode = 400, Message = "This product is not exist" });
            }
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Edit product successfully", Data = null });
        }
    }
}
