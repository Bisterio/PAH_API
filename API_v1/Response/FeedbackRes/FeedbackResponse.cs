namespace API.Response.FeedbackRes
{
    public class FeedbackResponse
    {
        public int Id { get; set; }
        public double? Ratings { get; set; }
        public string? BuyerFeedback { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
