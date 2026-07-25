namespace BusTracking.Mobile.Interfaces
{
    public interface IFuelLogService
    {
        Task<ApiResponse<List<FuelLogItem>>> GetAllAsync(int? busId = null);
        Task<ApiResponse<bool>> CreateAsync(FuelLogItem item);
        Task<ApiResponse<bool>> UpdateAsync(FuelLogItem item);
    }
}
