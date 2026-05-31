using Carevionix.Models;

namespace Carevionix.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> GenerateAsync(int appointmentId, decimal insuranceAmount = 0, decimal discount = 0);
    Task<IReadOnlyList<Invoice>> GetPatientInvoicesAsync(int patientId);
    Task<Invoice> SubmitDemoPaymentAsync(int patientId, int invoiceId, string payerName, string demoMethod);
    Task<Invoice> ApproveDemoPaymentAsync(int invoiceId);
}
