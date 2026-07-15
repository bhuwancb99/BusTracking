namespace BusTracking.Common.Entities
{
    public class BusImage : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int BusImageId { get; set; }
        public int BusId { get; set; }
        [Required, MaxLength(500)] public string ImageUrl { get; set; } = "";
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? UploadedBy { get; set; }

        [ForeignKey(nameof(BusId))]
        public Bus Bus { get; set; } = null!;
    }
}
