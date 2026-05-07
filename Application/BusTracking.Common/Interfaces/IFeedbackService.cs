using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Feedback;

namespace BusTracking.Common.Interfaces
{
    public interface IFeedbackService
    {
        Task<ApiResponse<PagedResult<FeedbackListDto>>> GetAllAsync(int page, int pageSize, string? status);
        Task<ApiResponse<bool>> CreateAsync(CreateFeedbackDto dto, int userId);
        Task<ApiResponse<bool>> UpdateStatusAsync(int feedbackId, string status, int resolvedBy);
    }
}
