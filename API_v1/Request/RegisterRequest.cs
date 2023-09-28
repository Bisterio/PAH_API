namespace API.Request {
    public class RegisterRequest {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Phone { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
    }
}
