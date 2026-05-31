using Carevionix.Data;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> NotifyPatientAsync(int patientId, NotificationType type, string title, string message)
    {
        var notification = new Notification
        {
            PatientId = patientId,
            Type = type,
            Title = title,
            Message = message
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<IReadOnlyList<Notification>> GetForPatientAsync(int patientId) =>
        await _context.Notifications
            .Where(n => n.PatientId == patientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<Notification> MarkAsReadAsync(int patientId, int notificationId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.PatientId == patientId)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return notification;
    }
}
