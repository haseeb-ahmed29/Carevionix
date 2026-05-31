using Carevionix.Data;
using Carevionix.DTOs;
using Carevionix.Helpers;
using Carevionix.Models;
using Carevionix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Carevionix.Controllers;

public class AccountController : AppControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IEmailNotificationService emailNotificationService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _emailNotificationService = emailNotificationService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login() => View(new LoginDto(string.Empty, string.Empty, false));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // Unified Login Logic
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(dto);
        }

        var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(dto);
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains(RoleNames.Admin))
            return RedirectToAction("Dashboard", "Admin");

        if (roles.Contains(RoleNames.Doctor))
            return RedirectToAction("Dashboard", "Doctor");

        return RedirectToAction("Dashboard", "Patient");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterPatient()
    {
        return View(new RegisterPatientDto(string.Empty, string.Empty, string.Empty, null, null, null, null, null, null));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPatient(RegisterPatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(dto.Email), "This email is already registered.");
            return View(dto);
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(dto);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.Patient);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(dto);
        }

        _context.Patients.Add(new Patient
        {
            UserId = user.Id,
            PatientName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            InsuranceInfo = dto.InsuranceInfo,
            MedicalHistory = dto.MedicalHistory
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        TempData["StatusMessage"] = "Patient registration successful. Please login.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterDoctor()
    {
        TempData["StatusMessage"] = "Doctor accounts are created by admin only.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult RegisterDoctor(RegisterDoctorDto dto)
    {
        TempData["StatusMessage"] = "Doctor accounts are created by admin only.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("Admin/Login")]
    [AllowAnonymous]
    public IActionResult AdminLogin()
    {
        ViewData["IsAdminLogin"] = true;
        return View("Login", new LoginDto(string.Empty, string.Empty, false));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordDto(string.Empty));

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = GenerateTemporaryPassword();
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                try
                {
                    await _emailNotificationService.SendGeneratedPasswordAsync(user.Email ?? dto.Email, user.FullName, newPassword);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Password was reset for {Email}, but the generated password email could not be sent.", dto.Email);
                    TempData["StatusMessage"] = "Password reset was processed, but email could not be sent. Please contact Carevionix support.";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                _logger.LogWarning("Password reset failed for {Email}: {Errors}", dto.Email, string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }

        TempData["StatusMessage"] = "If an account exists for this email, a new password has been sent.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null, string? token = null) =>
        View(new ResetPasswordDto(email ?? string.Empty, token ?? string.Empty, string.Empty));

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid reset request.");
            return View(dto);
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(dto);
        }

        TempData["StatusMessage"] = "Password reset successful. Please login.";
        return RedirectToAction(nameof(Login));
    }

    private static string GenerateTemporaryPassword()
    {
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "23456789";
        const string symbols = "!@$?_-";
        const string all = lower + upper + digits + symbols;

        var chars = new[]
        {
            Pick(lower),
            Pick(upper),
            Pick(digits),
            Pick(symbols),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all)
        };

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
