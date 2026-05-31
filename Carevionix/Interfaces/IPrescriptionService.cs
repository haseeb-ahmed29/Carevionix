using Carevionix.DTOs;
using Carevionix.Models;

namespace Carevionix.Interfaces;

public interface IPrescriptionService
{
    Task<Prescription> CreateAsync(int doctorId, CreatePrescriptionDto dto);
    Task<IReadOnlyList<Prescription>> GetPatientPrescriptionsAsync(int patientId);
}
