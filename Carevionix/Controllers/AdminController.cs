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

[Authorize(Roles = RoleNames.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IInvoiceService _invoices;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public AdminController(ApplicationDbContext context, IInvoiceService invoices, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _context = context;
        _invoices = invoices;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Dashboard() => View(await BuildDashboardAsync());

    public async Task<IActionResult> Home() => View(await BuildDashboardAsync());

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException("User not found.");
        return View(new AdminProfileViewModel
        {
            Admin = user,
            Profile = new AdminProfileDto(user.FullName, user.Email ?? string.Empty, user.PhoneNumber, user.Address)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Profile([Bind(Prefix = "Profile")] AdminProfileDto dto)
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException("User not found.");
        if (!ModelState.IsValid)
        {
            return View(new AdminProfileViewModel { Admin = user, Profile = dto });
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;

        var result = await _userManager.UpdateAsync(user);
        TempData["StatusMessage"] = result.Succeeded
            ? "Admin profile updated."
            : string.Join(" ", result.Errors.Select(e => e.Description));
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
        var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException("User not found.");
        var uploadRoot = Path.Combine(_environment.WebRootPath, "ProfileImages");
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{user.Id}-{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(uploadRoot, fileName);
        await using (var stream = System.IO.File.Create(path))
        {
            await profileImage.CopyToAsync(stream);
        }

        user.ProfileImagePath = $"/ProfileImages/{fileName}";
        await _userManager.UpdateAsync(user);

        TempData["StatusMessage"] = "Profile image updated.";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> SystemControl()
    {
        var dashboard = await BuildDashboardAsync();
        return View(new AdminSystemControlViewModel
        {
            Doctors = dashboard.Doctors,
            Patients = dashboard.Patients,
            Appointments = dashboard.Appointments,
            Messages = dashboard.Messages,
            HealthRecords = dashboard.HealthRecords,
            ContactSubmissions = dashboard.ContactSubmissions,
            PendingAppointments = dashboard.PendingAppointments,
            CompletedConsultations = dashboard.CompletedConsultations,
            Revenue = dashboard.Revenue,
            ActiveDoctors = await _context.Doctors.CountAsync(d => d.IsActive && d.User.IsActive),
            InactiveDoctors = await _context.Doctors.CountAsync(d => !d.IsActive || !d.User.IsActive),
            ActivePatients = await _context.Patients.CountAsync(p => p.User.IsActive),
            InactivePatients = await _context.Patients.CountAsync(p => !p.User.IsActive),
            OpenContactSubmissions = await _context.ContactSubmissions.CountAsync(c => !c.IsResolved),
            ServerTimeUtc = DateTime.UtcNow
        });
    }

    public async Task<IActionResult> ManageDoctors() =>
        View(new AdminUsersViewModel<Doctor>
        {
            Items = await _context.Doctors.AsNoTracking().Include(d => d.User).OrderBy(d => d.User.FullName).ToListAsync()
        });

    [HttpPost]
    public async Task<IActionResult> SaveDoctor(AdminDoctorDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Doctor form has missing or invalid fields.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        Doctor doctor;
        if (dto.Id.HasValue)
        {
            doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == dto.Id.Value)
                ?? throw new KeyNotFoundException("Doctor not found.");
            doctor.User.Email = dto.Email;
            doctor.User.UserName = dto.Email;
            doctor.User.FullName = dto.FullName;
            doctor.User.PhoneNumber = dto.PhoneNumber;
            doctor.User.IsActive = dto.IsActive;
            await _userManager.UpdateAsync(doctor.User);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                TempData["StatusMessage"] = "A password is required when creating a new doctor.";
                return RedirectToAction(nameof(ManageDoctors));
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true,
                IsActive = dto.IsActive
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(ManageDoctors));
            }
            await _userManager.AddToRoleAsync(user, RoleNames.Doctor);
            doctor = new Doctor { UserId = user.Id };
            _context.Doctors.Add(doctor);
        }

        doctor.DoctorName = dto.FullName;
        doctor.Specialty = dto.Specialty;
        doctor.Location = dto.Location;
        doctor.Languages = dto.Languages;
        doctor.Qualifications = dto.Qualifications;
        doctor.Bio = dto.Bio;
        doctor.ExperienceYears = dto.ExperienceYears;
        doctor.ConsultationFee = dto.ConsultationFee;
        doctor.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Doctor saved.";
        return RedirectToAction(nameof(ManageDoctors));
    }

    [HttpPost]
    public async Task<IActionResult> ActivateDoctor(int id, bool isActive)
    {
        var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException("Doctor not found.");
        doctor.IsActive = isActive;
        doctor.User.IsActive = isActive;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = isActive ? "Doctor activated." : "Doctor deactivated.";
        return RedirectToAction(nameof(ManageDoctors));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException("Doctor not found.");
        if (await _context.Appointments.AnyAsync(a => a.DoctorId == id))
        {
            doctor.IsActive = false;
            doctor.User.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Doctor has appointment history, so the account was deactivated instead of permanently removed.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        await _userManager.DeleteAsync(doctor.User);
        TempData["StatusMessage"] = "Doctor removed.";
        return RedirectToAction(nameof(ManageDoctors));
    }

    public async Task<IActionResult> ManagePatients() =>
        View(new AdminUsersViewModel<Patient>
        {
            Items = await _context.Patients.AsNoTracking().Include(p => p.User).OrderBy(p => p.User.FullName).ToListAsync()
        });

    [HttpPost]
    public async Task<IActionResult> SavePatient(AdminPatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Patient form has missing or invalid fields.";
            return RedirectToAction(nameof(ManagePatients));
        }

        Patient patient;
        if (dto.Id.HasValue)
        {
            patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == dto.Id.Value)
                ?? throw new KeyNotFoundException("Patient not found.");
            patient.User.Email = dto.Email;
            patient.User.UserName = dto.Email;
            patient.User.FullName = dto.FullName;
            patient.User.PhoneNumber = dto.PhoneNumber;
            patient.User.IsActive = dto.IsActive;
            await _userManager.UpdateAsync(patient.User);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                TempData["StatusMessage"] = "A password is required when creating a new patient.";
                return RedirectToAction(nameof(ManagePatients));
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true,
                IsActive = dto.IsActive
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(ManagePatients));
            }
            await _userManager.AddToRoleAsync(user, RoleNames.Patient);
            patient = new Patient { UserId = user.Id };
            _context.Patients.Add(patient);
        }

        patient.PatientName = dto.FullName;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Gender = dto.Gender;
        patient.Address = dto.Address;
        patient.InsuranceInfo = dto.InsuranceInfo;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.EmergencyContact = dto.EmergencyContact;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Patient saved.";
        return RedirectToAction(nameof(ManagePatients));
    }

    [HttpPost]
    public async Task<IActionResult> ActivatePatient(int id, bool isActive)
    {
        var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Patient not found.");
        patient.User.IsActive = isActive;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = isActive ? "Patient activated." : "Patient deactivated.";
        return RedirectToAction(nameof(ManagePatients));
    }

    [HttpPost]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Patient not found.");
        if (await _context.Appointments.AnyAsync(a => a.PatientId == id))
        {
            patient.User.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Patient has appointment history, so the account was deactivated instead of permanently removed.";
            return RedirectToAction(nameof(ManagePatients));
        }

        await _userManager.DeleteAsync(patient.User);
        TempData["StatusMessage"] = "Patient removed.";
        return RedirectToAction(nameof(ManagePatients));
    }

    public async Task<IActionResult> ManageAppointments() =>
        View(await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Invoice)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync());

    public async Task<IActionResult> History(string? search, string status = "All", string invoice = "All")
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Invoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.PatientName.Contains(term) ||
                a.DoctorName.Contains(term) ||
                a.Patient.User.FullName.Contains(term) ||
                a.Doctor.User.FullName.Contains(term));
        }

        if (Enum.TryParse<AppointmentStatus>(status, true, out var appointmentStatus))
        {
            query = query.Where(a => a.Status == appointmentStatus);
        }

        query = invoice switch
        {
            "WithInvoice" => query.Where(a => a.Invoice != null),
            "NoInvoice" => query.Where(a => a.Invoice == null),
            "Paid" => query.Where(a => a.Invoice != null && a.Invoice.PaymentStatus == PaymentStatus.Approved),
            "WaitingPayment" => query.Where(a => a.Status == AppointmentStatus.Completed && (a.Invoice == null || a.Invoice.PaymentStatus != PaymentStatus.Approved)),
            _ => query
        };

        return View(new AdminAppointmentHistoryViewModel
        {
            Appointments = await query
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.ScheduledAt)
                .ToListAsync(),
            Search = search,
            Status = status,
            Invoice = invoice
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentStatus status)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Appointment not found.");
        if (status is AppointmentStatus.Accepted or AppointmentStatus.Rejected or AppointmentStatus.Cancelled)
        {
            if (appointment.Status == AppointmentStatus.Completed)
            {
                TempData["StatusMessage"] = "Completed appointments cannot be changed.";
                return RedirectToAction(nameof(ManageAppointments));
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Appointment {status}.";
        }
        return RedirectToAction(nameof(ManageAppointments));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateInvoice(GenerateInvoiceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var invoice = await _invoices.GenerateAsync(dto.AppointmentId, dto.InsuranceAmount, dto.Discount);
        TempData["StatusMessage"] = "Invoice generated.";
        return RedirectToAction(nameof(ManageAppointments));
    }

    [HttpPost]
    public async Task<IActionResult> ApproveInvoicePayment(ApproveInvoicePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Payment approval request is invalid.";
            return RedirectToAction(nameof(ManageAppointments));
        }

        await _invoices.ApproveDemoPaymentAsync(dto.InvoiceId);
        TempData["StatusMessage"] = "Demo payment approved and processed.";
        return RedirectToAction(nameof(ManageAppointments));
    }

    public async Task<IActionResult> ManageHealthRecords() =>
        View(await _context.HealthRecords.AsNoTracking().Include(r => r.Patient).ThenInclude(p => p.User).OrderByDescending(r => r.UploadedAt).ToListAsync());

    public async Task<IActionResult> DownloadHealthRecord(int id)
    {
        var record = await _context.HealthRecords.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Health record not found.");
        var path = Path.Combine(_environment.ContentRootPath, "SecureUploads", record.PatientId.ToString(), record.StoredFileName);
        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException("Stored file not found.");
        }

        return new FileStreamResult(System.IO.File.OpenRead(path), record.ContentType)
        {
            FileDownloadName = record.FileName
        };
    }

    public async Task<IActionResult> ContactSubmissions() =>
        View(await _context.ContactSubmissions.AsNoTracking().OrderByDescending(c => c.SubmittedAt).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> ResolveContactSubmission(int id, bool isResolved)
    {
        var submission = await _context.ContactSubmissions.FindAsync(id)
            ?? throw new KeyNotFoundException("Contact submission not found.");
        submission.IsResolved = isResolved;
        submission.ResolvedAt = isResolved ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = isResolved ? "Contact submission marked resolved." : "Contact submission reopened.";
        return RedirectToAction(nameof(ContactSubmissions));
    }

    public async Task<IActionResult> Analytics() => View(new AdminDashboardViewModel
    {
        CompletedConsultations = await _context.Appointments.CountAsync(a => a.Status == Models.AppointmentStatus.Completed),
        PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == Models.AppointmentStatus.Pending),
        Appointments = await _context.Appointments.CountAsync(),
        Doctors = await _context.Doctors.CountAsync(),
        Patients = await _context.Patients.CountAsync(),
        Revenue = await _context.Invoices.Where(i => i.PaymentStatus == PaymentStatus.Approved).SumAsync(i => (decimal?)i.Total) ?? 0,
    });

    public async Task<IActionResult> Reports() => View(await _context.Appointments
        .AsNoTracking()
        .GroupBy(a => a.Status)
        .Select(g => new ReportRowViewModel(g.Key.ToString(), g.Count()))
        .ToListAsync());

    private async Task<AdminDashboardViewModel> BuildDashboardAsync() => new()
    {
        Doctors = await _context.Doctors.CountAsync(),
        Patients = await _context.Patients.CountAsync(),
        Appointments = await _context.Appointments.CountAsync(),
        Messages = await _context.ChatMessages.CountAsync(),
        HealthRecords = await _context.HealthRecords.CountAsync(),
        ContactSubmissions = await _context.ContactSubmissions.CountAsync(),
        PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
        CompletedConsultations = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed),
        Revenue = await _context.Invoices.Where(i => i.PaymentStatus == PaymentStatus.Approved).SumAsync(i => (decimal?)i.Total) ?? 0
    };

}
