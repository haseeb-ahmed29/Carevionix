using System.ComponentModel.DataAnnotations;
using Carevionix.Models;

namespace Carevionix.DTOs;

public record ShareHealthRecordDto([Range(1, int.MaxValue)] int RecordId, [Required] string DoctorUserId, DateTime? ExpiresAt);

public record UploadHealthRecordDto([Required] HealthRecordType Type, [Required] string Title, string? Description);
