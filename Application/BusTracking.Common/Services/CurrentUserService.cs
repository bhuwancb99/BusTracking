namespace BusTracking.Common.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public System.Security.Principal.IPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(idClaim, out var id))
                        return id;
                }
                return null;
            }
        }

        public int? SchoolId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var schoolClaim = user.FindFirst("school_id")?.Value ?? user.FindFirst("SchoolId")?.Value;
                    if (int.TryParse(schoolClaim, out var id))
                        return id;
                }
                return null;
            }
        }

        public string? UserRole
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    return user.FindFirst(ClaimTypes.Role)?.Value;
                }
                return null;
            }
        }

        public string? TimeZoneInfoId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var tz = user.FindFirst("time_zone")?.Value ?? user.FindFirst("TimeZoneInfoId")?.Value;
                    if (!string.IsNullOrWhiteSpace(tz))
                        return tz;
                }
                return null;
            }
        }

        public DateTime SchoolNow => TimeZoneHelper.GetNow(TimeZoneInfoId);
        public DateOnly SchoolToday => TimeZoneHelper.GetToday(TimeZoneInfoId);
    }
}
