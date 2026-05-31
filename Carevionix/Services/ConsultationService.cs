using Carevionix.Data;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class ConsultationService : IConsultationService
{
    private readonly ApplicationDbContext _context;

    public ConsultationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Consultation> StartAsync(int appointmentId, ConsultationType type)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected or AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Only active appointments can start a consultation.");
        }

        if (appointment.Status != AppointmentStatus.Accepted)
        {
            throw new InvalidOperationException("Admin approval is required before the room can open.");
        }

        if (appointment.ConsultationType is not (ConsultationType.Video or ConsultationType.Audio))
        {
            throw new InvalidOperationException("This consultation room is available for video and audio appointments only.");
        }

        var consultation = await _context.Consultations
            .Include(c => c.Appointment)
            .ThenInclude(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(c => c.Appointment)
            .ThenInclude(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);
        if (consultation is not null)
        {
            return consultation;
        }

        consultation = new Consultation
        {
            AppointmentId = appointmentId,
            ConsultationType = type,
            StartTime = DateTime.UtcNow,
            SessionReference = $"room-{appointmentId}-{Guid.NewGuid():N}"
        };

        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();
        await _context.Entry(consultation).Reference(c => c.Appointment).LoadAsync();
        await _context.Entry(consultation.Appointment).Reference(a => a.Patient).LoadAsync();
        await _context.Entry(consultation.Appointment.Patient).Reference(p => p.User).LoadAsync();
        await _context.Entry(consultation.Appointment).Reference(a => a.Doctor).LoadAsync();
        await _context.Entry(consultation.Appointment.Doctor).Reference(d => d.User).LoadAsync();
        return consultation;
    }

    public async Task<Consultation> CompleteAsync(int appointmentId, string? notes)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
        {
            throw new InvalidOperationException("Cannot complete a cancelled or rejected appointment.");
        }

        if (appointment.Status != AppointmentStatus.Accepted)
        {
            throw new InvalidOperationException("Only admin-approved appointments can be completed.");
        }

        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);
        if (consultation is null)
        {
            consultation = new Consultation
            {
                AppointmentId = appointmentId,
                ConsultationType = appointment.ConsultationType,
                StartTime = DateTime.UtcNow,
                SessionReference = $"room-{appointmentId}-{Guid.NewGuid():N}"
            };
            _context.Consultations.Add(consultation);
        }

        consultation.EndTime = DateTime.UtcNow;
        consultation.Notes = notes;
        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();
        return consultation;
    }
}
