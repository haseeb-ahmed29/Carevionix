namespace Carevionix.Models;

public class Consultation
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ConsultationType ConsultationType { get; set; }
    public string? SessionReference { get; set; }
    public string? Notes { get; set; }
}
