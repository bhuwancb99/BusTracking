namespace BusTracking.Common.DTOs.Teacher
{
    public class CreateTeacherDto
    {
        public int? SchoolId { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(150)]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(100)]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = "";

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? EmployeeCode { get; set; } // Optional field

        [MaxLength(150)]
        public string? Qualification { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? EmergencyContact { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
