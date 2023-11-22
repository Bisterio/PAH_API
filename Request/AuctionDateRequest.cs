﻿using Request.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request
{
    public class AuctionDateRequest : IValidatableObject
    {
        [Required]
        public DateTime? StartedAt { get; set; }
        [Required]
        public DateTime? EndedAt { get; set; }
        [Required]
        public DateTime? RegistrationStart { get; set; }
        [Required]
        public DateTime? RegistrationEnd { get; set; }
        [Required]
        public decimal Step { get; set; } = 50000;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RegistrationEnd <= RegistrationStart)
            {
                yield return new ValidationResult(
                    $"Ngày đóng đăng kí phải sau ngày mở đăng kí",
                    new[] { nameof(RegistrationEnd) });
            } 
            else if (Step < 50000)
            {
                yield return new ValidationResult(
                    $"Bước giá của cuộc đấu giá phải tối thiểu là 50000VND",
                    new[] { nameof(Step) });
            }
            else if (StartedAt <= RegistrationEnd)
            {
                yield return new ValidationResult(
                    $"Ngày bắt đầu phải sau ngày đóng đăng kí",
                    new[] { nameof(StartedAt) });
            }
            else if (EndedAt <= StartedAt)
            {
                yield return new ValidationResult(
                    $"Ngày kết thúc phải sau ngày bắt đầu",
                    new[] { nameof(EndedAt) });
            }
        }
    }
}
