using Carevionix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Data;

public static class DbInitializer
{
    public static readonly string[] Roles = ["Patient", "Doctor", "Admin"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = configuration["AdminSeed:Email"] ?? "admin@carevionix.local";
        var password = configuration["AdminSeed:Password"];
        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Admin user was not seeded because AdminSeed:Password is not configured.");
                return;
            }

            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = configuration["AdminSeed:FullName"] ?? "Carevionix Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Admin seed failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var doctorsWithoutNames = await context.Doctors.Include(d => d.User).Where(d => d.DoctorName == string.Empty).ToListAsync();
        foreach (var doctor in doctorsWithoutNames)
        {
            doctor.DoctorName = doctor.User.FullName;
        }

        var patientsWithoutNames = await context.Patients.Include(p => p.User).Where(p => p.PatientName == string.Empty).ToListAsync();
        foreach (var patient in patientsWithoutNames)
        {
            patient.PatientName = patient.User.FullName;
        }

        if (doctorsWithoutNames.Count > 0 || patientsWithoutNames.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }
}
