using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record CreateDoctorReviewDto(
    [Range(1, int.MaxValue)] int AppointmentId,
    [Range(1, 5)] int Rating,
    string? ReviewText);
