using API.ErrorHandling;
using API.Request;
using API.Response;
using API.Response.OrderRes;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IUserService userService, IMapper mapper) {
            _orderService = orderService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get() {
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Get order list successfully", Data = _orderService.GetAll().Select(p => _mapper.Map<OrderResponse>(p)) });
        }

        [HttpGet("/api/buyer/order")]
        public IActionResult GetByBuyerId() {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            var orders = _orderService.GetByBuyerId(id);
            orders.Select(p => _mapper.Map<OrderResponse>(p));
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Get Buyer's order list successfully", Data = orders});
        }
        
        [HttpGet("/api/seller/order")]
        public IActionResult GetBySellerId() {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            var orders = _orderService.GetBySellerId(id);
            orders.Select(p => _mapper.Map<OrderResponse>(p));
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Get Seller's order list successfully", Data = orders});
        }

        [HttpPost("/api/buyer/checkout")]
        public IActionResult Checkout([FromBody] CheckoutRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            _orderService.Create(request.ProductIds, user.Id, request.AddressId);
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Checkout successfully", Data = null });
        }

        private int GetUserIdFromToken() {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpPut("/api/seller/order/cancelrequest/{orderId:int}")]
        public IActionResult ApproveCancelRequest(int orderId, [FromBody] bool confirm) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            
            if (user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            if (confirm) {
                _orderService.ApproveCancelOrderRequest(id, orderId);
                return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Approve cancel request successfully", Data = null });
            }
            //implement deny flow
            return BadRequest(new ErrorDetails { StatusCode = (int) HttpStatusCode.InternalServerError, Message = "Not implemented" });
        }
        
        [HttpPost("/api/buyer/order/cancelrequest/{orderId:int}")]
        public IActionResult CreateCancelRequest(int orderId) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            
            if (user.Role != (int) Role.Buyer) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            _orderService.CancelOrderRequest(id, orderId);
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Create cancel request successfully", Data = null });
        }
        
        [HttpPost("/api/seller/order/{orderId:int}")]
        public IActionResult ConfirmOrder(int orderId, [FromBody] ConfirmOrderRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }
            
            if (user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not seller, not allowed to access this" });
            }
            
            if (request.Status == (int)OrderStatus.CancelledBySeller) {
                _orderService.SellerCancelOrder(id, orderId, request.message);
                return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Cancel order successfully", Data = null });
            }

            if (request.Status == (int) OrderStatus.ReadyForPickup) {
                _orderService.UpdateOrderStatus(id, request.Status, orderId);
                return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Confirm order successfully", Data = null });
            }
            return BadRequest(new ErrorDetails { StatusCode = (int) HttpStatusCode.BadRequest, Message = "Status not matching required information"});
        }
    }
}
