using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request
{
    public class LoginGoogleRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string Name { get; set; } = null!;
        public string? ProfilePicture { get; set; }
    }
}
