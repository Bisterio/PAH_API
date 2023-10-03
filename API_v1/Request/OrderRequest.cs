using System.ComponentModel.DataAnnotations;

namespace API.Request {
    public class OrderRequest {
    }

    public class CheckoutRequest {
        [Required]
        public List<int> ProductIds { get; set; }
        [Required]
        public int AddressId { get; set; }
    }

    public class ConfirmOrderRequest {
        [Required]
        public int Status { get; set; }
        [Required]
        public string message { get; set; }
    }
}
