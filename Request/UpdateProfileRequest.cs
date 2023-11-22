using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class UpdateProfileRequest : IValidatableObject{
        [Required(ErrorMessage = "Không được để trống tên")]
        [StringLength(255)]
        public string Name { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }

        [Required(ErrorMessage = "Không được để trống số điện thoại")]
        [Phone]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Không được để trống ảnh đại diện")]
        [Url]
        public string ProfilePicture { get; set; }

        [Required(ErrorMessage = "Không được để trống giới tính")]
        [Range(0, 1)]
        public int Gender { get; set; }

        [Required(ErrorMessage = "Không được để trống ngày sinh")]
        public DateTime Dob { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            int age = DateTime.Now.Year - Dob.Year;
            if (DateTime.Now.DayOfYear < Dob.DayOfYear) age = age - 1;
            if (age < 18) {
                yield return new ValidationResult($"Người dùng phải hơn 18 tuổi", new[] { nameof(Dob) });
            }
        }
    }
}
