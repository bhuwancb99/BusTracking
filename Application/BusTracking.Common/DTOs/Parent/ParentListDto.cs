namespace BusTracking.Common.DTOs.Parent
{
    public class ParentListDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> KidNames { get; set; } = [];
    }
}
