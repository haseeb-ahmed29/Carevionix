using Carevionix.Data;
using Carevionix.DTOs;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notifications;
    private readonly IInvoiceService _invoices;

    public PrescriptionService(ApplicationDbContext context, INotificationService notifications, IInvoiceService invoices)
    {
        _context = context;
        _notifications = notifications;
        _invoices = invoices;
    }

    public async Task<Prescription> CreateAsync(int doctorId, CreatePrescriptionDto dto)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.DoctorId == doctorId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
        {
            throw new InvalidOperationException("Cannot prescribe for a cancelled or rejected appointment.");
        }

        var medicines = dto.Medicines?.ToList() ?? [];
        if (medicines.Count == 0)
        {
            throw new InvalidOperationException("At least one medicine is required.");
        }

        var existingPrescription = await _context.Prescriptions.AnyAsync(p => p.AppointmentId == appointment.Id);
        if (existingPrescription)
        {
            throw new InvalidOperationException("A prescription already exists for this appointment.");
        }

        var prescription = new Prescription
        {
            AppointmentId = appointment.Id,
            Diagnosis = dto.Diagnosis,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate,
            Medicines = medicines.Select(m => new PrescriptionMedicine
            {
                MedicineName = m.MedicineName,
                Dose = m.Dose,
                Duration = m.Duration,
                Instructions = m.Instructions
            }).ToList()
        };

        _context.Prescriptions.Add(prescription);
        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();
        await _notifications.NotifyPatientAsync(appointment.PatientId, NotificationType.PrescriptionAdded, "Prescription added", "Your doctor has added a prescription.");
        await _invoices.GenerateAsync(appointment.Id);

        if (dto.FollowUpDate.HasValue)
        {
            await _notifications.NotifyPatientAsync(appointment.PatientId, NotificationType.FollowUpReminder, "Follow-up scheduled", $"Follow up on {dto.FollowUpDate.Value:d}.");
        }

        return prescription;
    }

    public async Task<IReadOnlyList<Prescription>> GetPatientPrescriptionsAsync(int patientId) =>
        await _context.Prescriptions
            .Include(p => p.Medicines)
            .Include(p => p.Appointment)
            .Where(p => p.Appointment.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
}
