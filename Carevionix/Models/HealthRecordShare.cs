namespace Carevionix.Models;

public class HealthRecordShare
{
    public int Id { get; set; }
    public int HealthRecordId { get; set; }
    public HealthRecord HealthRecord { get; set; } = null!;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public string SharedByUserId { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool ConsentActive { get; set; } = true;
}
