using Carevionix.Data;
using Carevionix.DTOs;
using Carevionix.Helpers;
using Carevionix.Interfaces;
using Carevionix.Models;
using Carevionix.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Controllers;

[Authorize(Roles = RoleNames.Patient)]
public class PatientController : AppControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentService _appointments;
    private readonly IHealthRecordService _healthRecords;
    private readonly IPrescriptionService _prescriptions;
    private readonly IInvoiceService _invoices;
    private readonly INotificationService _notifications;
    private readonly IChatService _chat;
    private readonly IConsultationService _consultations;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientController(ApplicationDbContext context, IAppointmentService appointments, IHealthRecordService healthRecords, IPrescriptionService prescriptions, IInvoiceService invoices, INotificationService notifications, IChatService chat, IConsultationService consultations, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _appointments = appointments;
        _healthRecords = healthRecords;
        _prescriptions = prescriptions;
        _invoices = invoices;
        _notifications = notifications;
        _chat = chat;
        _consultations = consultations;
        _environment = environment;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var patientId = await CurrentPatientIdAsync(_context);
        var patient = await _context.Patients.Include(p => p.User).FirstAsync(p => p.Id == patientId);
        var model = new PatientDashboardViewModel
        {
            Patient = patient,
            Appointments = await _appointments.GetPatientAppointmentsAsync(patientId),
            Notifications = await _notifications.GetForPatientAsync(patientId)
        };

        return WantsJson() ? Ok(model) : View(model);
    }

    public IActionResult Home()
    {
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var patientId = await CurrentPatientIdAsync(_context);
        var patient = await _context.Patients.Include(p => p.User).FirstAsync(p => p.Id == patientId);
        var model = new PatientProfileViewModel
        {
            Patient = patient,
            Profile = new PatientProfileDto(
                patient.User.FullName,
                patient.User.PhoneNumber,
                patient.User.Address ?? patient.Address,
                patient.DateOfBirth,
                patient.Gender,
                patient.InsuranceInfo,
                patient.MedicalHistory,
                patient.EmergencyContact)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Profile([Bind(Prefix = "Profile")] PatientProfileDto dto)
    {
        var patientId = await CurrentPatientIdAsync(_context);
        var patient = await _context.Patients.Include(p => p.User).FirstAsync(p => p.Id == patientId);
        if (!ModelState.IsValid)
        {
            return View(new PatientProfileViewModel { Patient = patient, Profile = dto });
        }

        patient.User.FullName = dto.FullName;
        patient.User.PhoneNumber = dto.PhoneNumber;
        patient.User.Address = dto.Address;
        patient.PatientName = dto.FullName;
        patient.Address = dto.Address;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Gender = dto.Gender;
        patient.InsuranceInfo = dto.InsuranceInfo;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.EmergencyContact = dto.EmergencyContact;
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordDto dto)
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException("User not found.");
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Password form is incomplete.";
            return RedirectToAction(nameof(Profile));
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        TempData["StatusMessage"] = result.Succeeded
            ? "Password changed successfully."
            : string.Join(" ", result.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage)
    {
        if (!await FileUploadValidator.IsAllowedProfileImageAsync(profileImage))
        {
            TempData["StatusMessage"] = FileUploadValidator.ProfileImageError;
            return RedirectToAction(nameof(Profile));
        }

        var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
        var uploadRoot = Path.Combine(_environment.WebRootPath, "ProfileImages");
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{CurrentUserId}-{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(uploadRoot, fileName);
        await using (var stream = System.IO.File.Create(path))
        {
            await profileImage.CopyToAsync(stream);
        }

        var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException("User not found.");
        user.ProfileImagePath = $"/ProfileImages/{fileName}";
        await _userManager.UpdateAsync(user);

        TempData["StatusMessage"] = "Profile image updated.";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> FindDoctors([FromQuery] DoctorSearchDto filter)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Availabilities)
            .Include(d => d.Reviews)
            .Where(d => d.User.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Specialty))
        {
            query = query.Where(d => d.Specialty.Contains(filter.Specialty));
        }
        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            query = query.Where(d => d.Location != null && d.Location.Contains(filter.Location));
        }
        if (!string.IsNullOrWhiteSpace(filter.Language))
        {
            query = query.Where(d => d.Languages != null && d.Languages.Contains(filter.Language));
        }
        if (!string.IsNullOrWhiteSpace(filter.Availability) && DateTime.TryParse(filter.Availability, out var requestedAt))
        {
            var requestedTime = TimeOnly.FromDateTime(requestedAt);
            var requestedEnd = requestedAt.AddMinutes(30);
            query = query.Where(d =>
                d.Availabilities.Any(a =>
                    a.IsAvailable &&
                    a.DayOfWeek == requestedAt.DayOfWeek &&
                    a.StartTime <= requestedTime &&
                    a.EndTime > requestedTime) &&
                !d.Appointments.Any(a =>
                    a.ScheduledAt < requestedEnd &&
                    requestedAt < a.ScheduledAt.AddMinutes(30) &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Rejected));
        }

        var doctors = await query.OrderBy(d => d.User.FullName).ToListAsync();
        if (filter.MinRating.HasValue)
        {
            doctors = doctors
                .Where(d => d.Reviews.Count != 0 && d.Reviews.Average(r => r.Rating) >= (double)filter.MinRating.Value)
                .ToList();
        }

        var model = new FindDoctorsViewModel
        {
            Filter = filter,
            Doctors = doctors
                .OrderByDescending(d => d.Reviews.Count != 0 ? d.Reviews.Average(r => r.Rating) : 0)
                .ThenBy(d => d.User.FullName)
                .ToList()
        };

        return WantsJson() ? Ok(model.Doctors) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> BookAppointment(int? doctorId)
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Availabilities)
            .Where(d => d.User.IsActive)
            .OrderBy(d => d.User.FullName)
            .ToListAsync();
        var model = new BookAppointmentViewModel
        {
            Doctors = doctors,
            Appointment = new BookAppointmentDto(doctorId ?? 0, DateTime.Now.AddDays(1).Date.AddHours(9), ConsultationType.Video, string.Empty, null)
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment(BookAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            if (WantsJson())
            {
                return BadRequest(ModelState);
            }

            return View(new BookAppointmentViewModel
            {
                Appointment = dto,
                Doctors = await _context.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Availabilities)
                    .Where(d => d.IsActive)
                    .ToListAsync()
            });
        }

        var appointment = await _appointments.BookAsync(await CurrentPatientIdAsync(_context), dto);
        TempData["StatusMessage"] = "Appointment request created.";
        return WantsJson() ? Ok(appointment) : RedirectToAction(nameof(MyAppointments));
    }

    [HttpPost]
    public async Task<IActionResult> RescheduleAppointment(RescheduleAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var appointment = await _appointments.RescheduleAsync(await CurrentPatientIdAsync(_context), dto);
        TempData["StatusMessage"] = "Appointment rescheduled.";
        return WantsJson() ? Ok(appointment) : RedirectToAction(nameof(MyAppointments));
    }

    [HttpPost]
    public async Task<IActionResult> CancelAppointment(CancelAppointmentDto dto)
    {
        await _appointments.CancelAsync(await CurrentPatientIdAsync(_context), dto);
        TempData["StatusMessage"] = "Appointment cancelled.";
        return WantsJson() ? Ok(new { message = "Appointment cancelled." }) : RedirectToAction(nameof(MyAppointments));
    }

    public async Task<IActionResult> MyAppointments()
    {
        var appointments = await _appointments.GetPatientAppointmentsAsync(await CurrentPatientIdAsync(_context));
        return WantsJson() ? Ok(appointments) : View(appointments);
    }

    public async Task<IActionResult> VideoCalls()
    {
        var appointments = await _appointments.GetPatientAppointmentsAsync(await CurrentPatientIdAsync(_context));
        return WantsJson() ? Ok(appointments) : View(appointments);
    }

    public async Task<IActionResult> HealthRecords()
    {
        var records = await _healthRecords.GetHistoryAsync(await CurrentPatientIdAsync(_context));
        ViewBag.Doctors = await _context.Doctors.Include(d => d.User).Where(d => d.User.IsActive).OrderBy(d => d.User.FullName).ToListAsync();
        return WantsJson() ? Ok(records) : View(records);
    }

    [HttpPost]
    public async Task<IActionResult> UploadRecord([FromForm] HealthRecordType type, [FromForm] string title, [FromForm] IFormFile file, [FromForm] string? description)
    {
        var record = await _healthRecords.UploadAsync(await CurrentPatientIdAsync(_context), CurrentUserId, type, title, file, description);
        TempData["StatusMessage"] = "Health record uploaded.";
        return WantsJson() ? Ok(record) : RedirectToAction(nameof(HealthRecords));
    }

    [HttpPost]
    public async Task<IActionResult> ShareRecord(ShareHealthRecordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var record = await _healthRecords.ShareAsync(await CurrentPatientIdAsync(_context), dto.RecordId, dto.DoctorUserId, CurrentUserId, dto.ExpiresAt);
        TempData["StatusMessage"] = "Record shared.";
        return WantsJson() ? Ok(record) : RedirectToAction(nameof(HealthRecords));
    }

    [HttpGet]
    public IActionResult AiDiagnosis() => View(new AiDiagnosisViewModel());

    [HttpPost]
    public IActionResult AiDiagnosis(AiDiagnosisRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return WantsJson() ? BadRequest(ModelState) : View(new AiDiagnosisViewModel { Request = dto });
        }

        var result = BuildBasicDiagnosis(dto);
        return WantsJson() ? Ok(result) : View(new AiDiagnosisViewModel { Request = dto, Result = result });
    }

    [HttpPost]
    public async Task<IActionResult> RateDoctor(CreateDoctorReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var patientId = await CurrentPatientIdAsync(_context);
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == patientId && a.Status == AppointmentStatus.Completed)
            ?? throw new KeyNotFoundException("Completed appointment not found.");

        var review = await _context.DoctorReviews.FirstOrDefaultAsync(r => r.AppointmentId == appointment.Id && r.PatientId == patientId);
        if (review is null)
        {
            review = new DoctorReview
            {
                AppointmentId = appointment.Id,
                PatientId = patientId,
                DoctorId = appointment.DoctorId
            };
            _context.DoctorReviews.Add(review);
        }

        review.Rating = dto.Rating;
        review.ReviewText = dto.ReviewText;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Doctor review saved.";
        return WantsJson() ? Ok(review) : RedirectToAction(nameof(MyAppointments));
    }

    public async Task<IActionResult> DownloadRecord(int id) => await _healthRecords.DownloadAsync(await CurrentPatientIdAsync(_context), id);

    public async Task<IActionResult> Prescriptions()
    {
        var prescriptions = await _prescriptions.GetPatientPrescriptionsAsync(await CurrentPatientIdAsync(_context));
        return WantsJson() ? Ok(prescriptions) : View(prescriptions);
    }

    public async Task<IActionResult> Invoices()
    {
        var invoices = await _invoices.GetPatientInvoicesAsync(await CurrentPatientIdAsync(_context));
        if (TempData["DemoPaidInvoiceId"] is string paidInvoiceId && int.TryParse(paidInvoiceId, out var parsedInvoiceId))
        {
            ViewBag.DemoPaidInvoiceId = parsedInvoiceId;
        }

        ViewBag.DemoPaymentReceipt = TempData["DemoPaymentReceipt"] as string;
        return WantsJson() ? Ok(invoices) : View(invoices);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayInvoiceDemo(DemoInvoicePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Demo payment details are incomplete.";
            return RedirectToAction(nameof(Invoices));
        }

        var patientId = await CurrentPatientIdAsync(_context);
        var invoice = await _invoices.SubmitDemoPaymentAsync(patientId, dto.InvoiceId, dto.PayerName, dto.DemoMethod);
        TempData["DemoPaidInvoiceId"] = invoice.Id.ToString();
        TempData["DemoPaymentReceipt"] = invoice.DemoPaymentReceipt;
        TempData["StatusMessage"] = $"Demo payment sent for invoice #{invoice.Id}. Receipt: {invoice.DemoPaymentReceipt}.";
        return RedirectToAction(nameof(Invoices));
    }

    public async Task<IActionResult> Notifications()
    {
        var notifications = await _notifications.GetForPatientAsync(await CurrentPatientIdAsync(_context));
        return WantsJson() ? Ok(notifications) : View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        var notification = await _notifications.MarkAsReadAsync(await CurrentPatientIdAsync(_context), id);
        return WantsJson() ? Ok(notification) : RedirectToAction(nameof(Notifications));
    }

    [HttpGet]
    public async Task<IActionResult> Chat(string doctorUserId)
    {
        await EnsureDoctorUserAsync(doctorUserId, await CurrentPatientIdAsync(_context));
        var messages = await _chat.ConversationAsync(CurrentUserId, doctorUserId);
        return WantsJson() ? Ok(messages) : View(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Chat(SendChatMessageDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await EnsureDoctorUserAsync(dto.ReceiverId, await CurrentPatientIdAsync(_context));
        var message = await _chat.SendAsync(CurrentUserId, dto.ReceiverId, dto.Message);
        return WantsJson() ? Ok(message) : RedirectToAction(nameof(Chat), new { doctorUserId = dto.ReceiverId });
    }

    [HttpPost]
    public async Task<IActionResult> VideoConsultation(int appointmentId)
    {
        var patientId = await CurrentPatientIdAsync(_context);
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);
        if (appointment is null)
        {
            return NotFound("Appointment not found.");
        }

        var consultation = await _consultations.StartAsync(appointmentId, appointment.ConsultationType);
        return WantsJson() ? Ok(consultation) : View(consultation);
    }

    private async Task EnsureDoctorUserAsync(string doctorUserId, int patientId)
    {
        var doctorExists = await _context.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.Doctor.UserId == doctorUserId &&
            a.Doctor.User.IsActive &&
            (a.Status == AppointmentStatus.Accepted || a.Status == AppointmentStatus.Completed));
        if (!doctorExists)
        {
            throw new KeyNotFoundException("Doctor not found.");
        }
    }

    private static AiDiagnosisResultDto BuildBasicDiagnosis(AiDiagnosisRequestDto dto)
    {
        var symptoms = dto.Symptoms.ToLowerInvariant();
        var emergencyTerms = new[] { "chest pain", "shortness of breath", "faint", "stroke", "severe bleeding", "unconscious" };
        var urgent = emergencyTerms.Any(symptoms.Contains);
        var feverish = symptoms.Contains("fever") || symptoms.Contains("temperature");
        var respiratory = symptoms.Contains("cough") || symptoms.Contains("sore throat") || symptoms.Contains("flu");
        var stomach = symptoms.Contains("vomit") || symptoms.Contains("diarrhea") || symptoms.Contains("stomach");

        var condition = urgent
            ? "Possible urgent/emergency symptoms"
            : feverish && respiratory
                ? "Possible viral respiratory infection"
                : stomach
                    ? "Possible gastrointestinal upset"
                    : "General symptoms requiring clinical review";

        var recommendations = urgent
            ? new[] { "Seek emergency care immediately.", "Do not wait for a routine virtual appointment." }
            : new[] { "Book a consultation for confirmation.", "Rest, hydrate, and monitor symptoms.", "Upload relevant reports or prescriptions before the visit." };

        return new AiDiagnosisResultDto(
            $"Symptoms reported for {dto.DurationDays} day(s).",
            condition,
            urgent ? "Emergency" : dto.DurationDays >= 7 ? "Soon" : "Routine",
            recommendations,
            "This is a basic preliminary screening and not a medical diagnosis.");
    }
}
