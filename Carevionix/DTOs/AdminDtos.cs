using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record AdminDoctorDto(
    int? Id,
    [Required] string FullName,
    [Required, EmailAddress] string Email,
    string? PhoneNumber,
    [Required] string Specialty,
    string? Location,
    string? Languages,
    string? Qualifications,
    string? Bio,
    [Range(0, 70)] int ExperienceYears,
    [Range(0, 1000000)] decimal ConsultationFee,
    bool IsActive,
    string? Password);

public record AdminPatientDto(
    int? Id,
    [Required] string FullName,
    [Required, EmailAddress] string Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? InsuranceInfo,
    string? MedicalHistory,
    string? EmergencyContact,
    bool IsActive,
    string? Password);

