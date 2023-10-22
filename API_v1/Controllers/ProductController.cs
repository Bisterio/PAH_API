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
using Respon.ProductRes;
using Respon.SellerRes;
using Service;
using Service.Implement;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
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

        private SellerWithAddressResponse GetSellerResponse(int sellerId)
        {
            Seller seller = _sellerService.GetSeller(sellerId);
            SellerWithAddressResponse sellerResponse = new SellerWithAddressResponse();
            if (seller != null)
            {
                sellerResponse = _mapper.Map<SellerWithAddressResponse>(seller);
                sellerResponse.Province = null;
                sellerResponse.WardCode = null;
                sellerResponse.Ward = null;
                sellerResponse.DistrictId = null;
                sellerResponse.District = null;
                sellerResponse.Street = null;

                Address address = _addressService.GetByCustomerId(sellerId)
                    .Where(a => a.Type == (int)AddressType.Pickup && a.IsDefault == true)
                    .FirstOrDefault();
                if(address != null)
                {
                    sellerResponse.Province = address.Province;
                    sellerResponse.WardCode = address.WardCode;
                    sellerResponse.Ward = address.Ward;
                    sellerResponse.DistrictId = address.DistrictId;
                    sellerResponse.District = address.District;
                    sellerResponse.Street = address.Street;
                }
            }
            return sellerResponse;
        }

        private int CountAuctions(string? nameSearch, int materialId, int categoryId, int type, decimal priceMin, decimal priceMax)
        {
            int count = 0;
            count = _productService.GetProducts(nameSearch, materialId, categoryId, type, priceMin, priceMax, 0).Count();
            return count;
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

            List<ProductListResponse> mappedList = _mapper.Map<List<ProductListResponse>>(productList);
            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.Id);
                if(image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }
            }

            int count = CountAuctions(nameSearch, materialId, categoryId, type, priceMin, priceMax);

            ProductListCountResponse response = new ProductListCountResponse()
            {
                Count = count,
                ProductList = mappedList
            };

            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK,
                Message = "Get products successfully", 
                Data = response
            });
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
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }
            }
            return Ok(new BaseResponse
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Get products by seller successfully",
                Data = response 
            });
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

            List<FeedbackResponse> feedbacks = _mapper.Map<List<FeedbackResponse>>(_feedbackService.GetTop3Newest(id));
            foreach (var feedback in feedbacks)
            {
                feedback.BuyerName = _userService.Get(feedback.BuyerId).Name;
            }
            if(feedbacks == null || feedbacks.Count == 0)
            {
                feedbacks = new List<FeedbackResponse>();
            }
            response.Feedbacks = feedbacks;

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK, 
                Message = "Get product successfully", 
                Data = response
            });
        }

        [Authorize]
        [HttpPost]
        public IActionResult RegisterProduct([FromBody] ProductRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails 
                { StatusCode = (int)HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this"
                });
            }
            var auctionRequest = new AuctionRequest
            {
                Title = request.Title,
                Step = request.Step,
            };
            int productId = _productService.CreateProduct(_mapper.Map<Product>(request), _mapper.Map<Auction>(auctionRequest));
            List<string> imageUrlList = request.ImageUrlLists;
            foreach (var imageUrl in imageUrlList)
            {
                _imageService.SaveProductImage(productId, imageUrl);
            }
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Register product successfully", 
                Data = null 
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult EditProduct(int id, [FromBody] ProductRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails 
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this" 
                });
            }

            var auctionRequest = new AuctionRequest
            {
                Title = request.Title,
                Step = request.Step,
            };

            Product product = _productService.UpdateProduct(id, _mapper.Map<Product>(request), _mapper.Map<Auction>(auctionRequest));
            if (product == null)
            {
                return NotFound(new ErrorDetails 
                { 
                    StatusCode = 400, 
                    Message = "This product is not exist" 
                });
            }
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Edit product successfully", 
                Data = null 
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails 
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this" 
                });
            }
            Product product = _productService.DeleteProduct(id);
            if (product == null)
            {
                return NotFound(new ErrorDetails 
                { 
                    StatusCode = 400, 
                    Message = "This product is not exist" 
                });
            }
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Edit product successfully", 
                Data = null
            });
        }
    }
}
