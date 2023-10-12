using DataAccess.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Service.CustomRequest
{
    public class OrderRequest
    {
    }

    public class ConfirmOrderRequest
    {
        [Required]
        public int Status { get; set; }
        [Required]
        public string message { get; set; }
    }

    public class CheckoutRequest : IValidatableObject
    {
        [Required]
        public List<CheckoutOrder> Order { get; set; }
        [Required]
        public decimal Total { get; set; }
        [Required]
        public int PaymentType { get; set; }
        [Required]
        public int AddressId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            decimal totalFromOrders = 0m;
            foreach (var order in Order)
            {
                totalFromOrders += order.Total;
            }
            if (totalFromOrders != Total)
            {
                yield return new ValidationResult(
                    $"Total calculated from Orders is {totalFromOrders} which is different from Total sent is {Total}",
                    new[] { nameof(Total) });
            }
        }
    }

    public class CheckoutOrder : IValidatableObject
    {
        [Required]
        public int SellerId { get; set; }
        [Required]
        public List<CheckoutProduct> Products { get; set; }
        [Required]
        public decimal Total { get; set; }
        [Required]
        public decimal ShippingCost { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            decimal totalFromProducts = 0m;
            foreach (var product in Products)
            {
                totalFromProducts += product.Price * product.Amount;
            }
            if (totalFromProducts != Total)
            {
                yield return new ValidationResult(
                    $"Total calculated from products is {totalFromProducts} which is different from Total sent is {Total}",
                    new[] { nameof(Total) });
            }
        }
    }

    public class CheckoutProduct
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Amount { get; set; }
    }
}
