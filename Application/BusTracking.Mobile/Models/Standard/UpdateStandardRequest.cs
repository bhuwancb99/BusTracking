namespace BusTracking.Mobile.Models.Standard
{
    public class UpdateStandardRequest
    {
        public string StandardName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
