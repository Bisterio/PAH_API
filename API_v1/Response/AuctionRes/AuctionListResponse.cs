﻿namespace API.Response.AuctionRes
{
    public class AuctionListResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; } = null!;
        public decimal EntryFee { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string ImageUrl { get; set; }
    }
}
