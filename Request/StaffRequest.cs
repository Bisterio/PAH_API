using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class StaffRequest {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string? Phone { get; set; }
        public string? ProfilePicture { get; set; }
        [Required]
        public int? Gender { get; set; }
        [Required]
        public DateTime? Dob { get; set; }
        [Required]
        public int Status { get; set; }
    }
}
