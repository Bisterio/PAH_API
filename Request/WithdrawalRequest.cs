using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Request {
    public class WithdrawalRequest : IValidatableObject{
        [Required]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(20)]
        public string Bank { get; set; }
        [Required]
        public string BankNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (Amount <= 0) {
                yield return new ValidationResult(
                    $"Amount must be greater than 0",
                    new[] { nameof(Amount) });
            }
            var bankData = GetBankData();
            if (!bankData.Contains(Bank)) {
                yield return new ValidationResult(
                    $"Your bank: \"{Bank}\" is not in our bank list",
                    new[] { nameof(Bank) });
            }
            
            if (!BankNumber.All(c => c >= '0' && c <= '9')) {
                yield return new ValidationResult(
                    $"Your bank number: \"{BankNumber}\" must contains only number",
                    new[] { nameof(Bank) });
            }
        }

        public string[] GetBankData() {
            JsonSerializerOptions _options = new() {
                PropertyNameCaseInsensitive = true
            };
            string path = System.IO.Directory.GetParent(Environment.CurrentDirectory).ToString() + "/Request/Data/Bank.json";
            //var path = "./../Request/Data/Bank.json";
            using FileStream json = File.OpenRead(path);
            return JsonSerializer.Deserialize<string[]>(json, _options);
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
