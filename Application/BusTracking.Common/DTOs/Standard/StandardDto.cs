namespace BusTracking.Common.DTOs.Standard
{
    public class StandardDto
    {
        public int StandardId { get; set; }
        public string StandardName { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
