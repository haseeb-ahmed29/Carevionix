using Carevionix.Data;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class HealthRecordService : IHealthRecordService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".docx"
    };

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly INotificationService _notifications;

    public HealthRecordService(ApplicationDbContext context, IWebHostEnvironment environment, INotificationService notifications)
    {
        _context = context;
        _environment = environment;
        _notifications = notifications;
    }

    public async Task<HealthRecord> UploadAsync(int patientId, string uploadedByUserId, HealthRecordType type, string title, IFormFile file, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Record title is required.");
        }

        if (file is null)
        {
            throw new InvalidOperationException("A file is required.");
        }

        if (file.Length <= 0 || file.Length > 10 * 1024 * 1024)
        {
            throw new InvalidOperationException("File must be between 1 byte and 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedContentTypes.Contains(file.ContentType) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
        if (!patientExists)
        {
            throw new UnauthorizedAccessException("Patient profile not found.");
        }

        var uploadRoot = Path.Combine(_environment.ContentRootPath, "SecureUploads", patientId.ToString());
        Directory.CreateDirectory(uploadRoot);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadRoot, storedFileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var record = new HealthRecord
        {
            PatientId = patientId,
            UploadedByUserId = uploadedByUserId,
            RecordType = type,
            Title = title,
            Description = description,
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length
        };

        _context.HealthRecords.Add(record);
        await _context.SaveChangesAsync();
        await _notifications.NotifyPatientAsync(patientId, NotificationType.RecordUploaded, "Health record uploaded", $"{title} was added to your records.");
        return record;
    }

    public async Task<FileStreamResult> DownloadAsync(int patientId, int recordId)
    {
        var record = await _context.HealthRecords.FirstOrDefaultAsync(r => r.Id == recordId && r.PatientId == patientId)
            ?? throw new KeyNotFoundException("Health record not found.");

        var path = Path.Combine(_environment.ContentRootPath, "SecureUploads", patientId.ToString(), record.StoredFileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Stored file not found.");
        }

        return new FileStreamResult(File.OpenRead(path), record.ContentType)
        {
            FileDownloadName = record.FileName
        };
    }

    public async Task<IReadOnlyList<HealthRecord>> GetHistoryAsync(int patientId) =>
        await _context.HealthRecords
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();

    public async Task<HealthRecordShare> ShareAsync(int patientId, int recordId, string doctorUserId, string sharedByUserId, DateTime? expiresAt)
    {
        var record = await _context.HealthRecords.Include(r => r.Shares).FirstOrDefaultAsync(r => r.Id == recordId && r.PatientId == patientId)
            ?? throw new KeyNotFoundException("Health record not found.");

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId && d.IsActive)
            ?? throw new KeyNotFoundException("Doctor not found.");

        var share = await _context.HealthRecordShares.FirstOrDefaultAsync(s => s.HealthRecordId == recordId && s.DoctorId == doctor.Id);
        if (share is null)
        {
            share = new HealthRecordShare
            {
                HealthRecordId = record.Id,
                PatientId = patientId,
                DoctorId = doctor.Id,
                SharedByUserId = sharedByUserId
            };
            _context.HealthRecordShares.Add(share);
        }

        share.ConsentActive = true;
        share.ExpiresAt = expiresAt;
        share.SharedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _notifications.NotifyPatientAsync(patientId, NotificationType.RecordUploaded, "Record shared", $"{record.Title} was shared with your selected doctor.");
        return share;
    }
}
