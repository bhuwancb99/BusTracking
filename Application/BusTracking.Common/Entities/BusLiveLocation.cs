using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class BusLiveLocation
    {
        [Key] public long LocationId { get; set; }
        public int TripId { get; set; }
        public int BusId { get; set; }
        [Column(TypeName = "decimal(10,7)")] public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(10,7)")] public decimal Longitude { get; set; }
        [Column(TypeName = "decimal(6,2)")] public decimal? Speed { get; set; }
        [Column(TypeName = "decimal(6,2)")] public decimal? Heading { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TripId))] public BusTrip Trip { get; set; } = null!;
        [ForeignKey(nameof(BusId))] public Bus Bus { get; set; } = null!;
    }
}
