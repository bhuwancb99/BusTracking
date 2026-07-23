namespace BusTracking.Common.Services
{
    public class FcmPushNotificationService : IFcmPushNotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FcmPushNotificationService> _logger;

        public FcmPushNotificationService(
            IServiceScopeFactory scopeFactory,
            ILogger<FcmPushNotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task SendStudentPickedUpPushAsync(int tripId, int studentId, int stopId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var isAllow = await db.AppConfigurations
                    .Where(c => c.ConfigKey == "IsAllowPushNotification" && c.IsActive)
                    .Select(c => c.ConfigValue)
                    .FirstOrDefaultAsync();

                if (isAllow == "0") return;

                var student = await db.Students
                    .IgnoreQueryFilters()
                    .Include(s => s.User)
                    .Include(s => s.Bus)
                    .Include(s => s.Stop)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student is null) return;

                var parentUserIds = await db.ParentStudents
                    .IgnoreQueryFilters()
                    .Include(ps => ps.Parent)
                    .Where(ps => ps.StudentId == studentId)
                    .Select(ps => ps.Parent.UserId)
                    .ToListAsync();

                var targetUserIds = new List<int> { student.UserId };
                targetUserIds.AddRange(parentUserIds);
                targetUserIds = targetUserIds.Distinct().ToList();

                var stopObj = await db.Stops.IgnoreQueryFilters().FirstOrDefaultAsync(st => st.StopId == stopId) ?? student.Stop;
                var stopName = stopObj?.StopName ?? "assigned stop";
                var busName = student.Bus?.BusName ?? "School Bus";
                var studentName = student.User?.FullName ?? "Student";

                var title = "🎒 Student Picked Up!";
                var body = $"{studentName} has been picked up at '{stopName}' on bus '{busName}'.";

                // 1. Save in-app notification records in DB for all target users
                foreach (var userId in targetUserIds)
                {
                    db.Notifications.Add(new BusTracking.Common.Entities.Notification
                    {
                        SchoolId = student.SchoolId,
                        RecipientUserId = userId,
                        Title = title,
                        Body = body,
                        NotificationType = NotificationType.StudentPickedUp,
                        ReferenceId = tripId,
                        ReferenceType = "Trip",
                        IsRead = false,
                        SentAt = DateTime.UtcNow
                    });
                }
                await db.SaveChangesAsync();

                // 2. Dispatch FCM Push Notifications to active device tokens
                var tokens = await db.DeviceTokens
                    .IgnoreQueryFilters()
                    .Where(d => d.IsActive && targetUserIds.Contains(d.UserId))
                    .Select(d => d.Token)
                    .Distinct()
                    .ToListAsync();

                if (tokens.Count == 0) return;

                var msg = new MulticastMessage
                {
                    Tokens = tokens,
                    Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
                    Data = new Dictionary<string, string>
                    {
                        ["type"] = "STUDENT_PICKED_UP",
                        ["tripId"] = tripId.ToString(),
                        ["studentId"] = studentId.ToString(),
                        ["studentName"] = studentName,
                        ["stopName"] = stopName,
                        ["busName"] = busName,
                        ["title"] = title,
                        ["body"] = body
                    }
                };

                if (FirebaseMessaging.DefaultInstance != null)
                {
                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(msg);
                    _logger.LogInformation($"[FCM] Sent PickedUp push to {response.SuccessCount}/{tokens.Count} devices for Student #{studentId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[FCM] Error sending PickedUp push for Student #{studentId}: {ex.Message}");
            }
        }

        public async Task SendTripStartedPushAsync(int tripId, int driverUserId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var isAllow = await db.AppConfigurations
                    .Where(c => c.ConfigKey == "IsAllowPushNotification" && c.IsActive)
                    .Select(c => c.ConfigValue)
                    .FirstOrDefaultAsync();

                if (isAllow == "0") return;

                var trip = await db.BusTrips
                    .IgnoreQueryFilters()
                    .Include(t => t.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Stops)
                    .Include(t => t.Driver)
                    .FirstOrDefaultAsync(t => t.TripId == tripId);

                if (trip?.Bus is null) return;

                var studentUserIds = await db.Students
                    .IgnoreQueryFilters()
                    .Include(s => s.User)
                    .Where(s => s.BusId == trip.BusId && s.User.IsActive)
                    .Select(s => s.UserId)
                    .ToListAsync();

                var busStudentIds = await db.Students
                    .IgnoreQueryFilters()
                    .Where(s => s.BusId == trip.BusId)
                    .Select(s => s.StudentId)
                    .ToListAsync();

                var parentUserIds = await db.ParentStudents
                    .IgnoreQueryFilters()
                    .Include(ps => ps.Parent)
                    .Where(ps => busStudentIds.Contains(ps.StudentId))
                    .Select(ps => ps.Parent.UserId)
                    .ToListAsync();

                var targetUserIds = studentUserIds.Concat(parentUserIds).Distinct().ToList();
                if (targetUserIds.Count == 0) return;

                var driverName = trip.Driver?.FullName ?? "Bus Driver";
                var busName = trip.Bus.BusName;
                var routeName = trip.Bus.Route?.RouteName ?? "Bus Route";

                var title = "🚌 Bus Trip Started!";
                var body = $"Driver {driverName} has started the trip on route '{routeName}'. Bus: {busName}.";

                // 1. Save in-app notification records in DB for all target users
                foreach (var userId in targetUserIds)
                {
                    db.Notifications.Add(new Notification
                    {
                        SchoolId = trip.SchoolId,
                        RecipientUserId = userId,
                        Title = title,
                        Body = body,
                        NotificationType = NotificationType.Broadcast,
                        ReferenceId = tripId,
                        ReferenceType = "Trip",
                        IsRead = false,
                        SentAt = DateTime.UtcNow
                    });
                }
                await db.SaveChangesAsync();

                // 2. Dispatch FCM Push Notifications to active device tokens
                var tokens = await db.DeviceTokens
                    .IgnoreQueryFilters()
                    .Where(d => d.IsActive && targetUserIds.Contains(d.UserId))
                    .Select(d => d.Token)
                    .Distinct()
                    .ToListAsync();

                if (tokens.Count == 0) return;

                var msg = new MulticastMessage
                {
                    Tokens = tokens,
                    Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
                    Data = new Dictionary<string, string>
                    {
                        ["type"] = "TRIP_STARTED",
                        ["tripId"] = tripId.ToString(),
                        ["busName"] = busName,
                        ["routeName"] = routeName,
                        ["driverName"] = driverName,
                        ["title"] = title,
                        ["body"] = body
                    }
                };

                if (FirebaseMessaging.DefaultInstance != null)
                {
                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(msg);
                    _logger.LogInformation($"[FCM] Sent TripStarted push to {response.SuccessCount}/{tokens.Count} devices for Trip #{tripId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[FCM] Error sending TripStarted push for Trip #{tripId}: {ex.Message}");
            }
        }
    }
}