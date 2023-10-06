﻿namespace API.Response.AddressRes {
    public class AddressResponse {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public int? DistrictId { get; set; }
        public string? Ward { get; set; }
        public string? WardCode { get; set; }
        public string? Street { get; set; }
        public int Type { get; set; }
        public bool? IsDefault { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
