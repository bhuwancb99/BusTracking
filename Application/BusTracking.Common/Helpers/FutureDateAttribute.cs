using System.ComponentModel.DataAnnotations;

namespace BusTracking.Common.Helpers
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && DateOnly.TryParse(s, out var d))
                return d >= DateOnly.FromDateTime(DateTime.Today)
                    ? ValidationResult.Success
                    : new ValidationResult("Date must be today or in the future.");
            return ValidationResult.Success;
        }
    }
}
