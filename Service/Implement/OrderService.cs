using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Service.CustomRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement {
    public class OrderService : IOrderService {
        private readonly IOrderDAO _orderDAO;
        private readonly IProductDAO _productDAO;
        private readonly IAddressDAO _addressDAO;
        private readonly IProductImageDAO _productImageDAO;
        private readonly IOrderCancelDAO _orderCancelDAO;
        private readonly IWalletService _walletService;

        public OrderService(IOrderDAO orderDAO, 
            IProductDAO productDAO, IAddressDAO addressDAO, 
            IProductImageDAO productImageDAO, IOrderCancelDAO orderCancelDAO,
            IWalletService walletService) {
            _orderDAO = orderDAO;
            _productDAO = productDAO;
            _addressDAO = addressDAO;
            _productImageDAO = productImageDAO;
            _orderCancelDAO = orderCancelDAO;
            _walletService = walletService;
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
            if (address.Type != (int) AddressType.Delivery) throw new Exception("401: Address type must be delivery");

            var dateNow = DateTime.Now;
            var totalWithShip = request.Total;
            foreach (var order in request.Order) {
                List<Product> products = new List<Product>();
                totalWithShip += order.ShippingCost;
                
                Order insert = new Order {
                    BuyerId = buyerId,
                    SellerId = order.SellerId,
                    RecipientName = address.RecipientName,
                    RecipientPhone = address.RecipientPhone,
                    RecipientAddress = address.Street + ", " + address.Ward + ", " + address.District + ", " + address.Province,
                    OrderDate = dateNow,
                    Status = (int) OrderStatus.Pending,
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
            var wallet = _walletService.GetByCurrentUser(buyerId);
            if (wallet == null) {
                throw new Exception("400: Wallet not found");
            }
            if (wallet.AvailableBalance < total) {
                return;
            }

            var orderList = _orderDAO.GetAllByBuyerIdAfterCheckout(buyerId, now).ToList();
            orderList.ForEach(order => {
                _walletService.CheckoutWallet(buyerId, order.Id);
            });
        }

        private void CheckoutZalopay() {

        }

        private bool CheckPrice(decimal requestPrice, decimal dbPrice) {
            return requestPrice == dbPrice;
        }

        public List<Order> GetAll() {
            var a = _orderDAO.GetAllOrder().ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetByBuyerId(int buyerId) {
            var a = _orderDAO.GetAllByBuyerId(buyerId).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetBySellerId(int sellerId) {
            var a = _orderDAO.GetAllBySellerId(sellerId).ToList();
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
    }
}
