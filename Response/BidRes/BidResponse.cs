namespace Respon.BidRes
{
    public class BidResponse
    {
        public int Id { get; set; }
        public int? AuctionId { get; set; }
        public int? BidderId { get; set; }
        public string BidderName { get; set; }
        public decimal? BidAmount { get; set; }
        public DateTime? BidDate { get; set; }
        public int Status { get; set; }
    }
}
