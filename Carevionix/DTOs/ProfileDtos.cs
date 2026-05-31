using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record DoctorProfileDto(
    [Required] string FullName,
    string? PhoneNumber,
    string? Address,
    [Required] string Specialty,
    string? Location,
    string? Languages,
    string? Qualifications,
    string? Bio,
    [Range(0, 70)] int ExperienceYears,
    [Range(0, 1000000)] decimal ConsultationFee);

public record PatientProfileDto(
    [Required] string FullName,
    string? PhoneNumber,
    string? Address,
    DateOnly? DateOfBirth,
    string? Gender,
    string? InsuranceInfo,
    string? MedicalHistory,
    string? EmergencyContact);

public record AdminProfileDto(
    [Required] string FullName,
    [Required, EmailAddress] string Email,
    string? PhoneNumber,
    string? Address);

public class ChangePasswordDto
{
    public ChangePasswordDto()
    {
    }

    public ChangePasswordDto(string currentPassword, string newPassword, string confirmPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
        ConfirmPassword = confirmPassword;
    }

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
