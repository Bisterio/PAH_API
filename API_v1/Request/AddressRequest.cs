using System.ComponentModel.DataAnnotations;

namespace API.Request {
    public class AddressRequest {
        [Required]
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
        [Required]
        public int Type { get; set; }
    }
}
