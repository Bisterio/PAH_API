namespace API.Response.UserRes
{
    public class SellerDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public decimal? Ratings { get; set; }
    }
}
