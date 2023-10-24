using API.ErrorHandling;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
using DataAccess;
using DataAccess.Models;
using Hangfire.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Request.Param;
using Request.ThirdParty.GHN;
using Respon;
using Respon.OrderRes;
using Service;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors]
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
        public IActionResult Get([FromQuery] PagingParam pagingParam, [FromQuery] OrderParam orderParam) {
            var list = _orderService.GetAll(orderParam.Status);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Get order list successfully",
                Data = new {
                    Count = list.Count,
                    List = list.Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList()
                            .Select(p => _mapper.Map<OrderResponse>(p))
                }
            });
        }
        
        [HttpGet("{orderId:int}")]
        public IActionResult Get(int orderId) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "You are not logged in to access this" });
            }

            var order = _orderService.Get(orderId);
            if (order == null) {
                return NotFound(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.NotFound,
                    Message = "Order not found"
                });
            }
            //if (order.BuyerId != id && order.SellerId != id) {
            //    return Unauthorized(new ErrorDetails {
            //        StatusCode = (int) HttpStatusCode.Unauthorized,
            //        Message = "You are not allowed to access this order"
            //    });
            //}
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Get order successfully",
                Data = _mapper.Map<OrderResponse>(order) 
            });
        }

        [HttpGet("/api/buyer/order")]
        public IActionResult GetByBuyerId([FromQuery] PagingParam pagingParam, [FromQuery] OrderParam orderParam) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            var orders = _orderService.GetByBuyerId(id, orderParam.Status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            var responseOrders = orders.Select(p => _mapper.Map<OrderResponse>(p));
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Get Buyer's order list successfully", Data = responseOrders});
        }
        
        [HttpGet("/api/seller/order")]
        public IActionResult GetBySellerId([FromQuery] PagingParam pagingParam, [FromQuery] OrderParam orderParam) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            var orders = _orderService.GetBySellerId(id, orderParam.Status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            var responseOrders = orders.Select(p => _mapper.Map<OrderResponse>(p));
            return Ok(new BaseResponse { Code = (int) HttpStatusCode.OK, Message = "Get Seller's order list successfully", Data = responseOrders});
        }

        [HttpPost("/api/buyer/checkout")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult Checkout([FromBody] CheckoutRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this" 
                });
            }
            
            if (user.Role != (int) Role.Buyer && user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "You are not allowed to access this" 
                });
            }
            _orderService.Checkout(request, user.Id, request.AddressId);
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
            
            if (user.Role != (int) Role.Buyer && user.Role != (int) Role.Seller) {
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

        [HttpPost("/api/seller/order/deliver/{orderId:int}")]
        public async Task<IActionResult> DefaultShippingOrderAsync(int orderId) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            if (user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not seller, not allowed to access this" });
            }
            await _orderService.DefaultShippingOrder(orderId);
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Default order shipping applied", 
                Data = null 
            });
        }
        
        //[HttpPatch("/api/seller/order/deliver/{orderId:int}")]
        //public IActionResult CreateShippingOrder(int orderId) {
        //    return Ok(new BaseResponse { 
        //        Code = (int) HttpStatusCode.OK, 
        //        Message = "Done nothing successfully", 
        //        Data = null 
        //    });
        //}

        [HttpPost("/api/buyer/order/done/{orderId:int}")]
        public IActionResult BuyerConfirmDoneOrder(int orderId) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
            }

            if (user.Role != (int) Role.Seller && user.Role != (int) Role.Buyer) {
                return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not buyer, not allowed to access this" });
            }

            var order = _orderService.Get(orderId);
            if (order == null) {
                return NotFound(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.NotFound,
                    Message = "Order not found"
                });
            }
            if (order.BuyerId != id) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to done this order"
                });
            }
            if (order.Status != (int) OrderStatus.Delivered) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to done this order"
                });
            }
            _orderService.DoneOrder(orderId);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Done order successfully",
                Data = null
            });
        }
    }
}
