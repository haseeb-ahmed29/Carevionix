using Carevionix.Data;
using Carevionix.Interfaces;
using Carevionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;

    public ChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage> SendAsync(string senderId, string receiverId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Message is required.");
        }

        if (senderId == receiverId)
        {
            throw new InvalidOperationException("Sender and receiver must be different users.");
        }

        var usersExist = await _context.Users.CountAsync(u => u.Id == senderId || u.Id == receiverId);
        if (usersExist != 2)
        {
            throw new KeyNotFoundException("Sender or receiver not found.");
        }

        var chatMessage = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Message = message
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();
        return chatMessage;
    }

    public async Task<IReadOnlyList<ChatMessage>> ConversationAsync(string currentUserId, string otherUserId) =>
        await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
}
