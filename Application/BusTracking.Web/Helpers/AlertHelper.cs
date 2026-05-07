using BusTracking.Common.Enums;
using BusTracking.Common.Models;

namespace BusTracking.Web.Helpers
{
    public static class AlertHelper
    {
        public const string SuccessKey = "SuccessMessage";
        public const string ErrorKey = "ErrorMessage";
        public const string WarningKey = "WarningMessage";
        public const string InfoKey = "InfoMessage";

        public static AlertModel? FromTempData(IDictionary<string, object?> tempData)
        {
            if (tempData.TryGetValue(SuccessKey, out var s) && s is string sm && !string.IsNullOrEmpty(sm))
                return new AlertModel { Type = AlertType.Success, Message = sm };

            if (tempData.TryGetValue(ErrorKey, out var e) && e is string em && !string.IsNullOrEmpty(em))
                return new AlertModel { Type = AlertType.Error, Message = em };

            if (tempData.TryGetValue(WarningKey, out var w) && w is string wm && !string.IsNullOrEmpty(wm))
                return new AlertModel { Type = AlertType.Warning, Message = wm };

            if (tempData.TryGetValue(InfoKey, out var i) && i is string im && !string.IsNullOrEmpty(im))
                return new AlertModel { Type = AlertType.Info, Message = im };

            return null;
        }
    }
}
