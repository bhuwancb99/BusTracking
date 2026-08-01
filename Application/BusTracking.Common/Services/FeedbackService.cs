namespace BusTracking.Common.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public FeedbackService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PagedResult<FeedbackListDto>>> GetAllAsync(int page, int pageSize, string? status)
        {
            var schoolId = _currentUser.SchoolId;
            var q = _db.Feedbacks.IgnoreQueryFilters().Include(f => f.User).AsQueryable();

            if (schoolId.HasValue)
            {
                q = q.Where(f => f.SchoolId == schoolId.Value || (f.User != null && f.User.SchoolId == schoolId.Value));
            }

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

        public async Task<ApiResponse<FeedbackListDto>> GetByIdAsync(int feedbackId)
        {
            var schoolId = _currentUser.SchoolId;
            var q = _db.Feedbacks.IgnoreQueryFilters().Include(f => f.User).AsQueryable();

            if (schoolId.HasValue)
            {
                q = q.Where(f => f.SchoolId == schoolId.Value || (f.User != null && f.User.SchoolId == schoolId.Value));
            }

            var f = await q.FirstOrDefaultAsync(x => x.FeedbackId == feedbackId);
            if (f is null) return ApiResponse<FeedbackListDto>.Fail("Feedback not found.");

            return ApiResponse<FeedbackListDto>.Ok(new FeedbackListDto
            {
                FeedbackId = f.FeedbackId,
                UserName = f.User?.FullName ?? "Unknown User",
                Category = f.Category.ToString(),
                Email = f.Email,
                Description = f.Description,
                Status = f.Status.ToString(),
                CreatedAt = f.CreatedAt
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateFeedbackDto dto, int userId)
        {
            var categoryRaw = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category.Trim() : "Other";

            // Map category to Inquiry vs Complaint to satisfy SQL CHECK constraint CK_FeedbackCategory
            var catEnum = categoryRaw.Equals("Inquiry", StringComparison.OrdinalIgnoreCase)
                ? FeedbackCategory.Inquiry
                : FeedbackCategory.Complaint;

            var description = !string.IsNullOrWhiteSpace(dto.Description)
                ? dto.Description.Trim()
                : (dto.Message?.Trim() ?? "");

            var prefix = $"[Category: {categoryRaw}]";
            if (!string.IsNullOrWhiteSpace(dto.Subject))
            {
                prefix += $" [Subject: {dto.Subject.Trim()}]";
            }

            description = $"{prefix} {description}".Trim();

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == userId);
            var schoolId = user?.SchoolId ?? _currentUser.SchoolId;

            _db.Feedbacks.Add(new Feedback
            {
                SchoolId = schoolId,
                UserId = userId,
                Category = catEnum,
                Email = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : user?.Email ?? "",
                PhoneNumber = !string.IsNullOrWhiteSpace(dto.PhoneNumber) ? dto.PhoneNumber : user?.PhoneNumber,
                Description = description
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Feedback submitted.");
        }

        public async Task<ApiResponse<bool>> UpdateStatusAsync(int feedbackId, string status, int resolvedBy)
        {
            var schoolId = _currentUser.SchoolId;
            var q = _db.Feedbacks.IgnoreQueryFilters().AsQueryable();

            if (schoolId.HasValue)
            {
                q = q.Where(f => f.SchoolId == schoolId.Value || (f.User != null && f.User.SchoolId == schoolId.Value));
            }

            var f = await q.FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
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
