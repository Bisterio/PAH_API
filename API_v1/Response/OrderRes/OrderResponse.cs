using API.Response.UserRes;
using DataAccess.Models;

namespace API.Response.OrderRes {
    public class OrderResponse {
        public int Id { get; set; }
        public int? BuyerId { get; set; }
        public int? SellerId { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? RecipientAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? ShippingCost { get; set; }
        public int Status { get; set; }

        public virtual SellerResponse? Seller { get; set; }
        public virtual ICollection<OrderItemResponse> OrderItems { get; set; }
    }
}
