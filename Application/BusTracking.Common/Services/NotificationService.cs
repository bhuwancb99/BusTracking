using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Notification;
using BusTracking.Common.Entities;
using BusTracking.Common.Enums;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        public NotificationService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId)
        {
            var items = await _db.Notifications
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.SentAt).Take(50)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Body = n.Body,
                    NotificationType = n.NotificationType.ToString(),
                    IsRead = n.IsRead,
                    SentAt = n.SentAt
                }).ToListAsync();

            return ApiResponse<List<NotificationDto>>.Ok(items);
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            var n = await _db.Notifications
                .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.RecipientUserId == userId);
            if (n is null) return ApiResponse<bool>.Fail("Notification not found.");
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId)
        {
            await _db.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTime.UtcNow));
            return ApiResponse<bool>.Ok(true);
        }

        public async Task SendAsync(int recipientUserId, string title, string body, string type, int? referenceId = null)
        {
            if (!Enum.TryParse<NotificationType>(type, out var notifType)) return;

            _db.Notifications.Add(new Notification
            {
                RecipientUserId = recipientUserId,
                Title = title,
                Body = body,
                NotificationType = notifType,
                ReferenceId = referenceId
            });
            await _db.SaveChangesAsync();
        }
    }
}
