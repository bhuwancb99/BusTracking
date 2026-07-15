namespace BusTracking.Common.DTOs.Standard
{
    public class CreateStandardDto
    {
        [Required(ErrorMessage = "Standard Name is required.")]
        [MaxLength(100, ErrorMessage = "Standard Name cannot exceed 100 characters.")]
        public string StandardName { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}
