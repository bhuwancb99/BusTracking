namespace BusTracking.Common.Services
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _db;

        public LogService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(
            string platform,
            string? exceptionMessage,
            string? stackTrace,
            string? requestUrl,
            int? userId,
            string? username,
            string? role,
            string? moduleName,
            string? actionName,
            string? additionalDetails)
        {
            var log = new Logger
            {
                Platform = platform,
                Timestamp = DateTime.UtcNow,
                ExceptionMessage = exceptionMessage,
                StackTrace = stackTrace,
                RequestUrl = requestUrl,
                UserId = userId,
                Username = username,
                Role = role,
                ModuleName = moduleName,
                ActionName = actionName,
                AdditionalDetails = additionalDetails
            };

            _db.Loggers.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
