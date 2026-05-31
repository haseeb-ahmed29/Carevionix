using System.ComponentModel.DataAnnotations;
using Carevionix.Models;

namespace Carevionix.DTOs;

public record DoctorSearchDto(string? Specialty, string? Location, string? Availability, string? Language, decimal? MinRating);

public record BookAppointmentDto(
    [Range(1, int.MaxValue)] int DoctorId,
    [Required] DateTime ScheduledAt,
    [Required] ConsultationType ConsultationType,
    [Required] string Reason,
    string? InsuranceInfo);

public record RescheduleAppointmentDto([Range(1, int.MaxValue)] int AppointmentId, [Required] DateTime ScheduledAt);

public record CancelAppointmentDto([Range(1, int.MaxValue)] int AppointmentId, string? Reason);

public record CompleteAppointmentDto([Range(1, int.MaxValue)] int AppointmentId, string? Notes);
