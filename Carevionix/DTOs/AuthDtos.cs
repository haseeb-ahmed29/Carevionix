using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record RegisterPatientDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string FullName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? InsuranceInfo,
    string? MedicalHistory);

public record RegisterDoctorDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string FullName,
    string? PhoneNumber,
    [Required] string Specialty,
    string? Location,
    string? Languages,
    [Range(0, 70)] int ExperienceYears,
    [Range(0, 1000000)] decimal ConsultationFee);

public record LoginDto([Required, EmailAddress] string Email, [Required] string Password, bool RememberMe);

public record ForgotPasswordDto([Required, EmailAddress] string Email);

public record ResetPasswordDto([Required, EmailAddress] string Email, [Required] string Token, [Required, MinLength(6)] string NewPassword);

public record AuthResponseDto(bool Succeeded, string? Token, string? RedirectUrl, IEnumerable<string> Errors);
