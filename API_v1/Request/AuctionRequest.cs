using DataAccess.Models;

namespace API.Request
{
    public class AuctionRequest
    {
        public int? ProductId { get; set; }
        public string Title { get; set; } = null!;
        public decimal StartingPrice { get; set; }
        public decimal Step { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
