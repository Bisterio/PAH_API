using System.ComponentModel.DataAnnotations;

namespace Request {
    public class RegisterRequest {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public string? Phone { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
    }

    public class VerificationRequest {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
