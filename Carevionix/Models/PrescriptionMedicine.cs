namespace Carevionix.Models;

public class PrescriptionMedicine
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    public string MedicineName { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}
