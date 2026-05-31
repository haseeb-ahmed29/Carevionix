using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record GenerateInvoiceDto(
    [Range(1, int.MaxValue)] int AppointmentId,
    [Range(0, 1000000)] decimal InsuranceAmount,
    [Range(0, 1000000)] decimal Discount);

public record DemoInvoicePaymentDto(
    [Range(1, int.MaxValue)] int InvoiceId,
    [Required, StringLength(80)] string PayerName,
    [Required, StringLength(40)] string DemoMethod);

public record ApproveInvoicePaymentDto([Range(1, int.MaxValue)] int InvoiceId);
