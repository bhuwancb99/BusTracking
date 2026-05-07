using System.ComponentModel.DataAnnotations;

namespace BusTracking.Common.Helpers
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var pwd = value?.ToString();
            if (string.IsNullOrEmpty(pwd)) return ValidationResult.Success;  // required handled separately

            var errors = new List<string>();
            if (pwd.Length < 8) errors.Add("at least 8 characters");
            if (!pwd.Any(char.IsUpper)) errors.Add("one uppercase letter");
            if (!pwd.Any(char.IsLower)) errors.Add("one lowercase letter");
            if (!pwd.Any(char.IsDigit)) errors.Add("one digit");
            if (!pwd.Any(c => "!@#$%^&*()_+-=[]{}".Contains(c))) errors.Add("one special character");

            return errors.Count == 0
                ? ValidationResult.Success
                : new ValidationResult($"Password must contain: {string.Join(", ", errors)}.");
        }
    }
}
