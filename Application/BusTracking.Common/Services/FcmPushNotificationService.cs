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
                    .IgnoreQueryFilters()
                    .Where(c => c.ConfigKey == "IsAllowPushNotification" && c.IsActive)
                    .Select(c => c.ConfigValue)
                    .FirstOrDefaultAsync();

                if (isAllow == "0") return;

                var student = await db.Students
                    .IgnoreQueryFilters()
                    .Include(s => s.Bus)
                    .Include(s => s.Stop)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student is null) return;

                var studentUser = await db.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UserId == student.UserId);

                var parentIdsFromLink = await db.ParentStudents
                    .IgnoreQueryFilters()
                    .Where(ps => ps.StudentId == studentId)
                    .Select(ps => ps.ParentId)
                    .ToListAsync();

                var parentUserIdsFromParentsTable = await db.Parents
                    .IgnoreQueryFilters()
                    .Where(p => parentIdsFromLink.Contains(p.ParentId))
                    .Select(p => p.UserId)
                    .ToListAsync();

                var targetUserIds = new List<int> { student.UserId };
                targetUserIds.AddRange(parentUserIdsFromParentsTable);
                targetUserIds.AddRange(parentIdsFromLink);
                targetUserIds = targetUserIds.Distinct().ToList();

                var stopObj = await db.Stops.IgnoreQueryFilters().FirstOrDefaultAsync(st => st.StopId == stopId) ?? student.Stop;
                var stopName = stopObj?.StopName ?? "assigned stop";
                var busName = student.Bus?.BusName ?? "School Bus";
                var studentName = studentUser?.FullName ?? "Student";

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
                    .Where(d => targetUserIds.Contains(d.UserId))
                    .Select(d => d.Token)
                    .Distinct()
                    .ToListAsync();

                if (tokens.Count == 0) return;

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

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
                    .IgnoreQueryFilters()
                    .Where(c => c.ConfigKey == "IsAllowPushNotification" && c.IsActive)
                    .Select(c => c.ConfigValue)
                    .FirstOrDefaultAsync();

                if (isAllow == "0") return;

                var trip = await db.BusTrips
                    .IgnoreQueryFilters()
                    .Include(t => t.Route)
                    .Include(t => t.Driver)
                    .FirstOrDefaultAsync(t => t.TripId == tripId);

                if (trip is null) return;

                int? routeId = trip.RouteId;

                // 1. Get student IDs from StudentTripStatuses for this trip
                var tripStudentIds = await db.StudentTripStatuses
                    .IgnoreQueryFilters()
                    .Where(sts => sts.TripId == tripId)
                    .Select(sts => sts.StudentId)
                    .ToListAsync();

                // 2. Get student IDs directly assigned to this bus
                var busStudentIds = trip.BusId != null
                    ? await db.Students
                        .IgnoreQueryFilters()
                        .Where(s => s.BusId == trip.BusId)
                        .Select(s => s.StudentId)
                        .ToListAsync()
                    : new List<int>();

                // 3. Get student IDs assigned to stops on this route
                var routeStopStudentIds = routeId != null
                    ? await db.Students
                        .IgnoreQueryFilters()
                        .Where(s => s.StopId != null && db.Stops.Any(st => st.StopId == s.StopId && st.RouteId == routeId))
                        .Select(s => s.StudentId)
                        .ToListAsync()
                    : new List<int>();

                var allStudentIds = tripStudentIds
                    .Concat(busStudentIds)
                    .Concat(routeStopStudentIds)
                    .Distinct()
                    .ToList();

                var studentUserIds = await db.Students
                    .IgnoreQueryFilters()
                    .Where(s => allStudentIds.Contains(s.StudentId))
                    .Select(s => s.UserId)
                    .ToListAsync();

                var parentIdsFromLink = await db.ParentStudents
                    .IgnoreQueryFilters()
                    .Where(ps => allStudentIds.Contains(ps.StudentId))
                    .Select(ps => ps.ParentId)
                    .ToListAsync();

                var parentUserIdsFromParentsTable = await db.Parents
                    .IgnoreQueryFilters()
                    .Where(p => parentIdsFromLink.Contains(p.ParentId))
                    .Select(p => p.UserId)
                    .ToListAsync();

                var targetUserIds = studentUserIds
                    .Concat(parentUserIdsFromParentsTable)
                    .Concat(parentIdsFromLink)
                    .Distinct()
                    .ToList();

                if (targetUserIds.Count == 0) return;

                var driverName = trip.Driver?.FullName ?? "Bus Driver";
                var busName = trip.Bus?.BusName ?? "School Bus";
                var routeName = trip.Route?.RouteName ?? "Bus Route";

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
                    .Where(d => targetUserIds.Contains(d.UserId))
                    .Select(d => d.Token)
                    .Distinct()
                    .ToListAsync();

                if (tokens.Count == 0) return;

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

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