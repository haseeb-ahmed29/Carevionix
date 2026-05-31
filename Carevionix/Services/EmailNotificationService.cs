using System.Net;
using System.Net.Mail;
using Carevionix.Models;

namespace Carevionix.Services;

public interface IEmailNotificationService
{
    Task SendContactNotificationAsync(string fromName, string fromEmail, string subject, string message, string? phoneNumber);
    Task SendPasswordResetAsync(string toEmail, string fullName, string resetUrl);
    Task SendGeneratedPasswordAsync(string toEmail, string fullName, string newPassword);
    Task SendAppointmentSubmittedAsync(string toEmail, Appointment appointment);
}

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactNotificationAsync(string fromName, string fromEmail, string subject, string message, string? phoneNumber)
    {
        var userName = GetSenderEmail();
        var adminEmail = _configuration["Smtp:ContactRecipient"] ?? userName;
        using var client = CreateClient();

        using var mail = new MailMessage
        {
            From = new MailAddress(userName, GetAppName()),
            Subject = $"{GetAppName()} contact: {subject}",
            Body = $"New contact message received on {GetAppName()}.\n\nName: {fromName}\nEmail: {fromEmail}\nPhone: {phoneNumber ?? "N/A"}\nSubject: {subject}\n\nMessage:\n{message}"
        };
        mail.To.Add(adminEmail);
        mail.ReplyToList.Add(new MailAddress(fromEmail, fromName));

        await client.SendMailAsync(mail);
    }

    public async Task SendPasswordResetAsync(string toEmail, string fullName, string resetUrl)
    {
        var userName = GetSenderEmail();
        using var client = CreateClient();

        using var mail = new MailMessage
        {
            From = new MailAddress(userName, GetAppName()),
            Subject = $"Reset your {GetAppName()} password",
            Body = $"Hello {fullName},\n\nUse this secure link to reset your password:\n{resetUrl}\n\nIf you did not request this, you can ignore this email."
        };
        mail.To.Add(toEmail);

        await client.SendMailAsync(mail);
    }

    public async Task SendGeneratedPasswordAsync(string toEmail, string fullName, string newPassword)
    {
        var userName = GetSenderEmail();
        using var client = CreateClient();

        using var mail = new MailMessage
        {
            From = new MailAddress(userName, GetAppName()),
            Subject = $"Your new {GetAppName()} password",
            Body = $"Hello {fullName},\n\nYour {GetAppName()} password has been reset successfully.\n\nNew password: {newPassword}\n\nPlease login with this password and change it after signing in."
        };
        mail.To.Add(toEmail);

        await client.SendMailAsync(mail);
    }

    public async Task SendAppointmentSubmittedAsync(string toEmail, Appointment appointment)
    {
        var userName = GetSenderEmail();
        using var client = CreateClient();

        using var mail = new MailMessage
        {
            From = new MailAddress(userName, GetAppName()),
            Subject = "Your appointment has been submitted",
            Body = $"Hello {appointment.PatientName},\n\nYour appointment has been submitted.\n\nDoctor: {appointment.DoctorName}\nDate and time: {appointment.ScheduledAt:f}\nConsultation type: {appointment.ConsultationType}\nStatus: {appointment.Status}\nReason: {appointment.Reason}\nInsurance: {appointment.InsuranceInfo ?? "N/A"}\n\n{GetAppName()} team will keep you updated."
        };
        mail.To.Add(toEmail);

        await client.SendMailAsync(mail);
    }

    private SmtpClient CreateClient()
    {
        var host = _configuration["Smtp:Host"];
        var userName = _configuration["Smtp:UserName"];
        var password = _configuration["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("SMTP is not configured. Email was not sent.");
            throw new InvalidOperationException("SMTP is not configured.");
        }

        return new SmtpClient(host, _configuration.GetValue("Smtp:Port", 587))
        {
            EnableSsl = _configuration.GetValue("Smtp:EnableSsl", true),
            Credentials = new NetworkCredential(userName, password)
        };
    }

    private string GetSenderEmail()
    {
        var userName = _configuration["Smtp:UserName"];
        if (string.IsNullOrWhiteSpace(userName))
        {
            _logger.LogWarning("SMTP username is not configured. Email was not sent.");
            throw new InvalidOperationException("SMTP username is not configured.");
        }

        return userName;
    }

    private string GetAppName() => _configuration["AppName"] ?? "carevionix";
}
