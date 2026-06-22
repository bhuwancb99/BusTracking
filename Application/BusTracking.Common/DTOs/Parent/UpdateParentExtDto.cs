namespace BusTracking.Common.DTOs.Parent
{
    public class UpdateParentExtDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NewPassword { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> StudentCodes { get; set; } = [];
    }
}
