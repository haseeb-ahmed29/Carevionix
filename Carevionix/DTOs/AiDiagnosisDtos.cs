using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record AiDiagnosisRequestDto(
    [Required] string Symptoms,
    int DurationDays,
    string? ExistingConditions,
    string? CurrentMedications);

public record AiDiagnosisResultDto(
    string Summary,
    string PossibleCondition,
    string Urgency,
    IReadOnlyList<string> Recommendations,
    string Disclaimer);
