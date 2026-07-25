namespace BusTracking.Mobile.Services
{
    public class FuelLogService : IFuelLogService
    {
        private readonly IApiService _api;

        public FuelLogService(IApiService api)
        {
            _api = api;
        }

        public async Task<ApiResponse<List<FuelLogItem>>> GetAllAsync(int? busId = null)
        {
            string url = busId.HasValue && busId.Value > 0
                ? $"{Constants.FuelLog.All}?busId={busId.Value}"
                : Constants.FuelLog.All;
            return await _api.GetAsync<List<FuelLogItem>>(url);
        }

        public async Task<ApiResponse<bool>> CreateAsync(FuelLogItem item)
        {
            return await _api.PostAsync<bool>(Constants.FuelLog.All, new
            {
                item.BusId,
                item.OdometerReading,
                item.FuelLiters,
                item.TotalCost,
                FuelDate = string.IsNullOrWhiteSpace(item.FuelDate) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : item.FuelDate,
                item.Notes
            });
        }

        public async Task<ApiResponse<bool>> UpdateAsync(FuelLogItem item)
        {
            return await _api.PutAsync<bool>(string.Format(Constants.FuelLog.ById, item.FuelLogId), new
            {
                item.BusId,
                item.OdometerReading,
                item.FuelLiters,
                item.TotalCost,
                FuelDate = item.FuelDate,
                item.Notes
            });
        }
    }
}
