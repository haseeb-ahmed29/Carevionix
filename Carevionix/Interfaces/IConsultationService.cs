using Carevionix.Models;

namespace Carevionix.Interfaces;

public interface IConsultationService
{
    Task<Consultation> StartAsync(int appointmentId, ConsultationType type);
    Task<Consultation> CompleteAsync(int appointmentId, string? notes);
}
