using Microsoft.AspNetCore.Identity;

namespace Carevionix.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? PatientProfile { get; set; }
    public Doctor? DoctorProfile { get; set; }
}
