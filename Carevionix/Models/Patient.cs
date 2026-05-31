namespace Carevionix.Models;

public class Patient
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string PatientName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? InsuranceInfo { get; set; }
    public string? MedicalHistory { get; set; }
    public string? EmergencyContact { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<DoctorReview> DoctorReviews { get; set; } = new List<DoctorReview>();
    public ICollection<HealthRecordShare> SharedHealthRecords { get; set; } = new List<HealthRecordShare>();
}
