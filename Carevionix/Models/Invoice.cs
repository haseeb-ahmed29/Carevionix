namespace Carevionix.Models;

public class Invoice
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public decimal ConsultationFee { get; set; }
    public decimal InsuranceAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string? DemoPayerName { get; set; }
    public string? DemoPaymentMethod { get; set; }
    public string? DemoPaymentReceipt { get; set; }
    public DateTime? DemoPaymentSubmittedAt { get; set; }
    public DateTime? PaymentApprovedAt { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
