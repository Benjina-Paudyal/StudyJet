using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Repositories.Interface;

namespace StudyJet.API.Repositories.Implementation
{
    public class NotificationRepo: INotificationRepo
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the notification.", ex);
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            if (notificationId <= 0)
                throw new ArgumentException("Invalid notification ID.");

            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while marking the notification as read.", ex);
            }
        }

        public async Task<Notification> SelectByIdAsync(int notificationId)
        {
            if (notificationId <= 0)
                throw new ArgumentException("Invalid notification ID.");

            try
            {
                return await _context.Notifications.FindAsync(notificationId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the notification by ID.", ex);

            }
        }

        public async Task UpdateAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            try
            {
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the notification.", ex);
            }
        }

        public async Task<List<Notification>> SelectByUserIdAsync(string userId, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.");

            var notifications = await _context.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.DateCreated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return notifications;
        }


    }
}
