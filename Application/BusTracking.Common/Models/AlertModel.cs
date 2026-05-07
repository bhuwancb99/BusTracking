using BusTracking.Common.Enums;

namespace BusTracking.Common.Models
{
    public class AlertModel
    {
        public AlertType Type { get; set; } = AlertType.Info;
        public string Message { get; set; } = "";

        public string CssClass => Type switch
        {
            AlertType.Success => "alert-success",
            AlertType.Error => "alert-danger",
            AlertType.Warning => "alert-warning",
            _ => "alert-info"
        };

        public string Icon => Type switch
        {
            AlertType.Success => "bi-check-circle-fill",
            AlertType.Error => "bi-x-circle-fill",
            AlertType.Warning => "bi-exclamation-triangle-fill",
            _ => "bi-info-circle-fill"
        };
    }
}
