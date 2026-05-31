namespace Carevionix.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public ConsultationType ConsultationType { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string Reason { get; set; } = string.Empty;
    public string? InsuranceInfo { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Prescription? Prescription { get; set; }
    public Invoice? Invoice { get; set; }
    public Consultation? Consultation { get; set; }
}
