using Carevionix.Models;
using Microsoft.AspNetCore.Mvc;

namespace Carevionix.Interfaces;

public interface IHealthRecordService
{
    Task<HealthRecord> UploadAsync(int patientId, string uploadedByUserId, HealthRecordType type, string title, IFormFile file, string? description);
    Task<FileStreamResult> DownloadAsync(int patientId, int recordId);
    Task<IReadOnlyList<HealthRecord>> GetHistoryAsync(int patientId);
    Task<HealthRecordShare> ShareAsync(int patientId, int recordId, string doctorUserId, string sharedByUserId, DateTime? expiresAt);
}
