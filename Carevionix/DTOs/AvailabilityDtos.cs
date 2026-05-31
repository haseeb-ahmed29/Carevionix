using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record UpsertAvailabilityDto(
    [Required] DayOfWeek DayOfWeek,
    IReadOnlyList<DayOfWeek>? Days,
    [Required] TimeOnly StartTime,
    [Required] TimeOnly EndTime,
    bool IsAvailable);
