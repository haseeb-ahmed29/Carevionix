using System.Security.Claims;
using Carevionix.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Controllers;

public abstract class AppControllerBase : Controller
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    protected bool WantsJson()
    {
        var accept = Request.Headers.Accept.ToString();
        var contentType = Request.ContentType ?? string.Empty;
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    protected async Task<int> CurrentPatientIdAsync(ApplicationDbContext context)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
        if (patient is null)
        {
            throw new UnauthorizedAccessException("Patient profile not found.");
        }

        return patient.Id;
    }

    protected async Task<int> CurrentDoctorIdAsync(ApplicationDbContext context)
    {
        var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == CurrentUserId);
        if (doctor is null)
        {
            throw new UnauthorizedAccessException("Doctor profile not found.");
        }

        return doctor.Id;
    }
}
