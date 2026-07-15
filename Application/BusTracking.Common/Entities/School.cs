namespace BusTracking.Common.Entities
{
    public class School
    {
        [Key]
        public int SchoolId { get; set; }

        [Required, MaxLength(200)]
        public string SchoolName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string SchoolCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SchoolLogo { get; set; }

        [Required, MaxLength(500)]
        public string SchoolAddress { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string EmailAddress { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string PrincipalName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Website { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = [];
    }
}
