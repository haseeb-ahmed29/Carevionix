using Carevionix.Data;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notifications;

    public InvoiceService(ApplicationDbContext context, INotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<Invoice> GenerateAsync(int appointmentId, decimal insuranceAmount = 0, decimal discount = 0)
    {
        if (insuranceAmount < 0 || discount < 0)
        {
            throw new InvalidOperationException("Insurance amount and discount cannot be negative.");
        }

        var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);
        if (existingInvoice is not null)
        {
            return existingInvoice;
        }

        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Invoices can only be generated after the appointment is completed.");
        }

        var fee = appointment.Doctor.ConsultationFee;
        var payableAdjustments = Math.Min(fee, insuranceAmount + discount);
        var invoice = new Invoice
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            ConsultationFee = fee,
            InsuranceAmount = insuranceAmount,
            Discount = discount,
            Total = fee - payableAdjustments
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        await _notifications.NotifyPatientAsync(appointment.PatientId, NotificationType.InvoiceGenerated, "Invoice generated", "Your consultation invoice is ready.");
        return invoice;
    }

    public async Task<IReadOnlyList<Invoice>> GetPatientInvoicesAsync(int patientId) =>
        await _context.Invoices
            .Include(i => i.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.GeneratedAt)
            .ToListAsync();

    public async Task<Invoice> SubmitDemoPaymentAsync(int patientId, int invoiceId, string payerName, string demoMethod)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Patient).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.PatientId == patientId)
            ?? throw new KeyNotFoundException("Invoice not found.");

        if (invoice.Total <= 0)
        {
            invoice.PaymentStatus = PaymentStatus.Approved;
            invoice.PaymentApprovedAt ??= DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return invoice;
        }

        if (invoice.PaymentStatus == PaymentStatus.Approved)
        {
            return invoice;
        }

        invoice.DemoPayerName = string.IsNullOrWhiteSpace(payerName)
            ? (invoice.Patient.PatientName.Length > 0 ? invoice.Patient.PatientName : invoice.Patient.User.FullName)
            : payerName.Trim();
        invoice.DemoPaymentMethod = demoMethod.Trim();
        invoice.DemoPaymentReceipt = $"DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}-{invoice.Id}";
        invoice.DemoPaymentSubmittedAt = DateTime.UtcNow;
        invoice.PaymentStatus = PaymentStatus.Submitted;

        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> ApproveDemoPaymentAsync(int invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");

        if (invoice.PaymentStatus != PaymentStatus.Submitted && invoice.Total > 0)
        {
            throw new InvalidOperationException("Only submitted demo payments can be approved.");
        }

        invoice.PaymentStatus = PaymentStatus.Approved;
        invoice.PaymentApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return invoice;
    }
}
