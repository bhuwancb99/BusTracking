namespace BusTracking.Common.Entities
{
    public class BusTypeMaster : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Bus> Buses { get; set; } = [];
    }
}
