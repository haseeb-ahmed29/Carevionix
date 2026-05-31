namespace Carevionix.Models;

public class Doctor
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Languages { get; set; }
    public string? Qualifications { get; set; }
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
    public ICollection<HealthRecordShare> SharedRecords { get; set; } = new List<HealthRecordShare>();
}
