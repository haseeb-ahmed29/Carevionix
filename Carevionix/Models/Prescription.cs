namespace Carevionix.Models;

public class Prescription
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionMedicine> Medicines { get; set; } = new List<PrescriptionMedicine>();
}
