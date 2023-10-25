using DataAccess;
using DataAccess.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Request;
using Request.ThirdParty.GHN;
using Request.ThirdParty.Zalopay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Implement {
    public class OrderService : IOrderService {
        private readonly IOrderDAO _orderDAO;
        private readonly IProductDAO _productDAO;
        private readonly IAddressDAO _addressDAO;
        private readonly IProductImageDAO _productImageDAO;
        private readonly IOrderCancelDAO _orderCancelDAO;
        private readonly IWalletService _walletService;
        private readonly IConfiguration _config;
        private IHttpClientFactory _httpClientFactory;
        private IBackgroundJobClient _backgroundJobClient;

        public OrderService(IOrderDAO orderDAO, 
            IProductDAO productDAO, IAddressDAO addressDAO, 
            IProductImageDAO productImageDAO, IOrderCancelDAO orderCancelDAO,
            IWalletService walletService, IConfiguration config,
            IHttpClientFactory httpClientFactory, IBackgroundJobClient backgroundJobClient) {
            _orderDAO = orderDAO;
            _productDAO = productDAO;
            _addressDAO = addressDAO;
            _productImageDAO = productImageDAO;
            _orderCancelDAO = orderCancelDAO;
            _walletService = walletService;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _backgroundJobClient = backgroundJobClient;
        }

        public void SellerCancelOrder(int sellerId, int orderId, string reason) {
            var order = _orderDAO.Get(orderId);

            if (order == null) throw new Exception("404: Order not found");
            if (sellerId != order.SellerId || order.Status != (int) OrderStatus.WaitingSellerConfirm) throw new Exception("401: You are not allowed to cancel this order");

            order.Status = (int) OrderStatus.CancelledBySeller;
            _orderDAO.UpdateOrder(order);
            _orderCancelDAO.Create(new OrderCancellation { Id = order.Id, Reason = reason });
        }

        public void ApproveCancelOrderRequest(int sellerId, int orderId) {
            var order = _orderDAO.Get(orderId);

            if (order == null) throw new Exception("404: Order not found");
            if (sellerId != order.SellerId || order.Status != (int) OrderStatus.CancelApprovalPending) throw new Exception("401: You are not allowed to approve cancel this order");

            order.Status = (int) OrderStatus.CancelledByBuyer;
            _orderDAO.UpdateOrder(order);
            _orderCancelDAO.Create(new OrderCancellation { Id = order.Id, Reason = "Buyer cancelled" });
        }

        public void CancelOrderRequest(int buyerId, int orderId) {
            var order = _orderDAO.Get(orderId);

            if (order == null) throw new Exception("404: Order not found");
            if (buyerId != order.BuyerId || order.Status != (int) OrderStatus.WaitingSellerConfirm) throw new Exception($"401: You are not allowed to cancel this order with status: {order.Status}");

            order.Status = (int) OrderStatus.CancelApprovalPending;
            _orderDAO.UpdateOrder(order);
        }

        public void Create(Order order) {
            _orderDAO.Create(order);
        }

        public void Checkout(CheckoutRequest request, int buyerId, int addressId) {
            var address = _addressDAO.Get(addressId);
            if (address == null) throw new Exception("404: Address not found when checkout");
            if (address.CustomerId != buyerId) throw new Exception("401: Address does not associate with current buyer");
            //if (address.Type != (int) AddressType.Delivery) throw new Exception("401: Address type must be delivery");

            var dateNow = DateTime.Now;
            var totalWithShip = request.Total;
            request.Order.ForEach(p => totalWithShip += p.ShippingCost);

            var wallet = _walletService.GetByCurrentUser(buyerId);
            if (wallet == null) {
                throw new Exception("400: Wallet not found");
            }
            if (wallet.AvailableBalance < totalWithShip && request.PaymentType == (int)PaymentType.Wallet) {
                throw new Exception("400: Wallet not enough to create order");
            }

            foreach (var order in request.Order) {
                List<Product> products = new List<Product>();
                //totalWithShip += order.ShippingCost;
                
                Order insert = new Order {
                    BuyerId = buyerId,
                    SellerId = order.SellerId,
                    RecipientName = address.RecipientName,
                    RecipientPhone = address.RecipientPhone,
                    RecipientAddress = address.Street + ", " + address.Ward + ", " + address.District + ", " + address.Province,
                    OrderDate = dateNow,
                    Status = (int) OrderStatus.WaitingSellerConfirm,
                    ShippingCost = order.ShippingCost,
                    TotalAmount = order.Total,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var product in order.Products) {
                    var dbProduct = _productDAO.GetProductById(product.Id);
                    if (dbProduct == null) {
                        throw new Exception("404: Product not found when create order");
                    }
                    if (!CheckPrice(product.Price, dbProduct.Price)) {
                        throw new Exception("400: Price does not match with product in database");
                    }
                    if (order.SellerId != dbProduct.SellerId) {
                        throw new Exception("400: Seller does not match with seller's product in database");
                    }
                    insert.OrderItems.Add(new OrderItem {
                        ProductId = dbProduct.Id,
                        Price = dbProduct.Price,
                        Quantity = product.Amount,
                        ImageUrl = _productImageDAO.GetByProductId(dbProduct.Id).FirstOrDefault().ImageUrl
                    });
                    //insert.TotalAmount += dbProduct.Price * product.Amount;
                }
                _orderDAO.Create(insert);
            }
            switch (request.PaymentType) {
                case (int)PaymentType.Wallet:
                    CheckoutWallet(totalWithShip, buyerId, dateNow); 
                    break;
                case (int) PaymentType.Zalopay:
                    CheckoutZalopay(); 
                    break;
                default:
                    break;
            }
        }

        private void CheckoutWallet(decimal total, int buyerId, DateTime now) {
            var orderList = _orderDAO.GetAllByBuyerIdAfterCheckout(buyerId, now).ToList();
            orderList.ForEach(order => {
                _walletService.CheckoutWallet(buyerId, order.Id, (int) OrderStatus.WaitingSellerConfirm);
            });
        }

        private void CheckoutZalopay() {

        }

        private bool CheckPrice(decimal requestPrice, decimal dbPrice) {
            return requestPrice == dbPrice;
        }

        public List<Order> GetAll(int status) {
            var a = status == 0 ? _orderDAO.GetAllOrder().ToList() : _orderDAO.GetAllOrder().Where(p => p.Status == status).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetByBuyerId(int buyerId, int status) {
            var a = status == 0 ? _orderDAO.GetAllByBuyerId(buyerId).ToList() : _orderDAO.GetAllByBuyerId(buyerId).Where(p => p.Status == status).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetBySellerId(int sellerId, int status) {
            var a = status == 0 ? _orderDAO.GetAllBySellerId(sellerId).ToList() : _orderDAO.GetAllBySellerId(sellerId).Where(p => p.Status == status).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }


        public Order UpdateOrderStatus(int sellerId, int status, int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Order not found");
            }

            if (sellerId !=  order.SellerId 
                || order.Status == (int)OrderStatus.CancelledByBuyer 
                || order.Status == (int) OrderStatus.CancelledBySeller 
                || order.Status == (int) OrderStatus.CancelApprovalPending) {
                throw new Exception("401: You are not allowed to update this order");
            }

            order.Status = status;
            _orderDAO.UpdateOrder(order);
            return _orderDAO.Get(orderId);
        }

        public Order Get(int orderId) {
            var order = _orderDAO.Get(orderId);
            order.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); });
            return order;
        }

        public async Task DefaultShippingOrder(int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Order not found when create shipping");
            }

            if (order.Status != (int) OrderStatus.ReadyForPickup) {
                throw new Exception("401: This order cannot be assigned to be delivered");
            }
            string orderShippingCode = _config["API3rdParty:GHN:dev:defaultShippingCode"];
            order.OrderShippingCode = orderShippingCode;
            _orderDAO.UpdateOrder(order);
            await CheckStatusShippingOrder(orderId, orderShippingCode);
        }

        public async Task CheckStatusShippingOrder(int orderId, string orderShippingCode) {
            //Call to GHN
            var client = _httpClientFactory.CreateClient("GHN");
            StringContent content = new(
                JsonSerializer.Serialize(new {
                    order_code = orderShippingCode
                }),
                Encoding.UTF8,
                "application/json");
            var httpResponse = await client.PostAsync("v2/shipping-order/detail", content);
            if (!httpResponse.IsSuccessStatusCode) {
                _backgroundJobClient.Schedule(() => CheckStatusShippingOrder(orderId, orderShippingCode), DateTime.Now.AddMinutes(10));
                return;
            }

            //Get data
            var responseData = await httpResponse.Content.ReadAsAsync<ShippingOrder.Root>();
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Order not found when updating after shipping");
            }

            //Ready for pickup => Delivering
            if (order.Status == (int) OrderStatus.ReadyForPickup) {
                var log = responseData.data.log.Where(p => p.status.Contains("picked"));
                if (log.Any()) {
                    order.Status = (int) OrderStatus.Delivering;
                    _orderDAO.UpdateOrder(order);
                }
                _backgroundJobClient.Schedule(() => CheckStatusShippingOrder(orderId, orderShippingCode), DateTime.Now.AddMinutes(60*24));
                return;
            }
            
            //Delivering => Delivered
            if (order.Status == (int) OrderStatus.Delivering) {
                var log = responseData.data.log.Where(p => p.status.Contains("delivered"));
                if (log.Any()) {
                    order.Status = (int) OrderStatus.Delivered;
                    _orderDAO.UpdateOrder(order);
                    _backgroundJobClient.Schedule(() => DoneOrder(orderId), DateTime.Now.AddMinutes(60 * 24));
                }
                return;
            }
        }

        public void DoneOrder(int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Order not found when updating to delivered");
            }
            if (order.Status == (int) OrderStatus.Done) {
                return;
            }
            order.Status = (int) OrderStatus.Done;
            _orderDAO.UpdateOrder(order);
        }

        public void CreateShippingOrder(int orderId) {
            //Update Order status
            //Create shipping order by calling ghn
        }
    }
}
