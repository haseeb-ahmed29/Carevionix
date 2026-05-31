using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record ContactSubmissionDto(
    [Required] string Name,
    [Required, EmailAddress] string Email,
    string? PhoneNumber,
    [Required] string Subject,
    [Required, MinLength(10)] string Message);

