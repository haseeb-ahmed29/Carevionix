using Carevionix.DTOs;
using Carevionix.Models;

namespace Carevionix.Interfaces;

public interface IAppointmentService
{
    Task<Appointment> BookAsync(int patientId, BookAppointmentDto dto);
    Task<Appointment> RescheduleAsync(int patientId, RescheduleAppointmentDto dto);
    Task CancelAsync(int patientId, CancelAppointmentDto dto);
    Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime scheduledAt);
    Task<IReadOnlyList<Appointment>> GetPatientAppointmentsAsync(int patientId);
    Task<IReadOnlyList<Appointment>> GetDoctorAppointmentsAsync(int doctorId);
    Task<Appointment> UpdateStatusAsync(int doctorId, int appointmentId, AppointmentStatus status);
    Task GenerateReminderAsync(int appointmentId);
}
