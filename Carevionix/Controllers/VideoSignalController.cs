using System.Collections.Concurrent;
using System.Security.Claims;
using Carevionix.Data;
using Carevionix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Controllers;

[Authorize]
public class VideoSignalController : Controller
{
    private static readonly ConcurrentDictionary<string, List<VideoSignalMessage>> Rooms = new();
    private static long _signalId;
    private readonly ApplicationDbContext _context;

    public VideoSignalController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Poll(string room, long after = 0)
    {
        if (!await CanAccessRoomAsync(room))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var messages = Rooms.GetOrAdd(room, _ => []);
        lock (messages)
        {
            return Ok(messages
                .Where(m => m.Id > after && m.SenderId != userId)
                .OrderBy(m => m.Id)
                .Take(40)
                .ToList());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendVideoSignalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Room) || string.IsNullOrWhiteSpace(dto.Type))
        {
            return BadRequest("Room and signal type are required.");
        }

        if (!await CanAccessRoomAsync(dto.Room))
        {
            return Forbid();
        }

        var messages = Rooms.GetOrAdd(dto.Room, _ => []);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var message = new VideoSignalMessage(
            Interlocked.Increment(ref _signalId),
            userId,
            User.Identity?.Name ?? "Participant",
            dto.Type,
            dto.Payload ?? "{}",
            DateTime.UtcNow);

        lock (messages)
        {
            messages.Add(message);
            messages.RemoveAll(m => m.CreatedAt < DateTime.UtcNow.AddHours(-2));
        }

        return Ok(message);
    }

    private async Task<bool> CanAccessRoomAsync(string room)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(room))
        {
            return false;
        }

        return await _context.Consultations
            .Include(c => c.Appointment).ThenInclude(a => a.Patient)
            .Include(c => c.Appointment).ThenInclude(a => a.Doctor)
            .AnyAsync(c =>
                c.SessionReference == room &&
                c.Appointment.Status != AppointmentStatus.Cancelled &&
                c.Appointment.Status != AppointmentStatus.Rejected &&
                (c.Appointment.Patient.UserId == userId || c.Appointment.Doctor.UserId == userId));
    }

    public record SendVideoSignalDto(string Room, string Type, string? Payload);
    private record VideoSignalMessage(long Id, string SenderId, string SenderName, string Type, string Payload, DateTime CreatedAt);
}
