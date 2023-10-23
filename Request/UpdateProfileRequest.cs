using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class UpdateProfileRequest : IValidatableObject{
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [Url]
        public string ProfilePicture { get; set; }

        [Required]
        [Range(0, 1)]
        public int Gender { get; set; }

        [Required]
        public DateTime Dob { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            int age = DateTime.Now.Year - Dob.Year;
            if (DateTime.Now.DayOfYear < Dob.DayOfYear) age = age - 1;
            if (age < 18) {
                yield return new ValidationResult($"Date of birth must be 18 years or older", new[] { nameof(Dob) });
            }
        }
    }
}
