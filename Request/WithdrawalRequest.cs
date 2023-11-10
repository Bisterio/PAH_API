using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class WithdrawalRequest : IValidatableObject{
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int Bank { get; set; }
        [Required]
        public string BankNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (Amount <= 0) {
                yield return new ValidationResult(
                    $"Amount must be greater than 0",
                    new[] { nameof(Amount) });
            }
        }
    }

    public class UpdateWithdrawRequest {
        [Required]
        public int WithdrawalId { get; set; }
        [Required]
        [Range(2,3)]
        public int Status { get; set; }
    }
}
