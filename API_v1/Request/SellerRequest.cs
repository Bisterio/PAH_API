using System.ComponentModel.DataAnnotations;

namespace API.Request
{
    public class SellerRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Phone { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string RecipientName { get; set; }
        [Required]
        public string RecipientPhone { get; set; }
        [Required]
        public string Province { get; set; }
        [Required]
        public int ProvinceId { get; set; }
        [Required]
        public string District { get; set; }
        [Required]
        public int DistrictId { get; set; }
        [Required]
        public string Ward { get; set; }
        [Required]
        public string WardCode { get; set; }
        [Required]
        public string Street { get; set; }
    }
}
