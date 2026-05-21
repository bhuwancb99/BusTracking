namespace BusTracking.Common.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _db;
        public FeedbackService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<FeedbackListDto>>> GetAllAsync(int page, int pageSize, string? status)
        {
            var q = _db.Feedbacks.Include(f => f.User).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FeedbackStatus>(status, out var fs))
                q = q.Where(f => f.Status == fs);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(f => new FeedbackListDto
                {
                    FeedbackId = f.FeedbackId,
                    UserName = f.User.FullName,
                    Category = f.Category.ToString(),
                    Email = f.Email,
                    Description = f.Description,
                    Status = f.Status.ToString(),
                    CreatedAt = f.CreatedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<FeedbackListDto>>.Ok(new PagedResult<FeedbackListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateFeedbackDto dto, int userId)
        {
            if (!Enum.TryParse<FeedbackCategory>(dto.Category, true, out var cat))
                return ApiResponse<bool>.Fail("Invalid category.");

            _db.Feedbacks.Add(new Feedback
            {
                UserId = userId,
                Category = cat,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Description = dto.Description
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Feedback submitted.");
        }

        public async Task<ApiResponse<bool>> UpdateStatusAsync(int feedbackId, string status, int resolvedBy)
        {
            var f = await _db.Feedbacks.FindAsync(feedbackId);
            if (f is null) return ApiResponse<bool>.Fail("Feedback not found.");
            if (!Enum.TryParse<FeedbackStatus>(status, true, out var fs))
                return ApiResponse<bool>.Fail("Invalid status.");

            f.Status = fs;
            f.UpdatedAt = DateTime.UtcNow;
            if (fs == FeedbackStatus.Resolved) { f.ResolvedBy = resolvedBy; f.ResolvedAt = DateTime.UtcNow; }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Status updated.");
        }
    }
}
