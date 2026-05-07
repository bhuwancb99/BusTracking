using System.ComponentModel.DataAnnotations;

namespace BusTracking.Common.Helpers
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly string _startProp;
        public DateRangeAttribute(string startPropertyName) => _startProp = startPropertyName;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var startProp = ctx.ObjectType.GetProperty(_startProp);
            if (startProp == null) return ValidationResult.Success;

            var startVal = startProp.GetValue(ctx.ObjectInstance)?.ToString();
            if (!DateOnly.TryParse(startVal, out var start) ||
                !DateOnly.TryParse(value?.ToString(), out var end))
                return ValidationResult.Success;

            return end >= start
                ? ValidationResult.Success
                : new ValidationResult("End date must be on or after start date.");
        }
    }
}
