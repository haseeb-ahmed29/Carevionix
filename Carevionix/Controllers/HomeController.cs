using Carevionix.Data;
using Carevionix.DTOs;
using Carevionix.Models;
using Carevionix.Services;
using Carevionix.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Carevionix.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IMemoryCache _cache;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IEmailNotificationService emailNotificationService,
            IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _emailNotificationService = emailNotificationService;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _cache.GetOrCreateAsync("home:index:stats", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                return new HomeIndexViewModel
                {
                    DatabaseConnected = true,
                    ActiveDoctors = await _context.Doctors.AsNoTracking().CountAsync(d => d.IsActive),
                    Patients = await _context.Patients.AsNoTracking().CountAsync(),
                    Appointments = await _context.Appointments.AsNoTracking().CountAsync(),
                    CompletedConsultations = await _context.Appointments.AsNoTracking().CountAsync(a => a.Status == AppointmentStatus.Completed)
                };
            }) ?? new HomeIndexViewModel();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About() => View();

        public IActionResult Services() => View();

        [HttpGet]
        public IActionResult Contact() => View(new ContactSubmissionDto(string.Empty, string.Empty, null, string.Empty, string.Empty));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactSubmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            _context.ContactSubmissions.Add(new ContactSubmission
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Subject = dto.Subject,
                Message = dto.Message
            });
            await _context.SaveChangesAsync();
            try
            {
                await _emailNotificationService.SendContactNotificationAsync(dto.Name, dto.Email, dto.Subject, dto.Message, dto.PhoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Contact submission was saved, but the email notification could not be sent.");
            }
            TempData["StatusMessage"] = "Thanks for contacting Carevionix. Your message has been saved.";
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                title = "Request failed",
                detail = "An error occurred while processing the request.",
                requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
