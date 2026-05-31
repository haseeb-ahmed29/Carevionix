using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record PrescriptionMedicineDto(
    [Required] string MedicineName,
    [Required] string Dose,
    [Required] string Duration,
    string? Instructions);

public record CreatePrescriptionDto(
    [Range(1, int.MaxValue)] int AppointmentId,
    [Required] string Diagnosis,
    [Required] IEnumerable<PrescriptionMedicineDto> Medicines,
    string? Notes,
    DateTime? FollowUpDate);
