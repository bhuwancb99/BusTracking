namespace BusTracking.Mobile.Models.Standard
{
    public class CreateStandardRequest
    {
        public string StandardName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
