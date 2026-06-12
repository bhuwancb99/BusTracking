namespace BusTracking.Common.DTOs.BusType
{
    public class SaveBusTypeDto
    {
        [Required(ErrorMessage = "Bus type name is required.")]
        [MaxLength(100)]
        public string Name { get; set; } = "";
    }
}
