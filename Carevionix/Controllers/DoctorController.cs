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

[Authorize(Roles = RoleNames.Doctor)]
public class DoctorController : AppControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentService _appointments;
    private readonly IPrescriptionService _prescriptions;
    private readonly IChatService _chat;
    private readonly IConsultationService _consultations;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorController(ApplicationDbContext context, IAppointmentService appointments, IPrescriptionService prescriptions, IChatService chat, IConsultationService consultations, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _appointments = appointments;
        _prescriptions = prescriptions;
        _chat = chat;
        _consultations = consultations;
        _environment = environment;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Availabilities)
            .FirstAsync(d => d.Id == doctorId);
        var model = new DoctorDashboardViewModel
        {
            Doctor = doctor,
            Appointments = await _appointments.GetDoctorAppointmentsAsync(doctorId)
        };

        return WantsJson() ? Ok(model) : View(model);
    }

    public IActionResult Home() => RedirectToAction(nameof(Dashboard));

    public async Task<IActionResult> Appointments()
    {
        var appointments = await _appointments.GetDoctorAppointmentsAsync(await CurrentDoctorIdAsync(_context));
        return WantsJson() ? Ok(appointments) : View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var doctor = await _context.Doctors.Include(d => d.User).FirstAsync(d => d.Id == doctorId);
        var model = new DoctorProfileViewModel
        {
            Doctor = doctor,
            Profile = new DoctorProfileDto(
                doctor.User.FullName, doctor.User.PhoneNumber, doctor.User.Address,
                doctor.Specialty, doctor.Location, doctor.Languages,
                doctor.Qualifications, doctor.Bio, doctor.ExperienceYears, doctor.ConsultationFee)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Profile([Bind(Prefix = "Profile")] DoctorProfileDto dto)
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var doctor = await _context.Doctors.Include(d => d.User).FirstAsync(d => d.Id == doctorId);
        if (!ModelState.IsValid) return View(new DoctorProfileViewModel { Doctor = doctor, Profile = dto });

        doctor.User.FullName = dto.FullName;
        doctor.User.PhoneNumber = dto.PhoneNumber;
        doctor.User.Address = dto.Address;
        doctor.DoctorName = dto.FullName;
        doctor.Specialty = dto.Specialty;
        doctor.Location = dto.Location;
        doctor.Languages = dto.Languages;
        doctor.Qualifications = dto.Qualifications;
        doctor.Bio = dto.Bio;
        doctor.ExperienceYears = dto.ExperienceYears;
        doctor.ConsultationFee = dto.ConsultationFee;
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
        TempData["StatusMessage"] = result.Succeeded ? "Password changed successfully." : string.Join(" ", result.Errors.Select(e => e.Description));
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

    public async Task<IActionResult> VideoCalls()
    {
        var appointments = await _appointments.GetDoctorAppointmentsAsync(await CurrentDoctorIdAsync(_context));
        return WantsJson() ? Ok(appointments) : View(appointments);
    }

    [HttpPost]
    public IActionResult AcceptAppointment(int appointmentId)
    {
        TempData["StatusMessage"] = "Appointment approval is handled by admin.";
        return WantsJson() ? Ok(new { appointmentId, message = "Admin approval is required." }) : RedirectToAction(nameof(Appointments));
    }

    [HttpPost]
    public async Task<IActionResult> RejectAppointment(int appointmentId)
    {
        var appointment = await _appointments.UpdateStatusAsync(await CurrentDoctorIdAsync(_context), appointmentId, AppointmentStatus.Rejected);
        TempData["StatusMessage"] = "Appointment rejected.";
        return WantsJson() ? Ok(appointment) : RedirectToAction(nameof(Appointments));
    }

    [HttpPost]
    public async Task<IActionResult> CompleteAppointment(CompleteAppointmentDto dto) => await CompleteAppointmentInternal(dto);

    public async Task<IActionResult> MyPatients()
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var patientIds = await _context.Appointments.Where(a => a.DoctorId == doctorId).Select(a => a.PatientId).Distinct().ToListAsync();
        var patients = await _context.Patients.Include(p => p.User).Where(p => patientIds.Contains(p.Id)).ToListAsync();
        return WantsJson() ? Ok(patients) : View(patients);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrescription(CreatePrescriptionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var prescription = await _prescriptions.CreateAsync(await CurrentDoctorIdAsync(_context), dto);
        TempData["StatusMessage"] = "Prescription saved and patient notified.";
        return WantsJson() ? Ok(prescription) : RedirectToAction(nameof(Appointments));
    }

    public async Task<IActionResult> SharedRecords()
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var records = await _context.HealthRecordShares
            .Include(s => s.HealthRecord).Include(s => s.Patient).ThenInclude(p => p.User)
            .Where(s => s.DoctorId == doctorId && s.ConsentActive && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(s => s.SharedAt).ToListAsync();
        return WantsJson() ? Ok(records) : View(records);
    }

    public async Task<IActionResult> DownloadSharedRecord(int shareId)
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var share = await _context.HealthRecordShares
            .Include(s => s.HealthRecord)
            .FirstOrDefaultAsync(s => s.Id == shareId && s.DoctorId == doctorId && s.ConsentActive && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            ?? throw new KeyNotFoundException("Shared health record not found.");

        var path = Path.Combine(_environment.ContentRootPath, "SecureUploads", share.PatientId.ToString(), share.HealthRecord.StoredFileName);
        if (!System.IO.File.Exists(path)) throw new FileNotFoundException("Stored file not found.");

        return new FileStreamResult(System.IO.File.OpenRead(path), share.HealthRecord.ContentType) { FileDownloadName = share.HealthRecord.FileName };
    }

    [HttpGet]
    public async Task<IActionResult> Chat(string patientUserId)
    {
        await EnsurePatientUserAsync(patientUserId, await CurrentDoctorIdAsync(_context));
        var messages = await _chat.ConversationAsync(CurrentUserId, patientUserId);
        return WantsJson() ? Ok(messages) : View(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Chat(SendChatMessageDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await EnsurePatientUserAsync(dto.ReceiverId, await CurrentDoctorIdAsync(_context));
        var message = await _chat.SendAsync(CurrentUserId, dto.ReceiverId, dto.Message);
        return WantsJson() ? Ok(message) : RedirectToAction(nameof(Chat), new { patientUserId = dto.ReceiverId });
    }

    [HttpPost]
    public async Task<IActionResult> VideoConsultation(int appointmentId)
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var ownsAppointment = await _context.Appointments.AnyAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);
        if (!ownsAppointment) return NotFound("Appointment not found.");
        var appointment = await _context.Appointments.FirstAsync(a => a.Id == appointmentId);
        var consultation = await _consultations.StartAsync(appointmentId, appointment.ConsultationType);
        return WantsJson() ? Ok(consultation) : View(consultation);
    }

    [HttpGet]
    public async Task<IActionResult> Availability()
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var availability = await _context.DoctorAvailabilities.Where(a => a.DoctorId == doctorId).OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime).ToListAsync();
        return WantsJson() ? Ok(availability) : View(availability);
    }

    [HttpPost]
    public async Task<IActionResult> Availability([FromForm] UpsertAvailabilityDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.EndTime <= dto.StartTime) return BadRequest("Availability end time must be after start time.");
        var doctorId = await CurrentDoctorIdAsync(_context);
        var days = dto.Days is { Count: > 0 } ? dto.Days.Distinct().ToList() : [dto.DayOfWeek];
        foreach (var day in days)
        {
            _context.DoctorAvailabilities.Add(new DoctorAvailability { DoctorId = doctorId, DayOfWeek = day, StartTime = dto.StartTime, EndTime = dto.EndTime, IsAvailable = dto.IsAvailable });
        }
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Availability saved.";
        return WantsJson() ? Ok(new { days }) : RedirectToAction(nameof(Availability));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAvailability(int id)
    {
        var doctorId = await CurrentDoctorIdAsync(_context);
        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);
        if (slot is null) return NotFound("Availability slot not found.");
        _context.DoctorAvailabilities.Remove(slot);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Availability removed.";
        return WantsJson() ? Ok(new { id }) : RedirectToAction(nameof(Availability));
    }

    private async Task<IActionResult> CompleteAppointmentInternal(CompleteAppointmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var doctorId = await CurrentDoctorIdAsync(_context);
        var ownsAppointment = await _context.Appointments.AnyAsync(a => a.Id == dto.AppointmentId && a.DoctorId == doctorId);
        if (!ownsAppointment) return NotFound("Appointment not found.");
        var consultation = await _consultations.CompleteAsync(dto.AppointmentId, dto.Notes);
        TempData["StatusMessage"] = "Consultation completed. Admin can generate the invoice now.";
        return WantsJson() ? Ok(consultation) : RedirectToAction(nameof(Appointments));
    }

    private async Task EnsurePatientUserAsync(string patientUserId, int doctorId)
    {
        var patientExists = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.Patient.UserId == patientUserId &&
            a.Patient.User.IsActive &&
            (a.Status == AppointmentStatus.Accepted || a.Status == AppointmentStatus.Completed));
        if (!patientExists) throw new KeyNotFoundException("Patient not found.");
    }
}
