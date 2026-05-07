using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class Stop
    {
        [Key] public int StopId { get; set; }
        public int RouteId { get; set; }
        [Required, MaxLength(150)] public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        [Column(TypeName = "decimal(10,7)")] public decimal? Latitude { get; set; }
        [Column(TypeName = "decimal(10,7)")] public decimal? Longitude { get; set; }
        public TimeOnly? MorningTime { get; set; }
        public TimeOnly? EveningTime { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RouteId))]
        public BusRoute Route { get; set; } = null!;

        public ICollection<StudentDetail> Students { get; set; } = [];
    }
}
