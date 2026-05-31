using Carevionix.Models;

namespace Carevionix.Interfaces;

public interface INotificationService
{
    Task<Notification> NotifyPatientAsync(int patientId, NotificationType type, string title, string message);
    Task<IReadOnlyList<Notification>> GetForPatientAsync(int patientId);
    Task<Notification> MarkAsReadAsync(int patientId, int notificationId);
}
