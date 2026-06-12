namespace BusTracking.Common.DTOs.BusType
{
    public class BusTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int BusCount { get; set; }      // number of buses using this type
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
