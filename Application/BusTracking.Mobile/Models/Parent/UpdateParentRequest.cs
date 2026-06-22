namespace BusTracking.Mobile.Models.Parent
{
    public class UpdateParentRequest
    {
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public List<string> StudentCodes { get; set; } = [];
        public bool IsActive { get; set; } = true;
    }
}
