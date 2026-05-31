using Carevionix.Data;
using Carevionix.DTOs;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class AppointmentService : IAppointmentService
{
    private const int AppointmentSlotMinutes = 30;
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notifications;
    private readonly IEmailNotificationService _emailNotifications;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        ApplicationDbContext context,
        INotificationService notifications,
        IEmailNotificationService emailNotifications,
        ILogger<AppointmentService> logger)
    {
        _context = context;
        _notifications = notifications;
        _emailNotifications = emailNotifications;
        _logger = logger;
    }

    public async Task<Appointment> BookAsync(int patientId, BookAppointmentDto dto)
    {
        if (dto.ScheduledAt <= DateTime.Now)
        {
            throw new InvalidOperationException("Appointment must be scheduled in the future.");
        }

        var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId);
        if (patient is null)
        {
            throw new UnauthorizedAccessException("Patient profile not found.");
        }

        var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == dto.DoctorId)
            ?? throw new KeyNotFoundException("Doctor not found.");

        if (!await IsDoctorAvailableInternalAsync(dto.DoctorId, dto.ScheduledAt, excludeAppointmentId: null))
        {
            throw new InvalidOperationException("Doctor is not available at the selected time.");
        }

        var appointment = new Appointment
        {
            PatientId = patientId,
            PatientName = patient.PatientName.Length > 0 ? patient.PatientName : patient.User.FullName,
            DoctorId = dto.DoctorId,
            DoctorName = doctor.DoctorName.Length > 0 ? doctor.DoctorName : doctor.User.FullName,
            ScheduledAt = dto.ScheduledAt,
            ConsultationType = dto.ConsultationType,
            Reason = dto.Reason,
            InsuranceInfo = dto.InsuranceInfo,
            Status = AppointmentStatus.Pending
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        await _notifications.NotifyPatientAsync(patientId, NotificationType.AppointmentBooked, "Appointment booked", "Your appointment request has been created.");
        if (!string.IsNullOrWhiteSpace(patient.User.Email))
        {
            try
            {
                await _emailNotifications.SendAppointmentSubmittedAsync(patient.User.Email, appointment);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Appointment {AppointmentId} was booked, but the confirmation email could not be sent.", appointment.Id);
            }
        }

        return appointment;
    }

    public async Task<Appointment> RescheduleAsync(int patientId, RescheduleAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == patientId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
        {
            throw new InvalidOperationException("Completed, cancelled, or rejected appointments cannot be rescheduled.");
        }

        if (dto.ScheduledAt <= DateTime.Now)
        {
            throw new InvalidOperationException("Appointment must be rescheduled to a future time.");
        }

        if (!await IsDoctorAvailableInternalAsync(appointment.DoctorId, dto.ScheduledAt, appointment.Id))
        {
            throw new InvalidOperationException("Doctor is not available at the selected time.");
        }

        appointment.ScheduledAt = dto.ScheduledAt;
        appointment.Status = AppointmentStatus.Rescheduled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task CancelAsync(int patientId, CancelAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == patientId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Completed or already cancelled appointments cannot be cancelled.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = dto.Reason;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime scheduledAt)
    {
        return await IsDoctorAvailableInternalAsync(doctorId, scheduledAt, excludeAppointmentId: null);
    }

    private async Task<bool> IsDoctorAvailableInternalAsync(int doctorId, DateTime scheduledAt, int? excludeAppointmentId)
    {
        var doctorActive = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive);
        if (!doctorActive)
        {
            return false;
        }

        var time = TimeOnly.FromDateTime(scheduledAt);
        var day = scheduledAt.DayOfWeek;
        var hasAnyAvailability = await _context.DoctorAvailabilities.AnyAsync(a => a.DoctorId == doctorId);
        var hasAvailability = !hasAnyAvailability || await _context.DoctorAvailabilities.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.DayOfWeek == day &&
            a.IsAvailable &&
            a.StartTime <= time &&
            a.EndTime > time);

        var slotEnd = scheduledAt.AddMinutes(AppointmentSlotMinutes);
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == doctorId &&
            (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value) &&
            a.ScheduledAt < slotEnd &&
            scheduledAt < a.ScheduledAt.AddMinutes(AppointmentSlotMinutes) &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected);

        return hasAvailability && !hasConflict;
    }

    public async Task<IReadOnlyList<Appointment>> GetPatientAppointmentsAsync(int patientId) =>
        await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();

    public async Task<IReadOnlyList<Appointment>> GetDoctorAppointmentsAsync(int doctorId) =>
        await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Invoice)
            .Where(a => a.DoctorId == doctorId && a.Status != AppointmentStatus.Pending && a.Status != AppointmentStatus.Rescheduled)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();

    public async Task<Appointment> UpdateStatusAsync(int doctorId, int appointmentId, AppointmentStatus status)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
        {
            throw new InvalidOperationException("Completed, cancelled, or rejected appointments cannot change status.");
        }

        if (appointment.Status == AppointmentStatus.Pending)
        {
            throw new InvalidOperationException("Admin approval is required before the doctor can manage this appointment.");
        }

        if (appointment.Status == AppointmentStatus.Accepted && status is not (AppointmentStatus.Rejected or AppointmentStatus.Completed))
        {
            throw new InvalidOperationException("Approved appointments can only be rejected or completed by the doctor.");
        }

        if (appointment.Status == AppointmentStatus.Rescheduled && status is not (AppointmentStatus.Rejected or AppointmentStatus.Completed))
        {
            throw new InvalidOperationException("Rescheduled appointments can only be rejected or completed by the doctor after admin review.");
        }

        if (status is not (AppointmentStatus.Rejected or AppointmentStatus.Completed))
        {
            throw new InvalidOperationException("Unsupported appointment status transition.");
        }

        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task GenerateReminderAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        await _notifications.NotifyPatientAsync(appointment.PatientId, NotificationType.AppointmentReminder, "Appointment reminder", $"Your appointment is scheduled for {appointment.ScheduledAt:g}.");
    }
}
