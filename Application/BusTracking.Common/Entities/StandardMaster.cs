namespace BusTracking.Common.Entities
{
    public class StandardMaster
    {
        [Key]
        public int StandardId { get; set; }

        [Required, MaxLength(100)]
        public string StandardName { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
